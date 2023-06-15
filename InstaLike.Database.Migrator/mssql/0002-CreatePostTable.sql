IF (OBJECT_ID(N'[dbo].[Post]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Post]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[UserID] [int] NOT NULL,
		[Picture] [varbinary](max) NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[PostDate] [datetimeoffset](7) NOT NULL,
		[PostGuid] [uniqueidentifier] ROWGUIDCOL NOT NULL
	)

	ALTER TABLE [dbo].[Post] ADD CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Post] ADD CONSTRAINT [UK_Post_PostGuid] UNIQUE NONCLUSTERED 
	(
		[PostGuid] ASC
	)

	ALTER TABLE [dbo].[Post] ADD CONSTRAINT [DF_Post_Comment] DEFAULT ('') FOR [Text]
								 
	ALTER TABLE [dbo].[Post] ADD CONSTRAINT [DF_Post_PostDate] DEFAULT (sysdatetimeoffset()) FOR [PostDate]
								 
	ALTER TABLE [dbo].[Post] ADD CONSTRAINT [DF_Post_PostGuid] DEFAULT (newsequentialid()) FOR [PostGuid]

	ALTER TABLE [dbo].[Post] WITH CHECK ADD CONSTRAINT [FK_User_Post] FOREIGN KEY([UserID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Post] CHECK CONSTRAINT [FK_User_Post]
END