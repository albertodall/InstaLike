-- The data used during the first development of InstaLike: Gotham City and its citizens.
-- Upload the pictures first (upload-sample-pictures.ps1): the rows reference them by their blob names.
-- Rows that already exist are skipped, so the script can run again.

SET NOCOUNT ON
SET XACT_ABORT ON
SET QUOTED_IDENTIFIER ON

BEGIN TRANSACTION

DECLARE @SampleUser TABLE ([SampleID] int PRIMARY KEY, [Nickname] varchar(20), [Password] varchar(64), [Name] varchar(30), [Surname] varchar(50), [Email] varchar(50), [Biography] varchar(500), [RegistrationDate] datetimeoffset(7), [ProfilePictureGuid] uniqueidentifier)
INSERT INTO @SampleUser VALUES
	(1, N'alby', N'fttfRh0fPGfLEHiGPItbciHFCPE9MM7H1xpm8z2RCiNedObS', N'Alberto', N'Dallagiacoma', N'alberto.dallagiacoma@live.it', N'Stréordinério!', '2019-02-08T11:13:14.5809801+01:00', '19323a28-8a2b-e911-a7e3-58fb840515da'),
	(2, N'batman', N'QmKzllM73U8vlSixhvMYycAsAPzozJmBXHEDrKDZAjHPiyA1', N'Bruce', N'Wayne', N'batman@gotham.com', N'The Dark Knight!', '2019-02-27T17:27:37.6910298+01:00', '85e5f397-ac3a-e911-a7eb-58fb840515da'),
	(3, N'joker', N'6YhQLcYh2a6MOuFlfburf5yTgm3+5f1n1D3D5D8crwCD8GV7', N'Jack', N'Napier', N'joker@gotham.com', N'Smile, and your life will smile you as well!', '2019-02-27T17:28:41.2652968+01:00', '85a0b4bd-ac3a-e911-a7eb-58fb840515da'),
	(1003, N'catwoman', N'QGRXQRmqF5IiL3TBh3FprgImNDiBvEBbkQ26B4gR8veGE8cS', N'Selina', N'Kyle', N'selina@gotham.com', N'Say "Meow!" seven times.', '2019-04-19T15:05:13.4292010+02:00', '990279c4-a362-e911-a809-58fb840515da'),
	(1007, N'riddler', N'J2EFJXv1jypBU7frFYKyD8o3PbfKhEc9uSHaYoJtd10fPSeG', N'Edward', N'Nygma', N'riddler@gotham.com', N'Guess who!', '2019-09-16T17:10:57.5046807+02:00', NULL),
	(1008, N'penguin', N'u0NGsaO+l1HEeB90SY3VOb61TXrPuxDKpTatCf3oQbDiiYiC', N'Oswald', N'Cobblepot', N'mayor@gotham.com', N'A penguin is a bird that cannot fly! I am a man.', '2020-01-04T22:12:06.1342688+01:00', NULL)

INSERT INTO [dbo].[User] ([Nickname], [Password], [Name], [Surname], [Email], [Biography], [RegistrationDate], [ProfilePictureGuid])
SELECT s.[Nickname], s.[Password], s.[Name], s.[Surname], s.[Email], s.[Biography], s.[RegistrationDate], s.[ProfilePictureGuid]
FROM @SampleUser s
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[User] u WHERE u.[Nickname] = s.[Nickname] OR u.[Email] = s.[Email])
ORDER BY s.[SampleID]

DECLARE @UserID TABLE ([SampleID] int PRIMARY KEY, [ID] int)
INSERT INTO @UserID
SELECT s.[SampleID], u.[ID] FROM @SampleUser s INNER JOIN [dbo].[User] u ON u.[Nickname] = s.[Nickname]

