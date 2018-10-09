<#
    ### How to use
    
    # Build locally
    PS> ./build.ps1 -target Default -build 123 -revision xyz -octopusUrl https://myoctopus.com -octopusApiKey xyz

    # Build with TeamCity PowerShell runner and the following settings:
    Format stderr:    error
    Script:           Source code
    Script source:
        .\build.ps1 -target Default -build %build.counter% -revision %build.vcs.number% -octopusUrl https://myoctopus.com -octopusApiKey xyz
        exit $LASTEXITCODE

#>

[CmdletBinding()]
Param(
    [string]$target = "Default",
    [string]$build = "0",
    [string]$revision = "",
    [string]$octopusUrl = "",
    [string]$octopusApiKey = ""    
)

If (Test-Path tools){
    Remove-Item tools -Force -Recurse
}
mkdir tools

'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.1</TargetFramework></PropertyGroup></Project>' > tools\build.csproj

dotnet add tools/build.csproj package Cake.CoreCLR --package-directory tools

$pathToCakeDll = (Get-Item tools/cake.coreclr/*/Cake.dll).FullName

dotnet $pathToCakeDll build.cake -target="$target" -build="$build" -revision="$revision" -octopusUrl="$octopusUrl" -octopusApiKey="$octopusApiKey"