param(
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http

Write-Host "Concurrent booking against $BaseUrl"
Write-Host "This creates one real demo reservation on the configured database."

$email = Read-Host "Email of a regular User (not Admin)"
$securePassword = Read-Host "Password" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
}

$loginBody = @{ email = $email; password = $password } | ConvertTo-Json
Remove-Variable password -ErrorAction SilentlyContinue
$auth = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
Remove-Variable loginBody -ErrorAction SilentlyContinue

$seances = Invoke-RestMethod -Uri "$BaseUrl/api/seances"
Write-Host "First Seance ID: $($seances[0].id)"
$seance = $null
$seat = $null

foreach ($candidate in $seances) {
    $availability = Invoke-RestMethod -Uri "$BaseUrl/api/seances/$($candidate.id)/seats"
    $available = @($availability.seats | Where-Object { $_.isAvailable })
    if ($available.Count -gt 0) {
        $seance = $candidate
        $seat = $available[0]
        break
    }
}

if ($null -eq $seance -or $null -eq $seat) {
    throw "No future screening with available seats was found."
}

Write-Host "SeanceId: $($seance.id), SeatId: $($seat.seatId)"
$json = @{ seanceId = [int]$seance.id; seatIds = @([int]$seat.seatId) } | ConvertTo-Json -Compress

$client = [System.Net.Http.HttpClient]::new()
$client.DefaultRequestHeaders.Authorization =
    [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $auth.token)

$requestA = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post, "$BaseUrl/api/reservations")
$requestB = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Post, "$BaseUrl/api/reservations")
$requestA.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, "application/json")
$requestB.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, "application/json")

try {
    # Both HTTP requests are started before waiting on either response.
    $taskA = $client.SendAsync($requestA)
    $taskB = $client.SendAsync($requestB)
    $responseA = $taskA.GetAwaiter().GetResult()
    $responseB = $taskB.GetAwaiter().GetResult()

    $statusA = [int]$responseA.StatusCode
    $statusB = [int]$responseB.StatusCode
    Write-Host "Response A: HTTP $statusA"
    Write-Host "Response B: HTTP $statusB"

    $availabilityAfter = Invoke-RestMethod -Uri "$BaseUrl/api/seances/$($seance.id)/seats"
    $seatAfter = @($availabilityAfter.seats | Where-Object { $_.seatId -eq $seat.seatId })[0]
    Write-Host "Seat now available: $($seatAfter.isAvailable)"

    $statuses = @($statusA, $statusB) | Sort-Object
    if ($statuses[0] -eq 201 -and $statuses[1] -eq 409 -and $seatAfter.isAvailable -eq $false) {
        Write-Host "PASS: one reservation accepted, the other rejected, seat reserved."
    } else {
        Write-Host "FAIL: expected HTTP 201 + HTTP 409 and isAvailable=false."
        Write-Host "Check API logs before relying on this as concurrency evidence."
        exit 1
    }
} finally {
    $requestA.Dispose()
    $requestB.Dispose()
    $client.Dispose()
}
