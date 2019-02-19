CREATE TABLE [dbo].[Notification] (
    [ID]                INT                IDENTITY (1, 1) NOT NULL,
    [SenderID]          INT                NOT NULL,
    [RecipientID]       INT                NOT NULL,
    [Message]           VARCHAR (200)      CONSTRAINT [DF_Notification_Message] DEFAULT ('') NOT NULL,
    [Read]              BIT                CONSTRAINT [DF_Notification_Read] DEFAULT ((0)) NOT NULL,
    [NotificationDate]  DATETIMEOFFSET (7) CONSTRAINT [DF_Notification_NotificationDate] DEFAULT (sysdatetimeoffset()) NOT NULL,
    CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Recipient_Notification] FOREIGN KEY ([RecipientID]) REFERENCES [dbo].[User] ([ID]),
    CONSTRAINT [FK_Sender_Notification] FOREIGN KEY ([SenderID]) REFERENCES [dbo].[User] ([ID])
);


GO

CREATE INDEX [IX_Notification_Recipient] ON [dbo].[Notification] ([RecipientID]) INCLUDE([Read])
GO
