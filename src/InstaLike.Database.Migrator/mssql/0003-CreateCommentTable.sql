IF ((OBJECT_ID(N'[dbo].[Comment]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL) AND (OBJECT_ID(N'[dbo].[Post]', N'U') IS NOT NULL))
BEGIN
	CREATE TABLE [dbo].[Comment]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[UserID] [int] NOT NULL,
		[PostID] [int] NOT NULL,
		[CommentDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Comment] ADD CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Comment] ADD CONSTRAINT [DF_Comment_Text] DEFAULT ('') FOR [Text]

	ALTER TABLE [dbo].[Comment] ADD CONSTRAINT [DF_Comment_DateTime] DEFAULT (sysdatetimeoffset()) FOR [CommentDate]

	ALTER TABLE [dbo].[Comment] WITH CHECK ADD CONSTRAINT [FK_Post_Comment] FOREIGN KEY([PostID]) REFERENCES [dbo].[Post] ([ID])
	ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Post_Comment]

	ALTER TABLE [dbo].[Comment] WITH CHECK ADD CONSTRAINT [FK_User_Comment] FOREIGN KEY([UserID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_User_Comment]

	CREATE NONCLUSTERED INDEX [IX_Comment_Post] ON [dbo].[Comment]
	(
		[PostID] ASC
	)
END