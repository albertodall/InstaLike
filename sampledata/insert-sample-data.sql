-- The data used during the first development of InstaLike: Gotham City and its citizens.
-- Upload the pictures first (upload-sample-pictures.ps1): the rows reference them by their blob names.
-- Rows that already exist are skipped, so the script can run again.

SET NOCOUNT ON
SET XACT_ABORT ON
SET QUOTED_IDENTIFIER ON

BEGIN TRANSACTION

DECLARE @SampleUser TABLE ([ID] uniqueidentifier, [SampleID] int PRIMARY KEY, [Nickname] varchar(20), [Password] varchar(64), [Name] varchar(30), [Surname] varchar(50), [Email] varchar(50), [Biography] varchar(500), [RegistrationDate] datetimeoffset(7), [ProfilePictureGuid] uniqueidentifier)
INSERT INTO @SampleUser VALUES
	('0780bc4b-e542-d873-47a0-0168cc98d4d4', 1, N'alby', N'fttfRh0fPGfLEHiGPItbciHFCPE9MM7H1xpm8z2RCiNedObS', N'Alberto', N'Dallagiacoma', N'alberto.dallagiacoma@live.it', N'Stréordinério!', '2019-02-08T11:13:14.5809801+01:00', '19323a28-8a2b-e911-a7e3-58fb840515da'),
	('3149efb2-2f0b-5bca-7a8e-01692fc86b5b', 2, N'batman', N'QmKzllM73U8vlSixhvMYycAsAPzozJmBXHEDrKDZAjHPiyA1', N'Bruce', N'Wayne', N'batman@gotham.com', N'The Dark Knight!', '2019-02-27T17:27:37.6910298+01:00', '85e5f397-ac3a-e911-a7eb-58fb840515da'),
	('99fbea2e-10ed-7a2b-0349-01692fc963b1', 3, N'joker', N'6YhQLcYh2a6MOuFlfburf5yTgm3+5f1n1D3D5D8crwCD8GV7', N'Jack', N'Napier', N'joker@gotham.com', N'Smile, and your life will smile you as well!', '2019-02-27T17:28:41.2652968+01:00', '85a0b4bd-ac3a-e911-a7eb-58fb840515da'),
	('ebd46c2a-3c09-7ce9-54cd-016a35b370d5', 1003, N'catwoman', N'QGRXQRmqF5IiL3TBh3FprgImNDiBvEBbkQ26B4gR8veGE8cS', N'Selina', N'Kyle', N'selina@gotham.com', N'Say "Meow!" seven times.', '2019-04-19T15:05:13.4292010+02:00', '990279c4-a362-e911-a809-58fb840515da'),
	('595d7ab3-6581-2df1-92a4-016d3aa075e0', 1007, N'riddler', N'J2EFJXv1jypBU7frFYKyD8o3PbfKhEc9uSHaYoJtd10fPSeG', N'Edward', N'Nygma', N'riddler@gotham.com', N'Guess who!', '2019-09-16T17:10:57.5046807+02:00', NULL),
	('7b1f0159-20df-9aad-f081-016f7266a0f6', 1008, N'penguin', N'u0NGsaO+l1HEeB90SY3VOb61TXrPuxDKpTatCf3oQbDiiYiC', N'Oswald', N'Cobblepot', N'mayor@gotham.com', N'A penguin is a bird that cannot fly! I am a man.', '2020-01-04T22:12:06.1342688+01:00', NULL)

INSERT INTO [dbo].[User] ([ID], [Nickname], [Password], [Name], [Surname], [Email], [Biography], [RegistrationDate], [ProfilePictureGuid])
SELECT s.[ID], s.[Nickname], s.[Password], s.[Name], s.[Surname], s.[Email], s.[Biography], s.[RegistrationDate], s.[ProfilePictureGuid]
FROM @SampleUser s
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[User] u WHERE u.[Nickname] = s.[Nickname] OR u.[Email] = s.[Email])
ORDER BY s.[SampleID]

