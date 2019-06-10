USE [InstaLike]
GO
/****** Object:  Table [dbo].[User]    Script Date: 10/06/2019 22:40:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nickname] [varchar](20) NOT NULL,
	[Password] [varchar](64) NOT NULL,
	[Name] [varchar](30) NOT NULL,
	[Surname] [varchar](50) NOT NULL,
	[Email] [varchar](50) NOT NULL,
	[Biography] [varchar](500) NOT NULL,
	[ProfilePicture] [varbinary](max) FILESTREAM  NOT NULL,
	[RegistrationDate] [datetimeoffset](7) NOT NULL,
	[ProfilePictureGuid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] FILESTREAM_ON [Pictures],
 CONSTRAINT [UK_User_ProfilePictureGuid] UNIQUE NONCLUSTERED 
(
	[ProfilePictureGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] FILESTREAM_ON [Pictures]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_User_Credentials]    Script Date: 10/06/2019 22:40:23 ******/
CREATE NONCLUSTERED INDEX [IX_User_Credentials] ON [dbo].[User]
(
	[Nickname] ASC,
	[Password] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_User_Nickname]    Script Date: 10/06/2019 22:40:23 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_Nickname] ON [dbo].[User]
(
	[Nickname] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Nickname]  DEFAULT ('') FOR [Nickname]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Password]  DEFAULT ('') FOR [Password]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Name]  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Surname]  DEFAULT ('') FOR [Surname]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Email]  DEFAULT ('') FOR [Email]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_Biography]  DEFAULT ('') FOR [Biography]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_RegistrationDate]  DEFAULT (sysdatetimeoffset()) FOR [RegistrationDate]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_ProfilePictureGuid]  DEFAULT (newsequentialid()) FOR [ProfilePictureGuid]
GO
