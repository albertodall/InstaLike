IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Post_Member_PostDate' AND [object_id] = OBJECT_ID(N'[dbo].[Post]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_Post_Member_PostDate] ON [dbo].[Post]
	(
		[MemberID] ASC,
		[PostDate] DESC
	)
END
