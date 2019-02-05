CREATE TABLE [dbo].[Follow] (
    [FollowerID]  INT                NOT NULL,
    [FollowingID] INT                NOT NULL,
    [FollowDate]  DATETIMEOFFSET (7) CONSTRAINT [DF_Follow_FollowDate] DEFAULT (SYSDATETIMEOFFSET()) NOT NULL,
    CONSTRAINT [PK_Follow] PRIMARY KEY CLUSTERED ([FollowerID] ASC, [FollowingID] ASC),
    CONSTRAINT [FK_User_Follower] FOREIGN KEY ([FollowerID]) REFERENCES [dbo].[User] ([ID]),
    CONSTRAINT [FK_User_Following] FOREIGN KEY ([FollowingID]) REFERENCES [dbo].[User] ([ID])
);




GO
CREATE NONCLUSTERED INDEX [IX_Follow_Following]
    ON [dbo].[Follow]([FollowingID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Follow_Follower]
    ON [dbo].[Follow]([FollowerID] ASC);

