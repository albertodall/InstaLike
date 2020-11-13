ALTER TABLE [dbo].[User]
	DROP CONSTRAINT [UK_User_ProfilePictureGuid]
GO

ALTER TABLE [dbo].[User]
	DROP CONSTRAINT [DF_ProfilePictureGuid] 
GO

ALTER TABLE [dbo].[User]
	DROP COLUMN [ProfilePictureGuid] 
GO