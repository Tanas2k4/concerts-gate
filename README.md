# Concerts Gate Backend - .NET 10 Web API Server

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Swagger UI Live Preview](https://img.shields.io/badge/Swagger%20Docs-Live%20Preview-brightgreen?style=for-the-badge&logo=swagger&logoColor=black)](https://tanas2k4.github.io/api-concertsgate-static-docs/)
[![OpenAPI 3.0](https://img.shields.io/badge/OpenAPI-3.0-85EA2D?style=for-the-badge&logo=openapiinitiative&logoColor=black)](../swagger.json)
[![Unit Tests](https://img.shields.io/badge/Unit%20Tests-8%2F8%20Passed%20(100%25)-success?style=for-the-badge&logo=checkmarx&logoColor=white)](concerts-gate.tests)

> [!TIP]
> **Online Static Swagger UI Documentation (No Local Setup Required)**:  
> [https://tanas2k4.github.io/api-concertsgate-static-docs/](https://tanas2k4.github.io/api-concertsgate-static-docs/)

High-performance backend API system built on **ASP.NET Core .NET 10**, **Entity Framework Core 10**, and **Microsoft SQL Server**, specialized for high-traffic Flash Sale concert ticketing.

---

## 1. Local Development Setup & Execution

### Prerequisites:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (MSSQLSERVER or SQLEXPRESS) running on `localhost`.

### Step 1: Initialize User Secrets
Run the following commands in the `concerts-gate.server` directory to store sensitive credentials outside the source repository:
```bash
dotnet user-secrets set "JwtSettings:Secret" "ConcertsGateUltraSecureSuperSecretKey2026!#$*&^%LongEnoughKeyForHmacSha256"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ConcertsGateDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

### Step 2: Run Backend Server & Access Swagger UI
```bash
dotnet run
```
- On initial startup, the system will **automatically initialize the `ConcertsGateDb` database**, apply migrations, and seed initial demo data (`DbInitializer.cs`).
- **Swagger UI (Local)**: Navigate to `http://localhost:5000/swagger`.
- **Static Swagger UI Docs (Online Quick View)**: [https://tanas2k4.github.io/api-concertsgate-static-docs/](https://tanas2k4.github.io/api-concertsgate-static-docs/).

### Sample Seed Accounts:
| Role | Email | Password | Permissions |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@concertsgate.com` | `Admin@123456` | Full platform administration, voucher campaign management, system analytics |
| **Operator** | `operator@concertsgate.com` | `Operator@123456` | Concert management, inventory control, booking monitoring & manual overrides |
| **Customer** | `customer@gmail.com` | `Customer@123456` | Browse concerts, reserve tickets, apply vouchers, complete payments |

### Step 3: Run All Unit & Concurrency Tests (8/8 Pass 100%)
From the `concerts-gate.server` directory, execute:
```bash
dotnet test
```
To view detailed test output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 2. Production Deployment Configuration (Docker & Cloud)

When deploying to a production server or Docker container, configurations are securely injected via **Environment Variables**:

| Environment Variable | Sample Production Value | Description |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Switches hosting runtime to Production mode |
| `ConnectionStrings__DefaultConnection` | `Server=db.production.internal;Database=ConcertsGateDb;User Id=app_user;Password=ComplexPassword123!;` | Authenticated SQL Server connection string |
| `JwtSettings__Secret` | `ExtremelyLongAndSecureSecretKeyWithOver64Characters!@#$*&^%2026` | 256-bit minimum HMAC-SHA256 signing secret |
| `JwtSettings__Issuer` | `https://api.concertsgate.com` | Token issuer domain |
| `JwtSettings__Audience` | `https://concertsgate.com` | Token target audience |

---

## 3. Architecture & Modular Folder Guidelines

Each subfolder in the backend contains a dedicated **README.md coding guideline**:

- [**`Controllers/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/Controllers/README.md): RESTful endpoint standards, `[ProducesResponseType]`, Swagger XML documentation.
- [**`Services/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/Services/README.md): Business logic implementation, transaction management, custom exceptions, OCC concurrency.
- [**`Repositories/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/Repositories/README.md): Data access patterns, `.AsNoTracking()` optimization, SQL query standards.
- [**`DTOs/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/DTOs/README.md): Request/Response DTO separation and DataAnnotations validation rules.
- [**`Entities/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/Entities/README.md): Entity definitions, GUID primary keys, and Concurrency Token `RowVersion`.
- [**`concerts-gate.tests/README.md`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/concerts-gate.tests/README.md): Unit and concurrency testing guidelines with xUnit + FluentAssertions.
- [**`BackgroundTasks/`**](file:///e:/WORKSPACE/concerts-gate/concerts-gate.server/BackgroundTasks/BookingExpirationWorker.cs): 30-second recurring background worker automatically releasing expired 10-minute holds.
