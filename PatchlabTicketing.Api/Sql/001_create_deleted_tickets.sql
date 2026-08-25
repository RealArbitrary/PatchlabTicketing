-- Archive table for hard-deleted tickets.
-- Standalone: no foreign keys back to Tickets/TicketComments/TicketFeedback,
-- since those live rows are gone by the time a row lands here.
-- TicketId preserves the original Tickets.Id for reference; it is not an identity here.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeletedTickets')
BEGIN
    CREATE TABLE dbo.DeletedTickets
    (
        ArchiveId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TicketId         INT NOT NULL,
        TicketNumber     VARCHAR(9) NULL,
        CellphoneNumber  NVARCHAR(40) NOT NULL,
        Issue            NVARCHAR(MAX) NOT NULL,
        Area             NVARCHAR(400) NULL,
        CreatedAt        DATETIME2 NOT NULL,
        ResolvedAt       DATETIME2 NULL,
        Status           NVARCHAR(40) NOT NULL,
        CommentsJson     NVARCHAR(MAX) NULL,
        FeedbackJson     NVARCHAR(MAX) NULL,
        DeletedAt        DATETIME2 NOT NULL DEFAULT (GETUTCDATE())
    );

    CREATE INDEX IX_DeletedTickets_DeletedAt ON dbo.DeletedTickets (DeletedAt);
END
GO