DECLARE @UserID TABLE ([SampleID] int PRIMARY KEY, [ID] uniqueidentifier)
INSERT INTO @UserID
SELECT s.[SampleID], u.[ID] FROM @SampleUser s INNER JOIN [dbo].[User] u ON u.[Nickname] = s.[Nickname]

DECLARE @SamplePost TABLE ([ID] uniqueidentifier, [SampleID] int PRIMARY KEY, [SampleUserID] int, [Text] varchar(500), [PostDate] datetimeoffset(7), [PostGuid] uniqueidentifier)
INSERT INTO @SamplePost VALUES
	('498b88fe-408f-7cbf-30a1-01691552c8dc', 1, 1, N'Foggy street', '2019-02-22T14:09:00.7640603+01:00', 'c7ab5008-a336-e911-a7eb-58fb840515da'),
	('05eabce7-35d9-4c1f-ea8f-016938b73bac', 2, 2, N'Typical night on the Gotham''s roof. @joker #vigilante', '2019-03-01T11:05:26.3168209+01:00', '66c69388-093c-e911-a7ec-58fb840515da'),
	('13da9613-5393-b13c-6f95-016a2fe6ff2f', 1002, 1, N'Sembro quasi una persona seria... #seminar #talk', '2019-04-18T12:03:48.9113244+02:00', 'ce2a5442-c161-e911-a805-58fb840515da'),
	('a9696782-c60f-e80e-fb9d-016a35b6d26c', 1003, 1003, N'Myself! #girl #hot', '2019-04-19T15:08:55.0202857+02:00', '499f7148-a462-e911-a809-58fb840515da'),
	('503907ca-dfad-be46-9db3-0170a79ee8a3', 1004, 1, N'Vrooom! Vrooom! #tachometer #tripcomputer #bmw', '2020-03-04T23:16:14.2431950+01:00', 'fffd7969-75cd-44a5-d970-08d7c089a714')

INSERT INTO [dbo].[Post] ([ID], [UserID], [Text], [PostDate], [PostGuid])
SELECT s.[ID], u.[ID], s.[Text], s.[PostDate], s.[PostGuid]
FROM @SamplePost s
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Post] p WHERE p.[PostGuid] = s.[PostGuid])
ORDER BY s.[SampleID]

DECLARE @PostID TABLE ([SampleID] int PRIMARY KEY, [ID] uniqueidentifier)
INSERT INTO @PostID
SELECT s.[SampleID], p.[ID] FROM @SamplePost s INNER JOIN [dbo].[Post] p ON p.[PostGuid] = s.[PostGuid]

INSERT INTO [dbo].[Comment] ([ID], [Text], [UserID], [PostID], [CommentDate])
SELECT s.[ID], s.[Text], u.[ID], p.[ID], s.[CommentDate]
FROM (VALUES
	('17a53ef8-4320-13ad-b888-016947cf4092', 4, N'I feel safe now! #gothamcitizen', 1, 2, '2019-03-04T09:25:58.6743344+01:00'),
	('15e658a4-5843-4184-9dca-01694968f94f', 5, N'Good place where to put a bomb!', 3, 1, '2019-03-04T16:53:30.1914165+01:00'),
	('4703bfe7-b018-5241-4331-016949763c1f', 7, N'If you try, I''ll stop you!', 2, 1, '2019-03-04T17:07:59.2631514+01:00'),
	('1949fb86-26ed-fe9a-efb3-0169fd6f177f', 1004, N'Anyway, a great view of the city! #gotham', 1, 2, '2019-04-08T16:51:50.0155919+02:00'),
	('83d8c1cf-fe68-2351-bb99-0169fd766712', 1005, N'Safe?? The best has yet to come! :-) #fool', 3, 2, '2019-04-08T16:59:49.1386881+02:00'),
	('290e4acc-db54-21cc-1cbb-0169fd806702', 1006, N'@batman, do you need a #Robin?', 1, 2, '2019-04-08T17:10:44.4828749+02:00'),
	('e495e960-1e39-9b98-9623-016a2fe7d969', 1007, N'@alby che figo!', 2, 1002, '2019-04-18T12:04:44.7773284+02:00'),
	('6bdbf361-092d-dc00-2512-016a2fe8b9e5', 1008, N'@batman avrei preferito me lo dicesse Vicky Vale... :-)', 1, 1002, '2019-04-18T12:05:42.2458633+02:00')
) s ([ID], [SampleID], [Text], [SampleUserID], [SamplePostID], [CommentDate])
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
	INNER JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Comment] c WHERE c.[UserID] = u.[ID] AND c.[PostID] = p.[ID] AND c.[CommentDate] = s.[CommentDate])
