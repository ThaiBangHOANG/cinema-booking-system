# US04 - Reserver des places

## User story
En tant que spectateur authentifie, je souhaite reserver une ou plusieurs
places disponibles afin d'obtenir un billet numerique.

## Acceptance criteria
- Anonymous POST /api/reservations is rejected (401).
- A user can register and log in to obtain a JWT.
- A logged-in user can reserve one or more seats (201).
- Seat IDs must belong to the screening's room (400 otherwise).
- Past screenings cannot be booked (400).
- Duplicate seat IDs in a request are rejected (400).
- Unknown screening returns 404.
- The same seat cannot be booked twice for the same screening (409).
- PostgreSQL unique index IX_ReservationSeats_SeanceId_SeatId is the final
  safeguard even under concurrent requests.

## Architecture
ReservationsController -> ReservationService -> CinemaDbContext -> PostgreSQL

## Security
User ID is read from a validated JWT, never accepted from a client request.
Public registration creates User-role accounts only (never Admin).
Passwords are stored as hashes (ASP.NET Core PasswordHasher).
Jwt:Key and database credentials must not be committed to Git.

## Known MVP scope
The response contains TicketCode. A ticket retrieval endpoint is planned
for US05. Frontend login, token handling, and admin onboarding are separate
work items. API authentication and authorization are enabled for bookings.
