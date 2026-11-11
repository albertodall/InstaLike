IF OBJECT_ID(N'[dbo].[CK_Follow_NotSelf]', N'C') IS NULL
	ALTER TABLE [dbo].[Follow] WITH CHECK ADD CONSTRAINT [CK_Follow_NotSelf] CHECK ([FollowerID] <> [FollowedID])
