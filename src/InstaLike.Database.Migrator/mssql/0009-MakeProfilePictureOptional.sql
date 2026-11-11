ALTER TABLE [dbo].[User] DROP CONSTRAINT IF EXISTS [DF_User_ProfilePictureGuid]
ALTER TABLE [dbo].[User] DROP CONSTRAINT IF EXISTS [DF_ProfilePictureGuid]
ALTER TABLE [dbo].[User] DROP CONSTRAINT IF EXISTS [UK_User_ProfilePictureGuid]

IF COLUMNPROPERTY(OBJECT_ID(N'[dbo].[User]'), N'ProfilePictureGuid', 'IsRowGuidCol') = 1
	ALTER TABLE [dbo].[User] ALTER COLUMN [ProfilePictureGuid] DROP ROWGUIDCOL

IF COLUMNPROPERTY(OBJECT_ID(N'[dbo].[User]'), N'ProfilePictureGuid', 'AllowsNull') = 0
	ALTER TABLE [dbo].[User] ALTER COLUMN [ProfilePictureGuid] [uniqueidentifier] NULL

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_User_ProfilePictureGuid' AND [object_id] = OBJECT_ID(N'[dbo].[User]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UX_User_ProfilePictureGuid] ON [dbo].[User]
	(
		[ProfilePictureGuid] ASC
	)
	WHERE [ProfilePictureGuid] IS NOT NULL
END
