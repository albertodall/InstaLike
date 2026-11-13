IF ((OBJECT_ID(N'[dbo].[Comment]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[Member]', N'U') IS NOT NULL) AND (OBJECT_ID(N'[dbo].[Post]', N'U') IS NOT NULL))
BEGIN
	CREATE TABLE [dbo].[Comment]
	(
		[ID] [uniqueidentifier] NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[MemberID] [uniqueidentifier] NOT NULL,
		[PostID] [uniqueidentifier] NOT NULL,
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

	ALTER TABLE [dbo].[Comment] WITH CHECK ADD CONSTRAINT [FK_Member_Comment] FOREIGN KEY([MemberID]) REFERENCES [dbo].[Member] ([ID])
	ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Member_Comment]

	CREATE NONCLUSTERED INDEX [IX_Comment_Post] ON [dbo].[Comment]
	(
		[PostID] ASC
	)
END