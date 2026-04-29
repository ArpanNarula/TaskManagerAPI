# TaskManagerAPI

A production-ready ASP.NET Core 8 backend API for task management, built with Clean Architecture, JWT authentication, PostgreSQL, Docker, Render, and Supabase.

This project is a **backend REST API**, not a frontend website. The live deployment exposes JSON endpoints and Swagger documentation for testing authentication and task CRUD flows.

## Live Demo

| Resource | URL |
|---|---|
| Live API | https://taskmanagerapi-juxj.onrender.com |
| Swagger UI | https://taskmanagerapi-juxj.onrender.com/swagger |
| Health Check | https://taskmanagerapi-juxj.onrender.com/health |

> Render free services may sleep after inactivity. If the first request is slow, wait a few seconds and refresh.

## Project Summary

TaskManagerAPI is a cloud-deployed backend service that demonstrates real-world API design. It supports user registration, login, JWT-based authentication, role-aware task access, task CRUD operations, pagination, filtering, searching, global exception handling, Swagger documentation, and PostgreSQL persistence.

The API is deployed on **Render** and connected to **Supabase PostgreSQL**. The application is containerized with Docker and uses Entity Framework Core migrations to manage the database schema.

## Tech Stack

| Area | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| Language | C# |
| Architecture | Clean Architecture |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| PostgreSQL Provider | Npgsql |
| Authentication | JWT Bearer |
| Password Hashing | BCrypt |
| API Docs | Swagger / OpenAPI |
| Hosting | Render |
| Database Hosting | Supabase |
| Containerization | Docker |

---

## Architecture Overview

```
TaskManagerAPI/
├── TaskManagerAPI.Core/              # Domain layer — zero framework dependencies
│   ├── Entities/                     # ApplicationUser, TaskItem
│   ├── Enums/                        # TaskItemStatus, TaskPriority
│   ├── DTOs/                         # Auth + Task request/response shapes
│   ├── Interfaces/                   # IRepository<T>, ITaskRepository, IUserRepository
│   │                                 # IAuthService, ITaskService, IJwtService
│   └── Common/                       # ApiResponse<T>, PagedResult<T>
│
├── TaskManagerAPI.Infrastructure/    # Data + service implementations
│   ├── Data/                         # AppDbContext (EF Core Code First)
│   ├── Repositories/                 # Repository<T>, TaskRepository, UserRepository
│   ├── Services/                     # AuthService, TaskService, JwtService
│   └── Migrations/                   # EF Core auto-generated migrations
│
└── TaskManagerAPI.API/               # HTTP layer — controllers, middleware, DI wiring
    ├── Controllers/                  # AuthController, TasksController
    ├── Middleware/                   # ExceptionMiddleware (global error handling)
    ├── Extensions/                   # ServiceExtensions (DI grouped by concern)
    └── Program.cs                    # App bootstrap — stays under 30 lines
```

### Design Decisions

| Decision | Rationale |
|---|---|
| Core has no NuGet refs | Domain stays testable without spinning up EF/ASP.NET |
| Generic `Repository<T>` | Eliminates boilerplate; entity repos only override what differs |
| Enums stored as strings | SQL is readable; `"Pending"` beats `0` for debugging and reporting |
| `ApiResponse<T>` wrapper | Clients always get a consistent `{ success, message, data }` shape |
| `PagedResult<T>` | Every list endpoint includes pagination metadata — clients never have to guess |
| `ExceptionMiddleware` first | Catches exceptions from routing, model binding, AND controller actions |
| `AsNoTracking()` on reads | Skips EF change-tracking overhead for read-only queries |
| BCrypt work factor 12 | Balances security and login latency (~250ms on modern hardware) |
| `ClockSkew = Zero` | JWT expiry is exact; default 5-min leeway is a security risk |

---

## What This Project Demonstrates

- Clean Architecture with separated Core, Infrastructure, and API layers.
- Secure authentication using JWT bearer tokens.
- Password hashing using BCrypt with work factor 12.
- Role-aware authorization handled inside the service layer.
- CRUD APIs for task management.
- Pagination, filtering, searching, and sorting for task lists.
- Consistent response shape using `ApiResponse<T>`.
- Global exception handling through custom middleware.
- PostgreSQL integration using EF Core and Npgsql.
- Docker-based deployment on Render.
- Supabase PostgreSQL as the cloud database.
- Swagger UI for live API testing and documentation.

---

## How to Demo

Open the deployed Swagger UI:

```text
https://taskmanagerapi-juxj.onrender.com/swagger
```

Demo flow:

1. Use `POST /api/Auth/register` to create a user.
2. Use `POST /api/Auth/login` to log in.
3. Copy the JWT token from the login response.
4. Click the Swagger **Authorize** button.
5. Paste the token in this format:

```text
Bearer YOUR_TOKEN_HERE
```

6. Test protected task endpoints:
   - `POST /api/Tasks`
   - `GET /api/Tasks`
   - `PUT /api/Tasks/{id}`
   - `DELETE /api/Tasks/{id}`

The JWT token is not a paid token. It is generated by this API after login and is used as proof that the user is authenticated.

---

## Example Demo Payloads

### Register

```json
{
  "userName": "demo",
  "email": "demo@example.com",
  "password": "Demo123!"
}
```

