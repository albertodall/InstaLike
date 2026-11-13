ALTER TABLE [dbo].[Member] DROP CONSTRAINT IF EXISTS [DF_Member_ProfilePictureGuid]
ALTER TABLE [dbo].[Member] DROP CONSTRAINT IF EXISTS [DF_ProfilePictureGuid]
ALTER TABLE [dbo].[Member] DROP CONSTRAINT IF EXISTS [UK_Member_ProfilePictureGuid]

IF COLUMNPROPERTY(OBJECT_ID(N'[dbo].[Member]'), N'ProfilePictureGuid', 'IsRowGuidCol') = 1
	ALTER TABLE [dbo].[Member] ALTER COLUMN [ProfilePictureGuid] DROP ROWGUIDCOL

IF COLUMNPROPERTY(OBJECT_ID(N'[dbo].[Member]'), N'ProfilePictureGuid', 'AllowsNull') = 0
	ALTER TABLE [dbo].[Member] ALTER COLUMN [ProfilePictureGuid] [uniqueidentifier] NULL

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_Member_ProfilePictureGuid' AND [object_id] = OBJECT_ID(N'[dbo].[Member]'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [UX_Member_ProfilePictureGuid] ON [dbo].[Member]
	(
		[ProfilePictureGuid] ASC
	)
	WHERE [ProfilePictureGuid] IS NOT NULL
END
