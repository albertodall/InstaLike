USE [InstaLike]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Comment](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Text] [varchar](500) NOT NULL,
    [UserID] [int] NOT NULL,
    [PostID] [int] NOT NULL,
    [CommentDate] [datetimeoffset](7) NOT NULL,
    CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
    (
        [ID] ASC
    ) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Comment_Post] ON [dbo].[Comment]
(
    [PostID] ASC
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_Text]  DEFAULT ('') FOR [Text]
ALTER TABLE [dbo].[Comment] ADD  CONSTRAINT [DF_Comment_DateTime]  DEFAULT (sysdatetimeoffset()) FOR [CommentDate]
ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Post_Comment] FOREIGN KEY([PostID]) REFERENCES [dbo].[Post] ([ID])
ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Post_Comment]
ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_User_Comment] FOREIGN KEY([UserID]) REFERENCES [dbo].[User] ([ID])
ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_User_Comment]
GO
