IF COL_LENGTH(N'[dbo].[Account]', N'SecurityStamp') IS NULL
	ALTER TABLE [dbo].[Account] ADD [SecurityStamp] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Account_SecurityStamp] DEFAULT NEWID()
