IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_User_Email' AND [object_id] = OBJECT_ID(N'[dbo].[User]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UX_User_Email] ON [dbo].[User]
	(
		[Email] ASC
	)
END
