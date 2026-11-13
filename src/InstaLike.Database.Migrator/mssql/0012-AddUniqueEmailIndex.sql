IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_Member_Email' AND [object_id] = OBJECT_ID(N'[dbo].[Member]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UX_Member_Email] ON [dbo].[Member]
	(
		[Email] ASC
	)
END
