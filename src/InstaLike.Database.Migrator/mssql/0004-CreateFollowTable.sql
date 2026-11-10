IF (OBJECT_ID(N'[dbo].[Follow]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Follow]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[FollowerID] [int] NOT NULL,
		[FollowedID] [int] NOT NULL,
		[FollowDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Follow] ADD CONSTRAINT [PK_Follow] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Follow] ADD CONSTRAINT [DF_Follow_FollowDate] DEFAULT (sysdatetimeoffset()) FOR [FollowDate]

	ALTER TABLE [dbo].[Follow] WITH CHECK ADD CONSTRAINT [FK_User_Follower] FOREIGN KEY([FollowerID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_User_Follower]

	ALTER TABLE [dbo].[Follow] WITH CHECK ADD CONSTRAINT [FK_User_Following] FOREIGN KEY([FollowedID]) REFERENCES [dbo].[User] ([ID])
	ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_User_Following]

	CREATE NONCLUSTERED INDEX [IX_Follow_Followed] ON [dbo].[Follow]
	(
		[FollowedID] ASC
	)

	CREATE NONCLUSTERED INDEX [IX_Follow_Follower] ON [dbo].[Follow]
	(
		[FollowerID] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [IX_Follow_Follower_Followed] ON [dbo].[Follow]
	(
		[FollowerID] ASC,
		[FollowedID] ASC
	)
END