ORDER BY s.[SampleID]

INSERT INTO [dbo].[Follow] ([ID], [FollowerID], [FollowedID], [FollowDate])
SELECT s.[ID], follower.[ID], followed.[ID], s.[FollowDate]
FROM (VALUES
	('898809ad-5bfa-077a-253f-0169eccccde0', 2, 1, '2019-04-05T11:20:38.8805832+02:00')
) s ([ID], [SampleFollowerID], [SampleFollowedID], [FollowDate])
	INNER JOIN @UserID follower ON follower.[SampleID] = s.[SampleFollowerID]
	INNER JOIN @UserID followed ON followed.[SampleID] = s.[SampleFollowedID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Follow] f WHERE f.[FollowerID] = follower.[ID] AND f.[FollowedID] = followed.[ID])
ORDER BY s.[FollowDate]

INSERT INTO [dbo].[Like] ([ID], [PostID], [UserID], [LikeDate])
SELECT s.[ID], p.[ID], u.[ID], s.[LikeDate]
FROM (VALUES
	('f23125ec-580c-a595-6470-01692fca9de7', 2, 2, 3, '2019-02-27T17:30:01.7030482+01:00'),
	('38634fdf-4d73-7041-fae5-016938b7d4d9', 1002, 1, 2, '2019-03-01T11:06:05.5291670+01:00'),
	('1b0a5fef-c82e-2415-a0a7-01694975c644', 2003, 2, 1, '2019-03-04T17:07:29.0925160+01:00'),
	('c4398a8b-f740-f218-58f7-016953b1d12d', 3002, 1, 3, '2019-03-06T16:49:16.2051776+01:00'),
	('18a0f89a-e49d-4441-2516-016a35b8ac55', 5003, 1, 1003, '2019-04-19T15:10:56.3414257+02:00')
) s ([ID], [SampleID], [SamplePostID], [SampleUserID], [LikeDate])
	INNER JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
	INNER JOIN @UserID u ON u.[SampleID] = s.[SampleUserID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Like] l WHERE l.[PostID] = p.[ID] AND l.[UserID] = u.[ID])
ORDER BY s.[SampleID]

