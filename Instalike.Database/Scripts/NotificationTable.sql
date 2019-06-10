USE [InstaLike]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 10/06/2019 22:40:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderID] [int] NOT NULL,
	[RecipientID] [int] NOT NULL,
	[Message] [varchar](200) NOT NULL,
	[ReadByRecipient] [bit] NOT NULL,
	[NotificationDate] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notification_Recipient]    Script Date: 10/06/2019 22:40:23 ******/
CREATE NONCLUSTERED INDEX [IX_Notification_Recipient] ON [dbo].[Notification]
(
	[RecipientID] ASC
)
INCLUDE ( 	[ReadByRecipient]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_Message]  DEFAULT ('') FOR [Message]
GO
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_Read]  DEFAULT ((0)) FOR [ReadByRecipient]
GO
ALTER TABLE [dbo].[Notification] ADD  CONSTRAINT [DF_Notification_NotificationDate]  DEFAULT (sysdatetimeoffset()) FOR [NotificationDate]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Recipient_Notification] FOREIGN KEY([RecipientID])
REFERENCES [dbo].[User] ([ID])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Recipient_Notification]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Sender_Notification] FOREIGN KEY([SenderID])
REFERENCES [dbo].[User] ([ID])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Sender_Notification]
GO
