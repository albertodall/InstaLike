ALTER DATABASE [$(DatabaseName)]
    ADD FILEGROUP [Pictures] CONTAINS FILESTREAM;
GO

ALTER DATABASE [$(DatabaseName)]
ADD FILE
(
    NAME = [InstaLike_Pictures],
    FILENAME = '$(DBDataPath)\$(DatabaseName)\$(PicturesFileStream)',
    MAXSIZE = UNLIMITED
) TO FILEGROUP [Pictures]
GO