### Login

```json
{
  "email": "demo@example.com",
  "password": "Demo123!"
}
```

### Create Task

```json
{
  "title": "Prepare interview demo",
  "description": "Show deployed backend API with JWT auth",
  "priority": "High"
}
```

---

## Important Note

This repository contains the backend API only. A full product could add a React, Next.js, Angular, or mobile frontend that consumes this API. The current deployed interface is Swagger, which is used for testing and documenting backend endpoints.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL (local Docker, Supabase, Neon, or any managed Postgres)
- (Optional) Visual Studio 2022 / VS Code with C# extension

---

## Setup Instructions

### 1. Clone and restore

```bash
git clone https://github.com/ArpanNarula/TaskManagerAPI.git
cd TaskManagerAPI
dotnet restore
```

### 2. Configure the database

Edit `TaskManagerAPI.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanagerdb_dev;Username=postgres;Password=postgres"
  }
}
```

**Docker Postgres (quickest local setup):**

```bash
docker run --name taskmanager-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=taskmanagerdb_dev \
  -p 5432:5432 \
  -d postgres:16
```

For Supabase, use the pooled Postgres connection string and set it as `ConnectionStrings__DefaultConnection` in your host environment. Supabase connections usually require SSL, for example: `Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true`.

### 3. Set JWT secret

**Never commit secrets to source control.** Use User Secrets or environment variables:

```bash
cd TaskManagerAPI.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "your-minimum-32-character-secret-key-here"
```

Or set as environment variable:

```bash
export JwtSettings__Secret="your-minimum-32-character-secret-key-here"
```

### 4. Apply migrations

```bash
# From the solution root
dotnet ef database update --project TaskManagerAPI.Infrastructure --startup-project TaskManagerAPI.API
```

To create a new migration after changing entities:

```bash
dotnet ef migrations add YourMigrationName \
  --project TaskManagerAPI.Infrastructure \
  --startup-project TaskManagerAPI.API
```

### 5. Run the API

```bash
dotnet run --project TaskManagerAPI.API
```

Swagger UI: `https://localhost:7000/swagger` (port may differ — check console output)

---

## Deploy on Render + Supabase

This repo includes a `Dockerfile`, `.dockerignore`, and `render.yaml` for Render.

1. Create a Supabase project and copy the Postgres connection string.
2. In Render, create a new Web Service from this GitHub repo.
3. Choose Docker as the runtime. Render will use the root `Dockerfile`.
4. Set these environment variables in Render:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
JwtSettings__Secret="your-minimum-32-character-production-secret"
JwtSettings__Issuer="TaskManagerAPI"
JwtSettings__Audience="TaskManagerAPIUsers"
JwtSettings__ExpiryMinutes="60"
```

The app exposes `/health` for Render health checks. Free Render web services spin down after idle time, so the first request after inactivity can be slow.

---

## API Reference

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | None | Create account |
| POST | `/api/auth/login` | None | Get JWT token |

### Tasks

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/tasks` | Bearer | List tasks (paginated) |
| GET | `/api/tasks/{id}` | Bearer | Get single task |
| POST | `/api/tasks` | Bearer | Create task |
| PUT | `/api/tasks/{id}` | Bearer | Update task |
| DELETE | `/api/tasks/{id}` | Bearer | Delete task |

### GET /api/tasks — Query Parameters

| Param | Type | Example | Notes |
|-------|------|---------|-------|
| `status` | enum | `Pending` | `Pending`, `InProgress`, `Completed` |
| `priority` | enum | `High` | `Low`, `Medium`, `High` |
| `search` | string | `fix bug` | Searches Title + Description |
| `page` | int | `2` | Default: 1 |
| `pageSize` | int | `20` | Default: 10, Max: 50 |
| `sortBy` | string | `priority` | `createdAt`, `updatedAt`, `title`, `priority`, `status` |
| `descending` | bool | `false` | Default: true |

---

## Quick API Test

```bash
BASE_URL="https://taskmanagerapi-juxj.onrender.com"

# 1. Register
curl -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"userName":"alice","email":"alice@example.com","password":"Secret123!"}'

# 2. Login → copy token from response
curl -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"Secret123!"}'

# 3. Create task
TOKEN="paste_token_here"
curl -X POST "$BASE_URL/api/tasks" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Fix login bug","description":"Users cant log in on mobile","priority":"High"}'

# 4. List tasks with filtering + pagination
curl "$BASE_URL/api/tasks?status=Pending&priority=High&page=1&pageSize=5" \
  -H "Authorization: Bearer $TOKEN"
```

---

## Authorization Rules

- **Users** see and modify only their own tasks.
- **Admins** see all users' tasks; can update or delete any task.
- Admin role must be set directly in the database (no self-promotion endpoint by design).

```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@example.com';
```

---

## Production Checklist

- [ ] Replace JWT secret with a cryptographically random 64-byte key
- [ ] Restrict CORS to your actual front-end origins
- [ ] Remove `MigrateAsync()` from startup and deploy migrations separately
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure structured logging (Serilog / Application Insights)
- [ ] Add rate limiting (`Microsoft.AspNetCore.RateLimiting`)
- [ ] Enable HTTPS-only, configure HSTS headers
- [x] Set up health check endpoint (`/health`)
