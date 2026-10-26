
IF OBJECT_ID('dbo.ContributionLedgerOutbox', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContributionLedgerOutbox
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_ContributionLedgerOutbox
            PRIMARY KEY DEFAULT NEWID(),

        ContributionId UNIQUEIDENTIFIER NOT NULL,

        Status VARCHAR(20) NOT NULL
            CONSTRAINT DF_ContributionLedgerOutbox_Status
            DEFAULT 'Pending',

        AttemptCount INT NOT NULL
            CONSTRAINT DF_ContributionLedgerOutbox_AttemptCount
            DEFAULT 0,

        CreatedAtUtc DATETIME2 NOT NULL
            CONSTRAINT DF_ContributionLedgerOutbox_CreatedAtUtc
            DEFAULT SYSUTCDATETIME(),

        NextAttemptAtUtc DATETIME2 NOT NULL
            CONSTRAINT DF_ContributionLedgerOutbox_NextAttemptAtUtc
            DEFAULT SYSUTCDATETIME(),

        ProcessedAtUtc DATETIME2 NULL,

        LastError NVARCHAR(2000) NULL,

        CONSTRAINT FK_ContributionLedgerOutbox_Contribution
            FOREIGN KEY (ContributionId)
            REFERENCES dbo.Contributions(Id),

        CONSTRAINT UQ_ContributionLedgerOutbox_ContributionId
            UNIQUE (ContributionId),

        CONSTRAINT CK_ContributionLedgerOutbox_Status
            CHECK (Status IN ('Pending', 'Processed')),

        CONSTRAINT CK_ContributionLedgerOutbox_AttemptCount
            CHECK (AttemptCount >= 0)
    );

    CREATE INDEX IX_ContributionLedgerOutbox_Pending
        ON dbo.ContributionLedgerOutbox
        (Status, NextAttemptAtUtc, CreatedAtUtc);
END;
