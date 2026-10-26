
IF DB_ID('AjoLedgerDb') IS NULL
BEGIN
    CREATE DATABASE AjoLedgerDb;
END;
GO

USE AjoLedgerDb;
GO

IF OBJECT_ID('dbo.SavingsGroups', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavingsGroups
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_SavingsGroups PRIMARY KEY,

        Name NVARCHAR(100) NOT NULL,

        ContributionAmount DECIMAL(19, 2) NOT NULL,

        MaxMembers INT NOT NULL,

        CreatedAtUtc DATETIME2 NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_SavingsGroups_IsActive DEFAULT 1,

        CONSTRAINT CK_SavingsGroups_ContributionAmount
            CHECK (ContributionAmount > 0),

        CONSTRAINT CK_SavingsGroups_MaxMembers
            CHECK (MaxMembers BETWEEN 2 AND 1000)
    );
END;
GO

SELECT name
FROM sys.tables
WHERE name = 'SavingsGroups';
GO
