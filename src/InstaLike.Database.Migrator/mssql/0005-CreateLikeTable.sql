IF (OBJECT_ID(N'[dbo].[Like]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL) AND (OBJECT_ID(N'[dbo].[Post]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Like]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[PostID] [int] NOT NULL,
		[UserID] [int] NOT NULL,
		[LikeDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Like] ADD CONSTRAINT [PK_Like] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Like] ADD CONSTRAINT [DF_Like_LikeDate] DEFAULT (sysdatetimeoffset()) FOR [LikeDate]

	ALTER TABLE [dbo].[Like] WITH CHECK ADD CONSTRAINT [FK_Post_Like] FOREIGN KEY([PostID]) REFERENCES [dbo].[Post] ([ID])
	ALTER TABLE [dbo].[Like] CHECK CONSTRAINT [FK_Post_Like]

	ALTER TABLE [dbo].[Like] WITH CHECK ADD CONSTRAINT [FK_User_Like] FOREIGN KEY([UserID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Like] CHECK CONSTRAINT [FK_User_Like]
END