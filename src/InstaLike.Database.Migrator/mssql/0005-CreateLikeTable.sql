IF (OBJECT_ID(N'[dbo].[Like]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[Member]', N'U') IS NOT NULL) AND (OBJECT_ID(N'[dbo].[Post]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Like]
	(
		[ID] [uniqueidentifier] NOT NULL,
		[PostID] [uniqueidentifier] NOT NULL,
		[MemberID] [uniqueidentifier] NOT NULL,
		[LikeDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Like] ADD CONSTRAINT [PK_Like] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Like] ADD CONSTRAINT [DF_Like_LikeDate] DEFAULT (sysdatetimeoffset()) FOR [LikeDate]

	ALTER TABLE [dbo].[Like] WITH CHECK ADD CONSTRAINT [FK_Post_Like] FOREIGN KEY([PostID]) REFERENCES [dbo].[Post] ([ID])
	ALTER TABLE [dbo].[Like] CHECK CONSTRAINT [FK_Post_Like]

	ALTER TABLE [dbo].[Like] WITH CHECK ADD CONSTRAINT [FK_Member_Like] FOREIGN KEY([MemberID]) REFERENCES [dbo].[Member] ([ID])
	ALTER TABLE [dbo].[Like] CHECK CONSTRAINT [FK_Member_Like]
END