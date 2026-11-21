#Requires -Version 7.3

<#
.SYNOPSIS
    Analyzes the solution on SonarCloud, with test results and code coverage.

.DESCRIPTION
    Builds the solution, runs every test project with code coverage and sends the analysis to SonarCloud.
    The SonarCloud token is read from the SONAR_TOKEN environment variable and never written to disk or to the output.

.EXAMPLE
    $env:SONAR_TOKEN = '<token>'; ./sonarcloud.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$token = $env:SONAR_TOKEN
if ([string]::IsNullOrWhiteSpace($token)) {
    throw 'Set the SONAR_TOKEN environment variable to a SonarCloud token before running the analysis.'
}

$solution = 'InstaLike.slnx'
$results = Join-Path $PSScriptRoot 'TestResults' 'SonarCloud'

Push-Location $PSScriptRoot
try {
    if (Test-Path $results) {
        Remove-Item $results -Recurse -Force
    }

    dotnet tool restore

    dotnet sonarscanner begin `
        /k:"albertodall_InstaLike" `
        /n:"InstaLike" `
        /o:"albertodall-github" `
        /d:sonar.host.url="https://sonarcloud.io" `
        /d:sonar.token="$token" `
        /d:sonar.exclusions="**/bin/**/*,**/obj/**/*,**/wwwroot/lib/**" `
        /d:sonar.coverage.exclusions="tests/**,**/*Fixture.cs" `
        /d:sonar.cs.vscoveragexml.reportsPaths="$results/**/*.xml" `
        /d:sonar.cs.vstest.reportsPaths="$results/**/*.trx" `
        /d:sonar.msbuild.testProjectPattern="^.+Tests$"

    dotnet build $solution
    dotnet test --solution $solution --no-build --results-directory $results --report-xunit-trx --coverage --coverage-output-format xml

    dotnet sonarscanner end /d:sonar.token="$token"
}
finally {
    Pop-Location
}
