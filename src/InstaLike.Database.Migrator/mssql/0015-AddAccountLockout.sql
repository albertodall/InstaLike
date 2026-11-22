IF COL_LENGTH(N'[dbo].[Account]', N'FailedLogInAttempts') IS NULL
	ALTER TABLE [dbo].[Account] ADD [FailedLogInAttempts] [int] NOT NULL CONSTRAINT [DF_Account_FailedLogInAttempts] DEFAULT 0

IF COL_LENGTH(N'[dbo].[Account]', N'LockedOutUntil') IS NULL
	ALTER TABLE [dbo].[Account] ADD [LockedOutUntil] [datetimeoffset](7) NULL
