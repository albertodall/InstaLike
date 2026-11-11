#Requires -Version 7
<#
.SYNOPSIS
    Uploads the sample pictures to the blob containers the application reads them from.
.PARAMETER ConnectionString
    The storage account to upload to. Defaults to the local Azurite emulator.
.PARAMETER AccountName
    Uploads with your Microsoft Entra login (az login) instead of a connection string.
#>
[CmdletBinding(DefaultParameterSetName = 'ConnectionString')]
param(
    [Parameter(ParameterSetName = 'ConnectionString')]
    [string] $ConnectionString = 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;',

    [Parameter(Mandatory, ParameterSetName = 'EntraID')]
    [string] $AccountName
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$account = if ($PSCmdlet.ParameterSetName -eq 'EntraID') {
    @('--account-name', $AccountName, '--auth-mode', 'login')
} else {
    @('--connection-string', $ConnectionString)
}

foreach ($container in 'posts', 'profiles') {
    az storage container create --name $container @account --only-show-errors | Out-Null
    az storage blob upload-batch --destination $container --source (Join-Path $PSScriptRoot 'pictures' $container) `
        --content-type image/jpeg @account --no-progress --only-show-errors | Out-Null
    Write-Host "Uploaded the pictures to '$container'."
}
