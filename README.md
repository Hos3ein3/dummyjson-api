# DummyJson API

A robust, enterprise-grade RESTful backend application built with **.NET 8** implementing **Clean Architecture** and **Domain-Driven Design (DDD)**. This project serves as a comprehensive example of modern backend development practices, featuring advanced caching, background jobs, scalable media processing, and CQRS patterns.

---

## 🏗️ Architecture

The project is structured into 5 distinct layers to enforce strict separation of concerns, maintainability, and testability:

1. **Domain (`DummyJson.Domain`)**
   - Contains the core business logic, entities, value objects, domain events, and core interfaces (e.g., `IRepository`, `IConcurrent`, `ISoftDelete`).
   - Absolutely no external dependencies (No EF Core, no third-party libraries).

2. **Application (`DummyJson.Application`)**
   - Implements the **CQRS** pattern using MediatR (Commands and Queries).
   - Contains Use Cases, DTOs, Mapping logic, and Validation logic.
   - Depends only on the Domain layer.

3. **Infrastructure (`DummyJson.Infrastructure`)**
   - Contains implementations for external concerns (Email, SMS, Caching, Token Generation, Media Processing).
   - Isolates external SDKs and libraries from the core business logic.

4. **Persistence (`DummyJson.Persistence`)**
   - Contains the Entity Framework Core `DbContext` and Repository implementations.
   - Handles Database Migrations, Interceptors (e.g., Audit Logging), and global query filters.

5. **API (`DummyJson.API`)**
   - The presentation layer using ASP.NET Core Minimal APIs.
   - Responsible for HTTP routing, Dependency Injection wiring, and Middleware execution.

---

## 🚀 Key Features

* **CQRS & MediatR**: Complete separation of read and write operations.
* **Optimistic Concurrency**: Built-in concurrency tokens (`ConcurrencyStamp`) to prevent lost database updates.
* **Hybrid Caching**: Combines Memory Cache (L1) and Redis (L2) using `.NET 9 HybridCache` for extreme performance.
* **Resumable File Uploads**: Implemented via the `tus` protocol (`tusdotnet`) to support pause/resume capabilities for large files.
* **Media Processing**: Asynchronous resizing, compression, and thumbnail generation using `ImageSharp` and `FFMpegCore`.
* **Global Exception Handling**: Centralized `IExceptionHandler` returning RFC 7807 ProblemDetails for predictable client responses.
* **Structured Logging**: Deep integration with `Serilog` for rich, searchable JSON logs.
* **Comprehensive Health Checks**: Native monitoring endpoints for MySQL, MongoDB, and Redis with a visual dashboard.
* **Authentication**: Robust ASP.NET Core Identity with JWT Bearer tokens and multi-factor/external login abstractions.
* **Background Jobs**: Supported abstractions for both `Quartz.NET` and `Hangfire` for distributed task execution.

---

## 📦 Major Packages & Libraries

* **Core & Architecture**: `MediatR`, `FluentValidation`, `Mapster`
* **Database**: `Pomelo.EntityFrameworkCore.MySql` (MySQL), `MongoDB.Driver`
* **Identity & Security**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`
* **Caching**: `Microsoft.Extensions.Caching.Hybrid`, `StackExchange.Redis`
* **Media & Storage**: `SixLabors.ImageSharp`, `FFMpegCore`, `tusdotnet`
* **Logging**: `Serilog.AspNetCore`, `Serilog.Sinks.Seq`
* **Monitoring**: `AspNetCore.HealthChecks.MySql`, `AspNetCore.HealthChecks.Redis`, `AspNetCore.HealthChecks.MongoDb`, `AspNetCore.HealthChecks.UI`
* **Testing**: `Moq`, `xUnit`, `ArchUnitNET`

---

## ⚙️ Procedures & Setup

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* MySQL Server
* Redis (Optional, but recommended for caching)
* MongoDB (For specialized document storage)

### 1. Configuration
Update the connection strings and settings inside `src/DummyJson.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=DummyJsonDb;Uid=root;Pwd=mypassword;",
    "Redis": "localhost:6379",
    "MongoDb": "mongodb://localhost:27017"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email",
    "SmtpPassword": "your-password"
  }
}
```

### 2. Database Migrations
To apply the EF Core migrations to your MySQL database, navigate to the API directory and run:
```bash
cd src/DummyJson.API
dotnet ef database update --project ../DummyJson.Persistence/DummyJson.Persistence.csproj
```

### 3. Run the Application
Start the application using the .NET CLI:
```bash
dotnet run --project src/DummyJson.API/DummyJson.API.csproj
```

### 4. Explore the Endpoints
Once the application is running, you can explore and test the endpoints visually using **Scalar**:
* Open your browser to: `https://localhost:5001/scalar`
* View the API Health Dashboard at: `https://localhost:5001/health-ui`
* View background job status at: `https://localhost:5001/hangfire`

---

## 🧪 Testing
The project includes a comprehensive test suite to ensure structural and logical integrity.
* **Unit Tests**: Verifies internal application logic and domain behavior.
* **Integration Tests**: Verifies database interactions and endpoint routing.
* **Architecture Tests**: Enforces Clean Architecture dependency rules (e.g., Domain cannot reference Application) using `ArchUnitNET`.

To run all tests:
```bash
dotnet test
```
