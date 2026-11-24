# Employee Management System

A production-ready ASP.NET Core Razor Pages application with SQL Server LocalDB, JWT, Cookie Authentication, and Web Forms-style code-behind architecture.

## Project Structure

```
EmployeeManagment/
├── EmployeeManagement.sln
└── EmployeeManagement/
    ├── EmployeeManagement.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Data/
    │   └── AppDbContext.cs
    ├── Models/
    │   ├── Employee.cs
    │   ├── User.cs
    │   └── LoginViewModel.cs
    ├── Services/
    │   ├── JwtService.cs
    │   ├── AuthService.cs
    │   └── EmployeeService.cs
    ├── Pages/
    │   ├── _ViewImports.cshtml
    │   ├── _ViewStart.cshtml
    │   ├── Index.cshtml
    │   ├── Index.cshtml.cs
    │   ├── Error.cshtml
    │   ├── Error.cshtml.cs
    │   ├── Account/
    │   │   ├── Login.cshtml (.cs)
    │   │   ├── Register.cshtml (.cs)
    │   │   ├── Logout.cshtml (.cs)
    │   │   └── AccessDenied.cshtml (.cs)
    │   ├── Employees/
    │   │   ├── Index.cshtml (.cs)
    │   │   ├── Create.cshtml (.cs)
    │   │   ├── Edit.cshtml (.cs)
    │   │   ├── Details.cshtml (.cs)
    │   │   └── Delete.cshtml (.cs)
    │   └── Shared/
    │       ├── _Layout.cshtml
    │       └── _ValidationScriptsPartial.cshtml
    └── wwwroot/
        ├── css/site.css
        └── js/site.js
```

## Getting Started

### Prerequisites
- .NET 10 SDK (or .NET 9 / .NET 8)
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)

### Running the Application

1. Open a terminal in `EmployeeManagement`:
   ```bash
   cd EmployeeManagement
   dotnet run
   ```
2. Navigate to `http://localhost:5000` in your browser.
3. The database `WiproEmployeeDB` is automatically created on startup via `EnsureCreated()`. Startup also adds the 100 sample employees (`WIP-BATCH-006` through `WIP-BATCH-105`) when they are missing; the SQL identity continues after the existing employee records.

### SQL Database Script
The complete SQL Server schema and master data are in `EmployeeManagement/Database/EmployeeManagement.sql`. Run it in SQL Server Management Studio or with `sqlcmd`; it creates the database if needed and can be rerun safely.

New accounts are stored in the SQL Server `Users` table. Passwords are stored as BCrypt hashes in `PasswordHash`; the original password is never saved or displayed. Public registration creates Employee accounts only. No default usernames or passwords are included in the repository.

### Secure Configuration
Set `Jwt__SecretKey` to a unique secret of at least 32 bytes in the hosting environment. Development generates an ephemeral key if none is configured; non-development environments fail to start without one.

To provision the first CEO account on a fresh database, set all four environment variables before starting the application: `BootstrapAdmin__Username`, `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, and `BootstrapAdmin__Password`. The password must be at least 12 characters. The bootstrap account is created only when the `Users` table is empty. Keep these values out of source control.
