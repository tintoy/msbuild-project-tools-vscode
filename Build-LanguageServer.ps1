Param(
    [Parameter(Mandatory=$true)]
    [string] $VersionPrefix,

    [Parameter()]
    [string] $VersionSuffix = ''
)

$dotnet = Get-Command 'dotnet'

$serverRoot = Join-Path $PSScriptRoot 'lib\server'
$publishRoot = Join-Path $PSScriptRoot 'out'

& $dotnet restore "$serverRoot\MSBuildProjectTools.sln" /p:VersionPrefix="$VersionPrefix" /p:VersionSuffix="$($VersionSuffix)"
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -f netcoreapp2.0 -o "$publishRoot\language-server" /p:VersionPrefix="$VersionPrefix" /p:VersionSuffix="$($VersionSuffix)"
& $dotnet publish "$serverRoot\src\LanguageServer.TaskReflection\LanguageServer.TaskReflection.csproj" -f netcoreapp2.0 -o "$publishRoot\task-reflection" /p:VersionPrefix="$VersionPrefix" /p:VersionSuffix="$($VersionSuffix)"
