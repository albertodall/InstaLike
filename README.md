# InstaLike
This project was born, more or less, two years ago. 
I was teaching a C# Course in an High School, and I needed a "cool" project to sum up all the concepts I explained throughout the course, and having a lot of fun! :-)
Since (lucky or not) a Social Network is "cool by default", I decided to create a simple [Instagram](https://www.instagram.com) clone.
The guys attending the course appreciated the idea a lot, so I did my best to create a working web project that was also simple but with a significant set of features.
The original project was a .NET Framework 4.6.2 web application created using ASP.NET MVC 4.0 with Entity Framework 6.2 and SQL Server 2014 Express as backing store.

## Evolution
The project looked interesting, so I decided to use it as my preferred playground for testing new technologies and patterns. Today *InstaLike* is a web application built using some interesting stuff:
 - Domain Driven Design. 
 - ASP.NET Core.
 - CQRS Pattern. 
 - O/RM Based persistence 
 - Unit testing and integration testing. 
 - Unstructured data (images) storage.

## Features
 - User registration. 
 - Sharing of pictures. 
 - Put "Likes" on shared pictures. 
 - Comments on shared pictures. 
 - Follow and unfollow users. 
 - Get notifications when other users interact with the content you shared.

## Requirements
To work with *InstaLike* source code, you need:
 - [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) 2.2
 - [Visual Studio](https://visualstudio.microsoft.com/) 2017 or greater, or the IDE you like. For VS, the Community Edition is enough-
 - [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) 2014 Express or greater.

## Setup
Download the code.
Install the [dotnet.exe](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet)-based [FluentMigrator Runner](https://fluentmigrator.github.io/articles/runners/dotnet-fm.html)
```
dotnet tool install -g FluentMigrator.DotNet.Cli
```
Build *InstaLike.Database* project:
```
cd InstaLike.Database
dotnet build Instalike.Database.csproj
```
Create the database using the provided script:
```
cd bin\Debug\netstandard2.0
sqlcmd -S (local) -E -i DatabaseDefinition.sql -v DatabaseName="InstaLike" StoragePath="<path where you want to store database's datafiles>"
```
Run migration stages, to create database objects:
```
dotnet fm migrate --processor SqlServer2014 --connection "Data Source=(local); Initial Catalog=InstaLike; Integrated Security=True" --tag SqlServerOnPrem --assembly Instalike.Database.dll up
```
Build the solution
## Acknowledgements
