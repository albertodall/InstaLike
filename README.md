# InstaLike

## Code analysis 
| Metric |  |
|-----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Lines of code | [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=albertodall_InstaLike&metric=ncloc)](https://sonarcloud.io/dashboard?id=albertodall_InstaLike) |
| Maintainability | [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=albertodall_InstaLike&amp;metric=sqale_rating)](https://sonarcloud.io/dashboard?id=albertodall_InstaLike) |
| Reliability | [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=albertodall_InstaLike&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=albertodall_InstaLike) |
| Security | [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=albertodall_InstaLike&metric=security_rating)](https://sonarcloud.io/dashboard?id=albertodall_InstaLike) |

## Once upon a time...
This project was born, more or less, in April 2016.  
I was teaching a C# Course in a High School, and I needed a "cool" project to sum up all the concepts I explained throughout the course.
Since (lucky or not) a Social Network is "cool by default", I decided to create a simple [Instagram](https://www.instagram.com) clone.  
The guys attending the course appreciated the idea a lot, so I did my best to create a working web project that was also simple but with a significant set of features.  
The original project was a .NET Framework 4.6.2 web application created using ASP.NET MVC 4.0 with Entity Framework 6.2 and SQL Server 2014 Express as backing store, and, since I had a lot of fun creating it, I decided to evolve it a little bit...

## Evolution
The project looked interesting, so I decided to use it as my preferred playground for testing new technologies and patterns. Today *InstaLike* is a web application built using some interesting stuff:
 - Cloud-based
 - Domain Driven Design. 
 - ASP.NET Core.
 - CQRS Pattern. 
 - O/RM Based persistence 
 - Unit testing and integration testing. 
 - Unstructured data (images) storage.
 - Activity logging.
 - Pictures auto-tagging with a vision-capable model (GPT-4.1 nano) on [Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-foundry/openai/overview).
 
## Features
 - User registration. 
 - Sharing of pictures. 
 - Put "Likes" on shared pictures. 
 - Comments on shared pictures. 
 - Follow and unfollow users. 
 - Get notifications when other users interact with the content you shared.
 - Pictures Auto-tagging.
 - Cloud storage compatibility (e.g. [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs)) for pictures.

## Requirements
To work with *InstaLike* source code, you need:
 - [.NET SDK](https://dotnet.microsoft.com/download) 10.0.
 - [Visual Studio](https://visualstudio.microsoft.com/) 2026 or greater (the free [Community Edition](https://visualstudio.microsoft.com/vs/community/) is enough), or the IDE you like.
 - [Docker Desktop](https://www.docker.com/products/docker-desktop/).

## Architecture at a glance
*InstaLike* is a cloud application: data lives in SQL Server (Azure SQL in the cloud), while pictures are stored in [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs), in the `posts` and `profiles` containers. The database keeps only the picture identifiers.
Locally, SQL Server and [Azurite](https://github.com/Azure/Azurite) (the Azure Storage emulator) run in containers.

| Service | Container | Local endpoint |
|---|---|---|
| Web application | `instalike-web` | http://localhost:5080 |
| SQL Server 2022 | `instalike-db-mssql` | `localhost,1433` (user `sa`, password `Pa55w0rd`) |
| Azurite (Blob Storage) | `instalike-storage-azurite` | http://localhost:10000 |
| Aspire Dashboard (logs, traces, metrics) | `instalike-dashboard` | http://localhost:18888 (OTLP on `localhost:4317`) |
| Database migrator (runs once) | `instalike-db-migrator` | - |

## Run everything in containers
```shell
docker compose up --build
```
The database migrator ([DbUp](https://dbup.readthedocs.io)) creates the database and applies the scripts in `InstaLike.Database.Migrator/mssql` once SQL Server is healthy, then the web application starts and creates the blob containers if they are missing.
Data is persisted under `.containers/`. Delete that folder to start from scratch.

The web application sends its logs, traces and metrics through OpenTelemetry to the [Aspire Dashboard](https://aspire.dev/dashboard/standalone/), where they can be searched and correlated by trace. In Azure, the same data goes to Application Insights.

In Visual Studio, set the `docker-compose` project as the startup project to debug the web application inside its container. The other services, including the one-shot migrator, start without the debugger.

## Run the web application from your IDE
1. Start the infrastructure only:
```shell
docker compose up -d instalike.db.mssql instalike.storage.azureblob instalike.db.migrator instalike.dashboard instalike.ai.ollama
```
2. Run the `InstaLike.Web` project with the `Development` environment: `appsettings.Development.json` points to the containerized SQL Server and to Azurite (`UseDevelopmentStorage=true`).

### Pictures auto-tagging
In Azure, pictures are tagged by GPT-4.1 nano on Azure OpenAI. Locally, a small open vision model ([Qwen2.5-VL 3B](https://ollama.com/library/qwen2.5vl)) runs for free in [Ollama](https://ollama.com), through the same OpenAI-compatible API. It starts with the rest of the stack. The first start downloads the ~3 GB model into `.containers/ollama`, and the model runs on the CPU unless Docker has GPU access.

To apply new database scripts without containers:
```shell
dotnet run --project InstaLike.Database.Migrator -- "Data Source=localhost,1433; Initial Catalog=InstaLike; User ID=sa; Password=Pa55w0rd; TrustServerCertificate=True"
```

## Tests
```shell
dotnet test
```
Integration tests run on SQLite with an in-memory picture store, so they do not need any container.

## Acknowledgements
This project has been built using these awesome Open Source projects:

- [ASP.NET Core](https://dotnet.microsoft.com/download/dotnet-core)
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [MediatR](https://github.com/jbogard/MediatR)
- [NHibernate](https://github.com/nhibernate/nhibernate-core)
- [FluentNHibernate](https://github.com/FluentNHibernate/fluent-nhibernate)
- [DbUp](https://dbup.readthedocs.io)
- [Xunit](https://xunit.net)
- [FluentAssertions](https://fluentassertions.com)
- [Serilog](https://serilog.net)
- [Humanizer](https://github.com/Humanizr/Humanizer)

Many thanks also to my friend and fellow worker [Matteo](https://github.com/cefla) for the graphic design. I gave him some rough stuff and he made it beautiful!
