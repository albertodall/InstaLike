CREATE TABLE [dbo].[Post] (
    [ID]        INT                        IDENTITY (1, 1) NOT NULL,
    [UserID]    INT                        NOT NULL,
    [Picture]   VARBINARY (MAX) FILESTREAM NOT NULL,
    [Comment]   VARCHAR (500)              CONSTRAINT [DF_Post_Comment] DEFAULT ('') NOT NULL,
    [DateTime]  DATETIMEOFFSET (7)         CONSTRAINT [DF_Post_DateTime] DEFAULT (SYSDATETIMEOFFSET()) NOT NULL,
    [PostGuid]  UNIQUEIDENTIFIER           ROWGUIDCOL CONSTRAINT [DF_Post_PostGuid] DEFAULT (NEWSEQUENTIALID()) NOT NULL,
    CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_User_Post] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([ID]),
    CONSTRAINT [UK_Post_PostGuid] UNIQUE NONCLUSTERED ([PostGuid] ASC)
) FILESTREAM_ON [Pictures];

