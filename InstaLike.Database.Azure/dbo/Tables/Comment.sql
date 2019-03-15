CREATE TABLE [dbo].[Comment] (
    [ID]            INT                IDENTITY (1, 1) NOT NULL,
    [Text]          VARCHAR (500)      CONSTRAINT [DF_Comment_Text] DEFAULT ('') NOT NULL,
    [UserID]        INT                NOT NULL,
    [PostID]        INT                NOT NULL,
    [CommentDate]   DATETIMEOFFSET (7) CONSTRAINT [DF_Comment_CommentDate] DEFAULT (SYSDATETIMEOFFSET()) NOT NULL,
    CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Post_Comment] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Post] ([ID]),
    CONSTRAINT [FK_User_Comment] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([ID])
);


GO

CREATE INDEX [IX_Comment_Post] ON [dbo].[Comment] ([PostID])
GO