DECLARE @SamplePost TABLE ([SampleID] int PRIMARY KEY, [SampleUserID] int, [Text] varchar(500), [PostDate] datetimeoffset(7), [PostGuid] uniqueidentifier)
INSERT INTO @SamplePost VALUES
	(1, 1, N'Foggy street', '2019-02-22T14:09:00.7640603+01:00', 'c7ab5008-a336-e911-a7eb-58fb840515da'),
	(2, 2, N'Typical night on the Gotham''s roof. @joker #vigilante', '2019-03-01T11:05:26.3168209+01:00', '66c69388-093c-e911-a7ec-58fb840515da'),
	(1002, 1, N'Sembro quasi una persona seria... #seminar #talk', '2019-04-18T12:03:48.9113244+02:00', 'ce2a5442-c161-e911-a805-58fb840515da'),
	(1003, 1003, N'Myself! #girl #hot', '2019-04-19T15:08:55.0202857+02:00', '499f7148-a462-e911-a809-58fb840515da'),
	(1004, 1, N'Vrooom! Vrooom! #tachometer #tripcomputer #bmw', '2020-03-04T23:16:14.2431950+01:00', 'fffd7969-75cd-44a5-d970-08d7c089a714')

INSERT INTO [dbo].[Post] ([UserID], [Text], [PostDate], [PostGuid])
SELECT u.[ID], s.[Text], s.[PostDate], s.[PostGuid]
FROM @SamplePost s
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Post] p WHERE p.[PostGuid] = s.[PostGuid])
ORDER BY s.[SampleID]

DECLARE @PostID TABLE ([SampleID] int PRIMARY KEY, [ID] int)
INSERT INTO @PostID
SELECT s.[SampleID], p.[ID] FROM @SamplePost s INNER JOIN [dbo].[Post] p ON p.[PostGuid] = s.[PostGuid]

INSERT INTO [dbo].[Comment] ([Text], [UserID], [PostID], [CommentDate])
SELECT s.[Text], u.[ID], p.[ID], s.[CommentDate]
FROM (VALUES
	(4, N'I feel safe now! #gothamcitizen', 1, 2, '2019-03-04T09:25:58.6743344+01:00'),
	(5, N'Good place where to put a bomb!', 3, 1, '2019-03-04T16:53:30.1914165+01:00'),
	(7, N'If you try, I''ll stop you!', 2, 1, '2019-03-04T17:07:59.2631514+01:00'),
	(1004, N'Anyway, a great view of the city! #gotham', 1, 2, '2019-04-08T16:51:50.0155919+02:00'),
	(1005, N'Safe?? The best has yet to come! :-) #fool', 3, 2, '2019-04-08T16:59:49.1386881+02:00'),
	(1006, N'@batman, do you need a #Robin?', 1, 2, '2019-04-08T17:10:44.4828749+02:00'),
	(1007, N'@alby che figo!', 2, 1002, '2019-04-18T12:04:44.7773284+02:00'),
	(1008, N'@batman avrei preferito me lo dicesse Vicky Vale... :-)', 1, 1002, '2019-04-18T12:05:42.2458633+02:00')
) s ([SampleID], [Text], [SampleUserID], [SamplePostID], [CommentDate])
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
	INNER JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Comment] c WHERE c.[UserID] = u.[ID] AND c.[PostID] = p.[ID] AND c.[CommentDate] = s.[CommentDate])
ORDER BY s.[SampleID]

INSERT INTO [dbo].[Follow] ([FollowerID], [FollowedID], [FollowDate])
SELECT follower.[ID], followed.[ID], s.[FollowDate]
FROM (VALUES
	(2, 1, '2019-04-05T11:20:38.8805832+02:00')
) s ([SampleFollowerID], [SampleFollowedID], [FollowDate])
	INNER JOIN @UserID follower ON follower.[SampleID] = s.[SampleFollowerID]
	INNER JOIN @UserID followed ON followed.[SampleID] = s.[SampleFollowedID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Follow] f WHERE f.[FollowerID] = follower.[ID] AND f.[FollowedID] = followed.[ID])
ORDER BY s.[FollowDate]

