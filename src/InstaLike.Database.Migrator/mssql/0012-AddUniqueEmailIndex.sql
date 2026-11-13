IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_Account_Email' AND [object_id] = OBJECT_ID(N'[dbo].[Account]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UX_Account_Email] ON [dbo].[Account]
	(
		[Email] ASC
	)
END
