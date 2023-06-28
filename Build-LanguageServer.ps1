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
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -o "$publishRoot\language-server" /p:VersionPrefix="$VersionPrefix" /p:VersionSuffix="$($VersionSuffix)"
& $dotnet publish "$serverRoot\src\LanguageServer.TaskReflection\LanguageServer.TaskReflection.csproj" -o "$publishRoot\task-reflection" /p:VersionPrefix="$VersionPrefix" /p:VersionSuffix="$($VersionSuffix)"
