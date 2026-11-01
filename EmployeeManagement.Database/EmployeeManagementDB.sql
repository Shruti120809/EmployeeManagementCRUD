CREATE DATABASE EmployeeManagementDB;

GO

USE EmployeeManagementDB;
GO


--CREATE TABLE dbo.Users
--(
--    UserId INT IDENTITY(1,1) NOT NULL
--        CONSTRAINT PK_Users PRIMARY KEY,
--    FullName NVARCHAR(150) NOT NULL,
--    Email NVARCHAR(250) NOT NULL,
--    PasswordHash NVARCHAR(MAX) NOT NULL,
--    CreatedDate DATETIME NOT NULL
--        CONSTRAINT DF_Users_CreatedDate DEFAULT (GETDATE()),
--    Status INT NOT NULL
--        CONSTRAINT DF_Users_Status DEFAULT (1),
--    CONSTRAINT CK_Users_Status CHECK (Status IN (0, 1))
--);
GO

GO

CREATE OR ALTER PROCEDURE dbo.p_UserRegister
    @FullName NVARCHAR(150),
    @Email NVARCHAR(250),
    @PasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
        THROW 50003, 'An account with this email address already exists.', 1;

    INSERT INTO dbo.Users (FullName, Email, PasswordHash, Status)
    VALUES (@FullName, @Email, @PasswordHash, 1);

    SELECT CONVERT(INT, SCOPE_IDENTITY()) AS UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.p_UserGetByEmail
    @Email NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UserId, FullName, Email, PasswordHash, CreatedDate, Status
    FROM dbo.Users
    WHERE Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.p_UserGetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UserId, FullName, Email, PasswordHash, CreatedDate, Status
    FROM dbo.Users
    WHERE UserId = @UserId;
END;
GO


--CREATE TABLE dbo.Employee
--(
--    EmployeeId INT IDENTITY(1,1) NOT NULL 
--        CONSTRAINT PK_Employee PRIMARY KEY,
--    Name NVARCHAR(250) NOT NULL,
--    Email NVARCHAR(250) NOT NULL,
--    MobileNo NVARCHAR(50) NOT NULL,
--    Department NVARCHAR(250) NOT NULL,
--    Designation NVARCHAR(250) NOT NULL,
--    Status INT NOT NULL
--        CONSTRAINT DF_Employee_Status DEFAULT (1),
--    CONSTRAINT CK_Employee_Status CHECK (Status IN (0, 1))
--);

GO

CREATE OR ALTER PROCEDURE dbo.p_EmployeeCRUD
    @Flag INT,
    @EmployeeId INT = NULL,
    @Name NVARCHAR(250) = NULL,
    @Email NVARCHAR(250) = NULL,
    @MobileNo NVARCHAR(50) = NULL,
    @Department NVARCHAR(250) = NULL,
    @Designation NVARCHAR(250) = NULL,
    @Search NVARCHAR(250) = NULL,
    @EmployeeIdOutput INT = NULL OUTPUT,
    @RowsAffected INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @RowsAffected = 0;

    IF @Flag = 1
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.Employee WHERE Email = @Email AND Status = 1)
            THROW 50001, 'An active employee with this email address already exists.', 1;

        INSERT INTO dbo.Employee (Name, Email, MobileNo, Department, Designation, Status)
        VALUES (@Name, @Email, @MobileNo, @Department, @Designation, 1);

        SET @EmployeeIdOutput = CONVERT(INT, SCOPE_IDENTITY());
        SET @RowsAffected = 1;

        SELECT @EmployeeIdOutput AS EmployeeId;
        RETURN;
    END;

    IF @Flag = 2
    BEGIN
        IF EXISTS
        (
            SELECT 1 FROM dbo.Employee
            WHERE Email = @Email AND EmployeeId <> @EmployeeId AND Status = 1
        )
            THROW 50001, 'An active employee with this email address already exists.', 1;

        UPDATE dbo.Employee
        SET Name = @Name,
            Email = @Email,
            MobileNo = @MobileNo,
            Department = @Department,
            Designation = @Designation
        WHERE EmployeeId = @EmployeeId AND Status = 1;

        SET @RowsAffected = @@ROWCOUNT;
        RETURN;
    END;

    IF @Flag = 3
    BEGIN
        UPDATE dbo.Employee
        SET Status = 0
        WHERE EmployeeId = @EmployeeId AND Status = 1;

        SET @RowsAffected = @@ROWCOUNT;
        RETURN;
    END;

    IF @Flag = 4
    BEGIN
        SELECT EmployeeId, Name, Email, MobileNo, Department, Designation
        FROM dbo.Employee
        WHERE Status = 1
          AND
          (
              @Search IS NULL
              OR Name LIKE N'%' + @Search + N'%'
              OR Email LIKE N'%' + @Search + N'%'
          )
        ORDER BY EmployeeId DESC;
        RETURN;
    END;

    IF @Flag = 5
    BEGIN
        SELECT EmployeeId, Name, Email, MobileNo, Department, Designation
        FROM dbo.Employee
        WHERE EmployeeId = @EmployeeId AND Status = 1;
        RETURN;
    END;

    THROW 50002, 'Invalid p_EmployeeCRUD flag.', 1;
END;
GO

-- Sample reads
--EXEC dbo.p_EmployeeCRUD @Flag = 4;
--EXEC dbo.p_EmployeeCRUD @Flag = 4, @Search = N'alex';

-- Sample insert
--DECLARE @NewEmployeeId INT;
--DECLARE @InsertRows INT;
--DECLARE @InsertedId INT;
--EXEC dbo.p_EmployeeCRUD
--    @Flag = 1,
--    @Name = N'Shruti Pancholi',
--    @Email = N'pancholishruti8@gmail.com',
--    @MobileNo = N'6355451495',
--    @Department = N'Engineering',
--    @Designation = N'Software Engineer',
--    @EmployeeIdOutput = @InsertedId OUTPUT,
--    @RowsAffected = @InsertRows OUTPUT;
--SELECT @InsertedId AS NewEmployeeId, @InsertRows AS RowsAffected;

-- Sample get by ID; replace 1 with an existing EmployeeId.
--EXEC dbo.p_EmployeeCRUD @Flag = 5, @EmployeeId = 2;

-- Sample update; replace 1 with an existing EmployeeId.
--EXEC dbo.p_EmployeeCRUD
--    @Flag = 2,
--    @EmployeeId = 1,
--    @Name = N'Alex Morgan',
--    @Email = N'alex.morgan@example.com',
--    @MobileNo = N'9876543210',
--    @Department = N'Engineering',
--    @Designation = N'Senior Software Engineer';

-- Sample soft delete; replace 1 with an existing EmployeeId.
--EXEC dbo.p_EmployeeCRUD @Flag = 3, @EmployeeId = 1;