ALTER TABLE [dbo].[Post]
	DROP COLUMN [Picture] 
GO

ALTER TABLE [dbo].[Post]
	DROP CONSTRAINT [UK_Post_PostGuid]
GO

ALTER TABLE [dbo].[Post]
	DROP CONSTRAINT [DF_PostGuid] 
GO

ALTER TABLE [dbo].[Post] 
	DROP COLUMN [PostGuid] 
GO