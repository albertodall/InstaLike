ALTER TABLE [dbo].[Post]
    ADD [PostGuid] [uniqueidentifier] ROWGUIDCOL NOT NULL
GO

ALTER TABLE [dbo].[Post] 
    ADD CONSTRAINT [DF_PostGuid] DEFAULT (newsequentialid()) FOR [PostGuid]
GO

ALTER TABLE [dbo].[Post] 
    ADD CONSTRAINT [UK_Post_PostGuid] UNIQUE NONCLUSTERED 
    (
        [PostGuid] ASC
    ) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Post]
    ADD [Picture] [varbinary](max)
GO
