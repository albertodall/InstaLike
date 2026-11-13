IF (OBJECT_ID(N'[dbo].[Follow]', N'U') IS NULL) AND (OBJECT_ID(N'[dbo].[Member]', N'U') IS NOT NULL)
BEGIN
	CREATE TABLE [dbo].[Follow]
	(
		[ID] [uniqueidentifier] NOT NULL,
		[FollowerID] [uniqueidentifier] NOT NULL,
		[FollowedID] [uniqueidentifier] NOT NULL,
		[FollowDate] [datetimeoffset](7) NOT NULL
	)

	ALTER TABLE [dbo].[Follow] ADD CONSTRAINT [PK_Follow] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Follow] ADD CONSTRAINT [DF_Follow_FollowDate] DEFAULT (sysdatetimeoffset()) FOR [FollowDate]

	ALTER TABLE [dbo].[Follow] WITH CHECK ADD CONSTRAINT [FK_Member_Follower] FOREIGN KEY([FollowerID]) REFERENCES [dbo].[Member] ([ID])
	ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_Member_Follower]

	ALTER TABLE [dbo].[Follow] WITH CHECK ADD CONSTRAINT [FK_Member_Following] FOREIGN KEY([FollowedID]) REFERENCES [dbo].[Member] ([ID])
	ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_Member_Following]

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