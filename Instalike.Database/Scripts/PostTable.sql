USE [InstaLike]
GO
/****** Object:  Table [dbo].[Post]    Script Date: 10/06/2019 22:40:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Post](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Picture] [varbinary](max) FILESTREAM  NOT NULL,
	[Text] [varchar](500) NOT NULL,
	[PostDate] [datetimeoffset](7) NOT NULL,
	[PostGuid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
 CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] FILESTREAM_ON [Pictures],
 CONSTRAINT [UK_Post_PostGuid] UNIQUE NONCLUSTERED 
(
	[PostGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] FILESTREAM_ON [Pictures]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [DF_Post_Comment]  DEFAULT ('') FOR [Text]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [DF_Post_PostDate]  DEFAULT (sysdatetimeoffset()) FOR [PostDate]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [DF_Post_PostGuid]  DEFAULT (newsequentialid()) FOR [PostGuid]
GO
ALTER TABLE [dbo].[Post]  WITH CHECK ADD  CONSTRAINT [FK_User_Post] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([ID])
GO
ALTER TABLE [dbo].[Post] CHECK CONSTRAINT [FK_User_Post]
GO
