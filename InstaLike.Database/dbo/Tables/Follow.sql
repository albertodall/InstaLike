CREATE TABLE [dbo].[Follow] (
    [ID]          INT                IDENTITY (1, 1) NOT NULL,
    [FollowerID]  INT                NOT NULL,
    [FollowedID] INT                NOT NULL,
    [FollowDate]  DATETIMEOFFSET (7) CONSTRAINT [DF_Follow_FollowDate] DEFAULT (SYSDATETIMEOFFSET()) NOT NULL,
    CONSTRAINT [PK_Follow] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_User_Follower]  FOREIGN KEY ([FollowerID])  REFERENCES [dbo].[User] ([ID]),
    CONSTRAINT [FK_User_Following] FOREIGN KEY ([FollowedID]) REFERENCES [dbo].[User] ([ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Follow_Followed]
    ON [dbo].[Follow]([FollowedID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Follow_Follower]
    ON [dbo].[Follow]([FollowerID] ASC);


GO

CREATE UNIQUE INDEX [IX_Follow_Follower_Followed] ON [dbo].[Follow] ([FollowerID], [FollowedID])
