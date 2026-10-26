
USE AjoLedgerDb;
GO

IF OBJECT_ID('dbo.SavingsGroupMembers', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavingsGroupMembers
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_SavingsGroupMembers PRIMARY KEY,

        SavingsGroupId UNIQUEIDENTIFIER NOT NULL,

        FullName NVARCHAR(100) NOT NULL,

        PhoneNumber VARCHAR(20) NOT NULL,

        JoinedAtUtc DATETIME2 NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_SavingsGroupMembers_IsActive
            DEFAULT 1,

        CONSTRAINT FK_SavingsGroupMembers_SavingsGroups
            FOREIGN KEY (SavingsGroupId)
            REFERENCES dbo.SavingsGroups(Id),

        CONSTRAINT CK_SavingsGroupMembers_FullName
            CHECK (LEN(LTRIM(RTRIM(FullName))) >= 3)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_SavingsGroupMembers_Group_Phone'
      AND object_id = OBJECT_ID('dbo.SavingsGroupMembers')
)
BEGIN
    CREATE UNIQUE INDEX UX_SavingsGroupMembers_Group_Phone
    ON dbo.SavingsGroupMembers(SavingsGroupId, PhoneNumber);
END;
GO

SELECT name
FROM sys.tables
WHERE name IN ('SavingsGroups', 'SavingsGroupMembers');
GO
