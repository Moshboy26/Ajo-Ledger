
USE AjoLedgerDb;
GO

-- Support a composite foreign key that ensures
-- each contribution's member belongs to its group.
CREATE UNIQUE INDEX UX_SavingsGroupMembers_Group_Id
ON dbo.SavingsGroupMembers (SavingsGroupId, Id);
GO

-- Store each member contribution.
CREATE TABLE dbo.Contributions
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_Contributions PRIMARY KEY
        DEFAULT NEWID(),

    SavingsGroupId UNIQUEIDENTIFIER NOT NULL,

    SavingsGroupMemberId UNIQUEIDENTIFIER NOT NULL,

    Amount DECIMAL(19, 2) NOT NULL,

    ContributedAtUtc DATETIME2 NOT NULL
        CONSTRAINT DF_Contributions_ContributedAtUtc
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT CK_Contributions_Amount
        CHECK (Amount > 0),

    CONSTRAINT FK_Contributions_SavingsGroup
        FOREIGN KEY (SavingsGroupId)
        REFERENCES dbo.SavingsGroups(Id),

    CONSTRAINT FK_Contributions_GroupMember
        FOREIGN KEY (SavingsGroupId, SavingsGroupMemberId)
        REFERENCES dbo.SavingsGroupMembers(SavingsGroupId, Id)
);
GO

-- Make contribution history for a group member
-- efficient to retrieve in date order.
CREATE INDEX IX_Contributions_Group_Member_Date
ON dbo.Contributions
(
    SavingsGroupId,
    SavingsGroupMemberId,
    ContributedAtUtc
);
GO
