ALTER TABLE [dbo].[Member] DROP CONSTRAINT IF EXISTS [DF_Member_ProfilePicture]
ALTER TABLE [dbo].[Member] DROP COLUMN IF EXISTS [ProfilePicture]
ALTER TABLE [dbo].[Post] DROP COLUMN IF EXISTS [Picture]