INSERT INTO [dbo].[Like] ([PostID], [UserID], [LikeDate])
SELECT p.[ID], u.[ID], s.[LikeDate]
FROM (VALUES
	(2, 2, 3, '2019-02-27T17:30:01.7030482+01:00'),
	(1002, 1, 2, '2019-03-01T11:06:05.5291670+01:00'),
	(2003, 2, 1, '2019-03-04T17:07:29.0925160+01:00'),
	(3002, 1, 3, '2019-03-06T16:49:16.2051776+01:00'),
	(5003, 1, 1003, '2019-04-19T15:10:56.3414257+02:00')
) s ([SampleID], [SamplePostID], [SampleUserID], [LikeDate])
	INNER JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Like] l WHERE l.[PostID] = p.[ID] AND l.[UserID] = u.[ID])
ORDER BY s.[SampleID]

INSERT INTO [dbo].[Notification] ([SenderID], [RecipientID], [ReadByRecipient], [NotificationDate], [Kind], [PostID])
SELECT sender.[ID], recipient.[ID], s.[ReadByRecipient], s.[NotificationDate], s.[Kind], p.[ID]
FROM (VALUES
	(1, 1, 2, 1, '2019-03-06T10:34:25.5086997+01:00', 2, 2),
	(1002, 1, 2, 1, '2019-03-07T12:20:49.8621949+01:00', 2, 2),
	(1003, 1, 2, 1, '2019-03-07T12:23:26.7512792+01:00', 2, 2),
	(1004, 3, 1, 1, '2019-03-08T12:44:13.6591069+01:00', 1, NULL),
	(2004, 1, 2, 1, '2019-03-20T17:22:55.8397809+01:00', 2, 2),
	(2005, 1, 2, 1, '2019-03-20T17:23:53.7246746+01:00', 2, 2),
	(2006, 1, 2, 1, '2019-03-20T17:23:56.7202411+01:00', 2, 2),
	(2007, 1, 2, 1, '2019-03-20T17:25:28.2260180+01:00', 2, 2),
	(3004, 1, 2, 1, '2019-03-21T12:42:09.4944389+01:00', 2, 2),
	(3005, 1, 2, 1, '2019-03-21T12:42:09.4944389+01:00', 2, 2),
	(3006, 1, 2, 1, '2019-03-21T12:42:11.4381371+01:00', 2, 2),
	(3007, 1, 2, 1, '2019-03-21T12:42:15.4958474+01:00', 2, 2),
	(3008, 1, 2, 1, '2019-03-21T12:42:18.9453966+01:00', 2, 2),
	(3009, 1, 2, 1, '2019-04-02T16:03:46.3188284+02:00', 2, 2),
	(4009, 1, 2, 1, '2019-04-05T11:20:39.4717892+02:00', 2, 2),
	(4010, 1, 2, 1, '2019-04-08T16:51:50.6667206+02:00', 3, 2),
	(4011, 3, 2, 1, '2019-04-08T16:59:07.3977874+02:00', 1, NULL),
	(4012, 3, 2, 1, '2019-04-08T16:59:49.4102772+02:00', 3, 2),
	(4013, 1, 2, 1, '2019-04-08T17:10:45.2559725+02:00', 3, 2),
	(4014, 2, 1, 1, '2019-04-18T12:04:45.0633337+02:00', 3, 1002),
	(4016, 2, 1, 1, '2019-04-18T12:07:39.2961947+02:00', 2, 1002),
	(4017, 1, 1003, 0, '2019-04-19T15:10:56.5890046+02:00', 1, NULL)
) s ([SampleID], [SampleSenderID], [SampleRecipientID], [ReadByRecipient], [NotificationDate], [Kind], [SamplePostID])
	INNER JOIN @UserID sender ON sender.[SampleID] = s.[SampleSenderID]
	INNER JOIN @UserID recipient ON recipient.[SampleID] = s.[SampleRecipientID]
	LEFT JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Notification] n WHERE n.[SenderID] = sender.[ID] AND n.[RecipientID] = recipient.[ID] AND n.[Kind] = s.[Kind] AND n.[NotificationDate] = s.[NotificationDate])
ORDER BY s.[SampleID]

COMMIT TRANSACTION
