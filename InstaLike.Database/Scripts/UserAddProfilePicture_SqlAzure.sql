ALTER TABLE [dbo].[User]
    ADD [ProfilePictureGuid] [uniqueidentifier] ROWGUIDCOL NOT NULL
GO

ALTER TABLE [dbo].[User] 
    ADD CONSTRAINT [DF_ProfilePictureGuid] DEFAULT (newsequentialid()) FOR [ProfilePictureGuid]
GO

ALTER TABLE [dbo].[User] 
    ADD CONSTRAINT [UK_User_ProfilePictureGuid] UNIQUE NONCLUSTERED 
    (
        [ProfilePictureGuid] ASC
    ) ON [PRIMARY]
GO

ALTER TABLE [dbo].[User]
    ADD [ProfilePicture] [varbinary](max)
GO
