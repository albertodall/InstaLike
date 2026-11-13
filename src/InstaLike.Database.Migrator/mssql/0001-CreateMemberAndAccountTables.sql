IF OBJECT_ID(N'[dbo].[Member]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Member]
	(
		[ID] [uniqueidentifier] NOT NULL,
		[Nickname] [varchar](20) NOT NULL,
		[Name] [varchar](30) NOT NULL,
		[Surname] [varchar](50) NOT NULL,
		[Biography] [varchar](500) NOT NULL,
		[RegistrationDate] [datetimeoffset](7) NOT NULL,
		[ProfilePictureGuid] [uniqueidentifier] ROWGUIDCOL NOT NULL
	)

	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [UK_Member_ProfilePictureGuid] UNIQUE NONCLUSTERED 
	(
		[ProfilePictureGuid] ASC
	)

	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_Nickname] DEFAULT ('') FOR [Nickname]
								 
	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_Name] DEFAULT ('') FOR [Name]
								 
	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_Surname] DEFAULT ('') FOR [Surname]
								 
	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_Biography] DEFAULT ('') FOR [Biography]
								 
	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_RegistrationDate] DEFAULT (sysdatetimeoffset()) FOR [RegistrationDate]
								 
	ALTER TABLE [dbo].[Member] ADD CONSTRAINT [DF_Member_ProfilePictureGuid] DEFAULT (newsequentialid()) FOR [ProfilePictureGuid]

	CREATE UNIQUE NONCLUSTERED INDEX [IX_Member_Nickname] ON [dbo].[Member]
	(
		[Nickname] ASC
	)
END

IF OBJECT_ID(N'[dbo].[Account]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Account]
	(
		[ID] [uniqueidentifier] NOT NULL,
		[Username] [varchar](20) NOT NULL,
		[Password] [varchar](64) NOT NULL,
		[Email] [varchar](50) NOT NULL
	)

	ALTER TABLE [dbo].[Account] ADD CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)

	ALTER TABLE [dbo].[Account] WITH CHECK ADD CONSTRAINT [FK_Member_Account] FOREIGN KEY([ID]) REFERENCES [dbo].[Member] ([ID])

	CREATE UNIQUE NONCLUSTERED INDEX [UX_Account_Username] ON [dbo].[Account]
	(
		[Username] ASC
	)
END
