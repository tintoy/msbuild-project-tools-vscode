$dotnet = Get-Command 'dotnet'

$serverRoot = Join-Path $PSScriptRoot 'lib\server'
$publishRoot = Join-Path $PSScriptRoot 'out'

& $dotnet restore "$serverRoot\MSBuildProjectTools.sln"
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -o "$publishRoot\language-server"
& $dotnet publish "$serverRoot\src\LanguageServer.TaskReflection\LanguageServer.TaskReflection.csproj" -o "$publishRoot\task-reflection"