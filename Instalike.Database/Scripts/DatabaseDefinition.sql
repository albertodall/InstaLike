USE [master]
GO

CREATE DATABASE [InstaLike]
ON PRIMARY 
( 
    NAME = N'InstaLike_Data', FILENAME = N'D:\SqlDb\InstaLike\InstaLike_Data.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB 
), 
FILEGROUP [Pictures] CONTAINS FILESTREAM  DEFAULT
( 
    NAME = N'InstaLike_Pictures', FILENAME = N'D:\SqlDb\InstaLike\Pictures' , MAXSIZE = UNLIMITED
)
LOG ON 
( 
    NAME = N'InstaLike_Log', FILENAME = N'D:\SqlDb\InstaLike\InstaLike_Log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB 
)
GO
ALTER DATABASE [InstaLike] SET COMPATIBILITY_LEVEL = 140
GO
ALTER DATABASE [InstaLike] SET ANSI_NULL_DEFAULT ON 
ALTER DATABASE [InstaLike] SET ANSI_NULLS ON 
ALTER DATABASE [InstaLike] SET ANSI_PADDING ON 
ALTER DATABASE [InstaLike] SET ANSI_WARNINGS ON 
ALTER DATABASE [InstaLike] SET ARITHABORT ON 
ALTER DATABASE [InstaLike] SET AUTO_CLOSE OFF 
ALTER DATABASE [InstaLike] SET AUTO_SHRINK OFF 
ALTER DATABASE [InstaLike] SET AUTO_UPDATE_STATISTICS ON 
ALTER DATABASE [InstaLike] SET CURSOR_CLOSE_ON_COMMIT OFF 
ALTER DATABASE [InstaLike] SET CURSOR_DEFAULT  LOCAL 
ALTER DATABASE [InstaLike] SET CONCAT_NULL_YIELDS_NULL ON 
ALTER DATABASE [InstaLike] SET NUMERIC_ROUNDABORT OFF 
ALTER DATABASE [InstaLike] SET QUOTED_IDENTIFIER ON 
ALTER DATABASE [InstaLike] SET RECURSIVE_TRIGGERS OFF 
ALTER DATABASE [InstaLike] SET  DISABLE_BROKER 
ALTER DATABASE [InstaLike] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
ALTER DATABASE [InstaLike] SET DATE_CORRELATION_OPTIMIZATION OFF 
ALTER DATABASE [InstaLike] SET TRUSTWORTHY OFF 
ALTER DATABASE [InstaLike] SET ALLOW_SNAPSHOT_ISOLATION OFF 
ALTER DATABASE [InstaLike] SET PARAMETERIZATION SIMPLE 
ALTER DATABASE [InstaLike] SET READ_COMMITTED_SNAPSHOT OFF 
ALTER DATABASE [InstaLike] SET HONOR_BROKER_PRIORITY OFF 
ALTER DATABASE [InstaLike] SET RECOVERY SIMPLE 
ALTER DATABASE [InstaLike] SET  MULTI_USER 
ALTER DATABASE [InstaLike] SET PAGE_VERIFY NONE  
ALTER DATABASE [InstaLike] SET DB_CHAINING OFF 
ALTER DATABASE [InstaLike] SET FILESTREAM( NON_TRANSACTED_ACCESS = FULL ) 
ALTER DATABASE [InstaLike] SET TARGET_RECOVERY_TIME = 0 SECONDS 
ALTER DATABASE [InstaLike] SET DELAYED_DURABILITY = DISABLED 
EXEC sys.sp_db_vardecimal_storage_format N'InstaLike', N'ON'
ALTER DATABASE [InstaLike] SET QUERY_STORE = OFF
ALTER DATABASE [InstaLike] SET READ_WRITE 
GO
