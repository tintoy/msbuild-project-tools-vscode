$dotnet = Get-Command 'dotnet'

$serverRoot = Join-Path $PSScriptRoot 'lib\server'
$publishRoot = Join-Path $PSScriptRoot 'out'

& $dotnet restore "$serverRoot\MSBuildProjectTools.sln"
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -f netcoreapp2.0 -o "$publishRoot\language-server"
& $dotnet publish "$serverRoot\src\LanguageServer.TaskReflection\LanguageServer.TaskReflection.csproj" -f netcoreapp2.0 -o "$publishRoot\task-reflection"
