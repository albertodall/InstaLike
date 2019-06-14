@echo off
set /p token=<sonar.token
dotnet sonarscanner begin /k:"albertodall_InstaLike" /n:"InstaLike" /v:"1.0.0" /o:"albertodall-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="%token%" /d:sonar.language="cs" /d:sonar.exclusions="**/bin/**/*,**/obj/**/*,**/wwwroot/lib/**" /d:sonar.coverage.exclusions="InstaLike.Core.Tests/**,InstaLike.IntegrationTests/**,**/*Fixture.cs" /d:sonar.cs.opencover.reportsPaths="%cd%\lcov.opencover.xml" /d:sonar.msbuild.testProjectPattern="^.+Tests$"
dotnet restore InstaLike.sln
dotnet build InstaLike.sln
dotnet test InstaLike.sln /p:CollectCoverage=true /p:CoverletOutputFormat=\"opencover,lcov\" /p:CoverletOutput=../lcov
dotnet sonarscanner end /d:sonar.login="%token%"
