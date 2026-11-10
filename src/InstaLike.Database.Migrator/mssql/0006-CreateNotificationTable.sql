IF (OBJECT_ID(N'[dbo].[Notification]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Notification]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[SenderID] [int] NOT NULL,
		[RecipientID] [int] NOT NULL,
		[Message] [varchar](200) NOT NULL,
		[ReadByRecipient] [bit] NOT NULL,
		[NotificationDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Notification] ADD CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Notification] ADD CONSTRAINT [DF_Notification_Message] DEFAULT ('') FOR [Message]

	ALTER TABLE [dbo].[Notification] ADD CONSTRAINT [DF_Notification_Read] DEFAULT ((0)) FOR [ReadByRecipient]

	ALTER TABLE [dbo].[Notification] ADD CONSTRAINT [DF_Notification_NotificationDate] DEFAULT (sysdatetimeoffset()) FOR [NotificationDate]

	ALTER TABLE [dbo].[Notification] WITH CHECK ADD CONSTRAINT [FK_Recipient_Notification] FOREIGN KEY([RecipientID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Recipient_Notification]

	ALTER TABLE [dbo].[Notification] WITH CHECK ADD CONSTRAINT [FK_Sender_Notification] FOREIGN KEY([SenderID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Sender_Notification]

	CREATE NONCLUSTERED INDEX [IX_Notification_Recipient] ON [dbo].[Notification]
	(
		[RecipientID] ASC
	)
	INCLUDE([ReadByRecipient])
END