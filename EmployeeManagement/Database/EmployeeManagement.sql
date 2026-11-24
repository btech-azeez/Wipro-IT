IF DB_ID(N'WiproEmployeeDB') IS NULL
BEGIN
    CREATE DATABASE [WiproEmployeeDB];
END;
GO

USE [WiproEmployeeDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Employees
        (
            EmployeeId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
            WiproEmployeeId nvarchar(20) NOT NULL,
            FirstName nvarchar(50) NOT NULL,
            LastName nvarchar(50) NOT NULL,
            Email nvarchar(100) NOT NULL,
            PhoneNumber nvarchar(15) NULL,
            Department nvarchar(50) NOT NULL,
            Designation nvarchar(100) NOT NULL,
            Role nvarchar(20) NOT NULL,
            Salary decimal(18,2) NOT NULL,
            DateOfJoining datetime2 NOT NULL,
            WorkLocation nvarchar(50) NULL,
            ReportingManagerId nvarchar(50) NULL,
            BloodGroup nvarchar(10) NULL,
            DateOfBirth datetime2 NULL,
            Gender nvarchar(10) NULL,
            IsActive bit NOT NULL,
            Address nvarchar(max) NULL,
            ProfilePhotoUrl nvarchar(200) NULL,
            CreatedAt datetime2 NOT NULL,
            UpdatedAt datetime2 NULL
        );
    END;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users
        (
            UserId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
            Username nvarchar(50) NOT NULL,
            FullName nvarchar(100) NOT NULL,
            Email nvarchar(100) NOT NULL,
            PasswordHash nvarchar(max) NOT NULL,
            Role nvarchar(20) NOT NULL,
            Department nvarchar(50) NULL,
            LinkedEmployeeId int NULL,
            IsActive bit NOT NULL,
            CreatedAt datetime2 NOT NULL,
            LastLoginAt datetime2 NULL,
            RefreshToken nvarchar(max) NULL,
            RefreshTokenExpiry datetime2 NULL
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Employees') AND name = N'IX_Employees_Email')
        CREATE UNIQUE INDEX IX_Employees_Email ON dbo.Employees(Email);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Employees') AND name = N'IX_Employees_WiproEmployeeId')
        CREATE UNIQUE INDEX IX_Employees_WiproEmployeeId ON dbo.Employees(WiproEmployeeId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'IX_Users_Username')
        CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users(Username);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'IX_Users_Email')
        CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);

    SET IDENTITY_INSERT dbo.Employees ON;

    INSERT INTO dbo.Employees
    (
        EmployeeId, WiproEmployeeId, FirstName, LastName, Email, PhoneNumber,
        Department, Designation, Role, Salary, DateOfJoining, WorkLocation,
        ReportingManagerId, BloodGroup, DateOfBirth, Gender, IsActive, Address,
        ProfilePhotoUrl, CreatedAt, UpdatedAt
    )
    SELECT seed.EmployeeId, seed.WiproEmployeeId, seed.FirstName, seed.LastName,
           seed.Email, seed.PhoneNumber, seed.Department, seed.Designation,
           seed.Role, seed.Salary, seed.DateOfJoining, seed.WorkLocation,
           seed.ReportingManagerId, NULL, NULL, seed.Gender, 1, NULL, NULL,
           CONVERT(datetime2, '2024-01-01T00:00:00'), NULL
    FROM
    (
        VALUES
        (1, N'WIP-CEO-001', N'Casey', N'Morgan', N'casey.morgan@example.test', NULL, N'Executive', N'Executive', N'CEO', CONVERT(decimal(18,2), 5000000), CONVERT(datetime2, '2023-04-06'), N'Remote', NULL, N'Prefer not to say'),
        (2, N'WIP-HR-001', N'Riley', N'Quinn', N'riley.quinn@example.test', NULL, N'Human Resources', N'People Operations Lead', N'HR', CONVERT(decimal(18,2), 1500000), CONVERT(datetime2, '2018-06-01'), N'Remote', NULL, N'Prefer not to say'),
        (3, N'WIP-ENG-001', N'Jordan', N'Lee', N'jordan.lee@example.test', NULL, N'Engineering', N'Engineering Manager', N'Manager', CONVERT(decimal(18,2), 1800000), CONVERT(datetime2, '2016-03-15'), N'Remote', NULL, N'Prefer not to say'),
        (4, N'WIP-ENG-002', N'Alex', N'Reed', N'alex.reed@example.test', NULL, N'Engineering', N'Senior Software Engineer', N'Employee', CONVERT(decimal(18,2), 1200000), CONVERT(datetime2, '2021-07-01'), N'Remote', N'WIP-ENG-001', N'Prefer not to say'),
        (5, N'WIP-ENG-003', N'Taylor', N'Park', N'taylor.park@example.test', NULL, N'Engineering', N'Software Engineer', N'Employee', CONVERT(decimal(18,2), 800000), CONVERT(datetime2, '2022-11-01'), N'Remote', N'WIP-ENG-001', N'Prefer not to say')
    ) AS seed(EmployeeId, WiproEmployeeId, FirstName, LastName, Email, PhoneNumber,
              Department, Designation, Role, Salary, DateOfJoining, WorkLocation,
              ReportingManagerId, Gender)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.Employees existing
        WHERE existing.EmployeeId = seed.EmployeeId
           OR existing.WiproEmployeeId = seed.WiproEmployeeId
           OR existing.Email = seed.Email
    );

    SET IDENTITY_INSERT dbo.Employees OFF;

    ;WITH Numbers AS
    (
        SELECT TOP (100) ROW_NUMBER() OVER (ORDER BY object_id) + 5 AS EmployeeSequence
        FROM sys.all_objects
    ),
    FirstNames AS
    (
        SELECT NameIndex, FirstName
        FROM (VALUES
            (0, N'Aanya'), (1, N'Arjun'), (2, N'Dev'), (3, N'Isha'), (4, N'Kabir'),
            (5, N'Mira'), (6, N'Neel'), (7, N'Riya'), (8, N'Vihaan'), (9, N'Zoya')
        ) names(NameIndex, FirstName)
    ),
    LastNames AS
    (
        SELECT NameIndex, LastName
        FROM (VALUES
            (0, N'Adiga'), (1, N'Bansal'), (2, N'Chawla'), (3, N'Das'), (4, N'Iyer'),
            (5, N'Joshi'), (6, N'Kapoor'), (7, N'Nair'), (8, N'Rao'), (9, N'Singh')
        ) names(NameIndex, LastName)
    ),
    Departments AS
    (
        SELECT DepartmentIndex, Department
        FROM (VALUES
            (0, N'Engineering'), (1, N'Human Resources'), (2, N'Sales'), (3, N'Finance'),
            (4, N'Marketing'), (5, N'Operations'), (6, N'Legal'), (7, N'IT Infrastructure'),
            (8, N'Quality Assurance'), (9, N'Product')
        ) departments(DepartmentIndex, Department)
    ),
    Locations AS
    (
        SELECT LocationIndex, WorkLocation
        FROM (VALUES
            (0, N'Bangalore'), (1, N'Hyderabad'), (2, N'Pune'), (3, N'Chennai'), (4, N'Mumbai')
        ) locations(LocationIndex, WorkLocation)
    )
    INSERT INTO dbo.Employees
    (
        WiproEmployeeId, FirstName, LastName, Email, PhoneNumber, Department,
        Designation, Role, Salary, DateOfJoining, WorkLocation, IsActive, CreatedAt
    )
    SELECT TOP (100)
        CONCAT(N'WIP-BATCH-', RIGHT(CONCAT(N'000', numbers.EmployeeSequence), 3)),
        firstNames.FirstName,
        lastNames.LastName,
        CONCAT(N'employee', RIGHT(CONCAT(N'000', numbers.EmployeeSequence), 3), N'@example.test'),
        NULL,
        departments.Department,
        N'Associate',
        N'Employee',
        CONVERT(decimal(18,2), 600000 + (numbers.EmployeeSequence % 12 * 100000)),
        DATEADD(day, numbers.EmployeeSequence, CONVERT(datetime2, '2024-01-01T00:00:00')),
        locations.WorkLocation,
        1,
        SYSUTCDATETIME()
    FROM Numbers numbers
    JOIN FirstNames firstNames ON firstNames.NameIndex = ((numbers.EmployeeSequence - 6) / 10)
    JOIN LastNames lastNames ON lastNames.NameIndex = ((numbers.EmployeeSequence - 6) % 10)
    JOIN Departments departments ON departments.DepartmentIndex = ((numbers.EmployeeSequence - 6) % 10)
    JOIN Locations locations ON locations.LocationIndex = ((numbers.EmployeeSequence - 6) % 5)
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.Employees existing
        WHERE existing.WiproEmployeeId = CONCAT(N'WIP-BATCH-', RIGHT(CONCAT(N'000', numbers.EmployeeSequence), 3))
    )
    ORDER BY numbers.EmployeeSequence;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT
    (SELECT COUNT(*) FROM dbo.Employees) AS EmployeeCount,
    (SELECT MIN(EmployeeId) FROM dbo.Employees) AS FirstEmployeeId,
    (SELECT MAX(EmployeeId) FROM dbo.Employees) AS LastEmployeeId,
    (SELECT COUNT(*) FROM dbo.Users) AS UserCount;
GO