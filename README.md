# InstaLike
This project was born, more or less, two years ago.  
I was teaching a C# Course in a High School, and I needed a "cool" project to sum up all the concepts I explained throughout the course.
Since (lucky or not) a Social Network is "cool by default", I decided to create a simple [Instagram](https://www.instagram.com) clone.  
The guys attending the course appreciated the idea a lot, so I did my best to create a working web project that was also simple but with a significant set of features.  
The original project was a .NET Framework 4.6.2 web application created using ASP.NET MVC 4.0 with Entity Framework 6.2 and SQL Server 2014 Express as backing store, and, since I had a lot of fun creating it, I decided to evolve it a little bit...

## Evolution
The project looked interesting, so I decided to use it as my preferred playground for testing new technologies and patterns. Today *InstaLike* is a web application built using some interesting stuff:
 - Domain Driven Design. 
 - ASP.NET Core.
 - CQRS Pattern. 
 - O/RM Based persistence 
 - Unit testing and integration testing. 
 - Unstructured data (images) storage.
 - Activity logging.
 - Cloud-based image recognition.

## Features
 - User registration. 
 - Sharing of pictures. 
 - Put "Likes" on shared pictures. 
 - Comments on shared pictures. 
 - Follow and unfollow users. 
 - Get notifications when other users interact with the content you shared.
 - Pictures Auto-tagging.

## Requirements
To work with *InstaLike* source code, you need:
 - [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) 2.2.
 - [Visual Studio](https://visualstudio.microsoft.com/) 2017 or greater (the free [Community Edition](https://visualstudio.microsoft.com/vs/community/) is enough), or the IDE you like.
 - [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) 2014 Express or greater.

## Setup
1. Download the code.
2. Install the [dotnet.exe](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet)-based [FluentMigrator Runner](https://fluentmigrator.github.io/articles/runners/dotnet-fm.html)
```
dotnet tool install -g FluentMigrator.DotNet.Cli
```
3. Build *InstaLike.Database* project
```
cd InstaLike.Database
dotnet build Instalike.Database.csproj
```
4. Create the database using the provided script
```
cd bin\Debug\netstandard2.0\Scripts
sqlcmd -S (local) -E -i DatabaseDefinition.sql -v DatabaseName="InstaLike" StoragePath="<path where you want to store data files>"
```
5. Run migrations to create database objects
```
dotnet fm migrate --processor SqlServer2014 --connection "Data Source=(local); Initial Catalog=InstaLike; Integrated Security=True" --tag SqlServerOnPrem --assembly Instalike.Database.dll up
```
6. Build and run the solution

## Acknowledgements
This project has been built using these awesome Open Source projects:

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [MediatR](https://github.com/jbogard/MediatR)
- [NHibernate](https://github.com/nhibernate/nhibernate-core)
- [FluentNHibernate](https://github.com/FluentNHibernate/fluent-nhibernate)
- [FluentMigrator](https://fluentmigrator.github.io)
- [Xunit](https://xunit.net)
- [FluentAssertions](https://fluentassertions.com)
- [Serilog](https://serilog.net)

Many thanks  also to my friend and fellow worker [Matteo](https://github.com/cefla) for the graphic design. I gave him some rough stuff and he made it beautiful!
