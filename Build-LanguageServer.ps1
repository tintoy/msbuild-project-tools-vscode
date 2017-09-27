$dotnet = Get-Command 'dotnet'

& $dotnet publish "$PSScriptRoot\src\LanguageServer\LanguageServer.csproj" -f netcoreapp2.0 -o "$PSScriptRoot\out\language-server"
& $dotnet publish "$PSScriptRoot\src\LanguageServer.TaskReflection\LanguageServer.TaskReflection.csproj" -f netcoreapp2.0 -o "$PSScriptRoot\out\task-reflection"
