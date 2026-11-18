# Le Grand Cinema - Deployment (MVP)

## Architecture
- Single container: Angular 20 static build hosted by ASP.NET Core 10.
- Backend API: `/api/...` (same origin as Angular; no production proxy).
- Public HTTPS endpoint: Azure Container Apps, container target port 8080.
- PostgreSQL: external Neon database (persistent storage).

## Build image locally
From repository root:

```powershell
docker version
docker build -t le-grand-cinema:local .
```

## IMPORTANT: Configure ASP.NET Core static hosting
In `backend/Cinema.Api/Program.cs`:
- Add `app.UseDefaultFiles();` then `app.UseStaticFiles();` BEFORE `app.MapControllers();`.
- Add `app.MapFallbackToFile("index.html");` AFTER `app.MapControllers();`.
- For an evaluator-accessible demo, put `app.MapOpenApi()` and `app.UseSwaggerUI(...)` outside `if (app.Environment.IsDevelopment())`; consider restricting API docs after evaluation.
- Keep `app.UseAuthentication();` before `app.UseAuthorization();`.

## Azure Container Apps requirements
- External HTTP ingress, container target port 8080. External endpoint uses HTTPS.
- Secret `cinema-db-connection` referenced as environment variable `ConnectionStrings__DefaultConnection`.
- Secret `cinema-jwt-key` referenced as environment variable `Jwt__Key`.
- Never commit secrets, passwords or production connection strings.
- Rotate the weak demo Admin password BEFORE making the service public.
- Database migrations were already applied to Neon; don't run automatic migrations on each container startup.

## Smoke tests after deployment
- Open `/` (Angular frontend).
- Open `/films` (Angular deep link and refresh).
- Open `/api/films` (5 demo films expected).
- Open `/swagger` if intentionally enabled for the evaluator.
- Verify User login, seat booking, ticket, Admin pages, and permissions.
