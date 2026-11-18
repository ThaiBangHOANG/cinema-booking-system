
# Architecture - Le Grand Cinema

## Stack technique
- Frontend: Angular 20
- Backend: ASP.NET Core 10 Web API
- ORM: Entity Framework Core 10
- Database: PostgreSQL
- Authentication: JWT
- API documentation: Swagger / OpenAPI
- Testing: xUnit, Coverlet
- Code quality: static analysis
- Cloud: Azure Container Apps + Neon PostgreSQL

## Repository structure

cinema-booking-system/
  backend/
  frontend/
  tests/
  docs/
  Dockerfile
  README.md

## Architecture

- REST API with Controllers, Services and DTOs.
- Entity Framework Core for database access.
- Angular communicates with the REST API.
- Admin endpoints protected by role-based authorization.
- Passwords are hashed, never stored in plaintext.
- Database connection secrets are not committed.

## Business rules

- Users can browse films and screenings.
- Users can view available seats.
- Users can reserve one or more seats.
- A seat cannot be booked twice for the same screening.
- A database unique constraint and transaction protect bookings.
- Users can access a digital ticket.
- Administrators manage screenings and view statistics.

## Deployment

- One Docker image with ASP.NET Core API and Angular static files.
- Azure Container Apps provides the public HTTPS endpoint.
- PostgreSQL is hosted externally with persistent storage.
- Secrets are configured using environment variables.
- Deployment must be tested before final submission.

## Git

- main: stable version
- develop: integration
- feature/USXX-description: User Story development
- Pull Requests target develop.
