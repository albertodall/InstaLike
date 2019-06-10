USE [InstaLike]
GO
/****** Object:  Table [dbo].[Follow]    Script Date: 10/06/2019 22:40:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Follow](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FollowerID] [int] NOT NULL,
	[FollowedID] [int] NOT NULL,
	[FollowDate] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_Follow] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Follow_Followed]    Script Date: 10/06/2019 22:40:23 ******/
CREATE NONCLUSTERED INDEX [IX_Follow_Followed] ON [dbo].[Follow]
(
	[FollowedID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Follow_Follower]    Script Date: 10/06/2019 22:40:23 ******/
CREATE NONCLUSTERED INDEX [IX_Follow_Follower] ON [dbo].[Follow]
(
	[FollowerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Follow_Follower_Followed]    Script Date: 10/06/2019 22:40:23 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Follow_Follower_Followed] ON [dbo].[Follow]
(
	[FollowerID] ASC,
	[FollowedID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Follow] ADD  CONSTRAINT [DF_Follow_FollowDate]  DEFAULT (sysdatetimeoffset()) FOR [FollowDate]
GO
ALTER TABLE [dbo].[Follow]  WITH CHECK ADD  CONSTRAINT [FK_User_Follower] FOREIGN KEY([FollowerID])
REFERENCES [dbo].[User] ([ID])
GO
ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_User_Follower]
GO
ALTER TABLE [dbo].[Follow]  WITH CHECK ADD  CONSTRAINT [FK_User_Following] FOREIGN KEY([FollowedID])
REFERENCES [dbo].[User] ([ID])
GO
ALTER TABLE [dbo].[Follow] CHECK CONSTRAINT [FK_User_Following]
GO
