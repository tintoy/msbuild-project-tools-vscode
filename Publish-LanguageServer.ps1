$dotnet = Get-Command 'dotnet'

if ($args -contains '--dev') {
    $buildConfiguration = 'Debug'
} else {
    $buildConfiguration = 'Release'
}

Write-Output "Chosen build configuration: $buildConfiguration"

$serverRoot = Join-Path $PSScriptRoot 'lib\server'
$publishRoot = Join-Path $PSScriptRoot 'out'

& $dotnet restore "$serverRoot\MSBuildProjectTools.sln"
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -o "$publishRoot\language-server" -c $buildConfiguration
