IF OBJECT_ID(N'[dbo].[User]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[User]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[Nickname] [varchar](20) NOT NULL,
		[Password] [varchar](64) NOT NULL,
		[Name] [varchar](30) NOT NULL,
		[Surname] [varchar](50) NOT NULL,
		[Email] [varchar](50) NOT NULL,
		[Biography] [varchar](500) NOT NULL,
		[ProfilePicture] [varbinary](max) NULL,
		[RegistrationDate] [datetimeoffset](7) NOT NULL,
		[ProfilePictureGuid] [uniqueidentifier] ROWGUIDCOL NOT NULL
	)

	ALTER TABLE [dbo].[User] ADD CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[User] ADD CONSTRAINT [UK_User_ProfilePictureGuid] UNIQUE NONCLUSTERED 
	(
		[ProfilePictureGuid] ASC
	)

	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Nickname] DEFAULT ('') FOR [Nickname]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Password] DEFAULT ('') FOR [Password]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Name] DEFAULT ('') FOR [Name]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Surname] DEFAULT ('') FOR [Surname]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Email] DEFAULT ('') FOR [Email]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_Biography] DEFAULT ('') FOR [Biography]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_ProfilePicture] DEFAULT (0x) FOR [ProfilePicture]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_RegistrationDate] DEFAULT (sysdatetimeoffset()) FOR [RegistrationDate]
								 
	ALTER TABLE [dbo].[User] ADD CONSTRAINT [DF_User_ProfilePictureGuid] DEFAULT (newsequentialid()) FOR [ProfilePictureGuid]

	CREATE NONCLUSTERED INDEX [IX_User_Credentials] ON [dbo].[User]
	(
		[Nickname] ASC,
		[Password] ASC
	)

	CREATE UNIQUE NONCLUSTERED INDEX [IX_User_Nickname] ON [dbo].[User]
	(
		[Nickname] ASC
	)
END