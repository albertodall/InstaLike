IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Like_Post_User' AND [object_id] = OBJECT_ID(N'[dbo].[Like]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_Like_Post_User] ON [dbo].[Like]
	(
		[PostID] ASC,
		[UserID] ASC
	)
END
