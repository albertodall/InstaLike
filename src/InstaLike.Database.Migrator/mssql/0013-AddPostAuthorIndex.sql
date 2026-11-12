IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Post_User_PostDate' AND [object_id] = OBJECT_ID(N'[dbo].[Post]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Post_User_PostDate] ON [dbo].[Post]
	(
		[UserID] ASC,
		[PostDate] DESC
	)
END