INSERT INTO [dbo].[Notification] ([ID], [SenderID], [RecipientID], [ReadByRecipient], [NotificationDate], [Kind], [PostID])
SELECT s.[ID], sender.[ID], recipient.[ID], s.[ReadByRecipient], s.[NotificationDate], s.[Kind], p.[ID]
FROM (VALUES
	('60921e48-5b21-b061-158a-0169525aa2e4', 1, 1, 2, 1, '2019-03-06T10:34:25.5086997+01:00', 2, 2),
	('b8db8a96-481a-e432-136d-016957e269c6', 1002, 1, 2, 1, '2019-03-07T12:20:49.8621949+01:00', 2, 2),
	('1e9f5c7a-256d-1b58-3a0e-016957e4ce9f', 1003, 1, 2, 1, '2019-03-07T12:23:26.7512792+01:00', 2, 2),
	('cad8b947-67e2-5a03-d03c-01695d1e315b', 1004, 3, 1, 1, '2019-03-08T12:44:13.6591069+01:00', 1, NULL),
	('830469fe-08ee-f024-97e8-01699be9aa5f', 2004, 1, 2, 1, '2019-03-20T17:22:55.8397809+01:00', 2, 2),
	('1934cf8a-c33f-387d-57dc-01699bea8c7c', 2005, 1, 2, 1, '2019-03-20T17:23:53.7246746+01:00', 2, 2),
	('a015b8f6-657d-2667-c44c-01699bea9830', 2006, 1, 2, 1, '2019-03-20T17:23:56.7202411+01:00', 2, 2),
	('a5836abc-6f49-c7e2-426a-01699bebfda2', 2007, 1, 2, 1, '2019-03-20T17:25:28.2260180+01:00', 2, 2),
	('a1e2b1ab-945a-adfb-ed4f-0169a00ef856', 3004, 1, 2, 1, '2019-03-21T12:42:09.4944389+01:00', 2, 2),
	('e7f39c62-39df-66e5-d52c-0169a00ef856', 3005, 1, 2, 1, '2019-03-21T12:42:09.4944389+01:00', 2, 2),
	('423b50dd-6744-545b-562d-0169a00effee', 3006, 1, 2, 1, '2019-03-21T12:42:11.4381371+01:00', 2, 2),
	('fc5e9bc8-b235-bf84-77ae-0169a00f0fc7', 3007, 1, 2, 1, '2019-03-21T12:42:15.4958474+01:00', 2, 2),
	('7560f357-d827-cf93-95ba-0169a00f1d41', 3008, 1, 2, 1, '2019-03-21T12:42:18.9453966+01:00', 2, 2),
	('8d6619e6-8c88-5616-85a2-0169de5cef0e', 3009, 1, 2, 1, '2019-04-02T16:03:46.3188284+02:00', 2, 2),
	('a285d30b-d6de-ed0a-0a3a-0169ecccd02f', 4009, 1, 2, 1, '2019-04-05T11:20:39.4717892+02:00', 2, 2),
	('ecd2fb32-8547-d001-f487-0169fd6f1a0a', 4010, 1, 2, 1, '2019-04-08T16:51:50.6667206+02:00', 3, 2),
	('85fe42ff-9421-7745-6383-0169fd75c405', 4011, 3, 2, 1, '2019-04-08T16:59:07.3977874+02:00', 1, NULL),
	('07deb8b7-4176-d978-68b3-0169fd766822', 4012, 3, 2, 1, '2019-04-08T16:59:49.4102772+02:00', 3, 2),
	('193c7cb7-3748-987f-5bd1-0169fd806a07', 4013, 1, 2, 1, '2019-04-08T17:10:45.2559725+02:00', 3, 2),
	('badc9ad6-5b73-4a13-bf6e-016a2fe7da87', 4014, 2, 1, 1, '2019-04-18T12:04:45.0633337+02:00', 3, 1002),
	('acd83dc5-8ec6-a15c-9267-016a2fea8320', 4016, 2, 1, 1, '2019-04-18T12:07:39.2961947+02:00', 2, 1002),
	('147ac4e5-2586-5ddc-bdca-016a35b8ad4d', 4017, 1, 1003, 0, '2019-04-19T15:10:56.5890046+02:00', 1, NULL)
) s ([ID], [SampleID], [SampleSenderID], [SampleRecipientID], [ReadByRecipient], [NotificationDate], [Kind], [SamplePostID])
	INNER JOIN @UserID sender ON sender.[SampleID] = s.[SampleSenderID]
	INNER JOIN @UserID recipient ON recipient.[SampleID] = s.[SampleRecipientID]
	LEFT JOIN @PostID p ON p.[SampleID] = s.[SamplePostID]
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Notification] n WHERE n.[SenderID] = sender.[ID] AND n.[RecipientID] = recipient.[ID] AND n.[Kind] = s.[Kind] AND n.[NotificationDate] = s.[NotificationDate])
ORDER BY s.[SampleID]

COMMIT TRANSACTION
