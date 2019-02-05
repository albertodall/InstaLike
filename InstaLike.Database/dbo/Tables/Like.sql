﻿CREATE TABLE [dbo].[Like] (
    [ID]        INT                IDENTITY (1, 1) NOT NULL,
    [PostID]    INT                NOT NULL,
    [UserID]    INT                NOT NULL,
    [DateTime]  DATETIMEOFFSET (7) CONSTRAINT [DF_Like_DateTime] DEFAULT (sysdatetimeoffset()) NOT NULL,
    CONSTRAINT [PK_Like] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Post_Like] FOREIGN KEY ([PostID]) REFERENCES [dbo].[Post] ([ID]),
    CONSTRAINT [FK_User_Like] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([ID])
);


GO

CREATE UNIQUE INDEX [IX_Like_PostUtente] ON [dbo].[Like] ([PostID], [UserID])
GO
