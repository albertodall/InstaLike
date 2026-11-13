IF COL_LENGTH(N'[dbo].[Notification]', N'Message') IS NOT NULL AND EXISTS (SELECT 1 FROM [dbo].[Notification])
	THROW 50000, 'The Notification table contains notifications stored as HTML. Delete or convert them before applying this migration.', 1

ALTER TABLE [dbo].[Notification] DROP CONSTRAINT IF EXISTS [DF_Notification_Message]
ALTER TABLE [dbo].[Notification] DROP COLUMN IF EXISTS [Message]

IF COL_LENGTH(N'[dbo].[Notification]', N'Kind') IS NULL
	ALTER TABLE [dbo].[Notification] ADD [Kind] [tinyint] NOT NULL

IF COL_LENGTH(N'[dbo].[Notification]', N'PostID') IS NULL
	ALTER TABLE [dbo].[Notification] ADD [PostID] [uniqueidentifier] NULL

IF OBJECT_ID(N'[dbo].[FK_Post_Notification]', N'F') IS NULL
	ALTER TABLE [dbo].[Notification] WITH CHECK ADD CONSTRAINT [FK_Post_Notification] FOREIGN KEY([PostID]) REFERENCES [dbo].[Post] ([ID])
