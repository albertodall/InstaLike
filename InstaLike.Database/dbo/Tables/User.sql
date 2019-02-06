CREATE TABLE [dbo].[User] (
    [ID]                    INT                IDENTITY (1, 1) NOT NULL,
    [Nickname]              VARCHAR (20)       CONSTRAINT [DF_User_Nickname] DEFAULT ('') NOT NULL,
    [Password]              VARCHAR (64)       CONSTRAINT [DF_User_Password] DEFAULT ('') NOT NULL,
    [Name]                  VARCHAR (30)       CONSTRAINT [DF_User_Name] DEFAULT ('') NOT NULL,
    [Surname]               VARCHAR (50)       CONSTRAINT [DF_User_Surname] DEFAULT ('') NOT NULL,
    [Email]				    VARCHAR (50)	   CONSTRAINT [DF_User_Email] DEFAULT('') NOT NULL,
    [Biography]             VARCHAR (500)      CONSTRAINT [DF_User_Biography] DEFAULT ('') NOT NULL,
    [ProfilePicture]	    VARBINARY (MAX)	   FILESTREAM NOT NULL,
    [RegistrationDate]      DATETIMEOFFSET (7) CONSTRAINT [DF_User_RegistrationDate] DEFAULT (sysdatetimeoffset()) NOT NULL,
    [ProfilePictureGuid]    UNIQUEIDENTIFIER ROWGUIDCOL CONSTRAINT [DF_ProfilePictureGuid] DEFAULT (newsequentialid()) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [UK_User_ProfilePictureGuid] UNIQUE NONCLUSTERED ([ProfilePictureGuid] ASC)
) FILESTREAM_ON [Pictures];
GO

CREATE UNIQUE INDEX [IX_User_Nickname] ON [dbo].[User] ([Nickname])
GO


CREATE INDEX [IX_User_Credentials] ON [dbo].[User] ([Nickname], [Password])
GO
