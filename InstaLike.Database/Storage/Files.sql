ALTER DATABASE [$(DatabaseName)]
ADD FILE
(
	NAME = InstaLike_Data,
	FILENAME = '$(DBDataPath)\$(DatabaseName)\$(DatabaseName)_Data.mdf',
	SIZE = 10240KB,
	FILEGROWTH = 5120KB
) TO FILEGROUP [PRIMARY]
GO

ALTER DATABASE [$(DatabaseName)]
ADD LOG FILE
(
	NAME = InstaLike_Log,
	FILENAME = '$(DBLogPath)\$(DatabaseName)\$(DatabaseName)_Log.ldf',
	SIZE = 5120KB,
	FILEGROWTH = 5120KB
)
GO