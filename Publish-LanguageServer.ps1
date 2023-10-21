$dotnet = Get-Command 'dotnet'

$latestTag = git describe --tags --abbrev=0

if ($latestTag.StartsWith('v')) {
    $latestTag = $latestTag.Substring(1)
}

$parts = $latestTag.Split('-')

$versionPrefix = $parts[0]

if ($parts.Count -gt 1) {
    $versionSuffix = $parts[1]
}

if ($args -contains '--dev') {
    $buildConfiguration = 'Debug'
    if ($versionSuffix -eq '') {
        $versionSuffix = "$versionSuffix-"
    }
    $versionSuffix = $versionSuffix + 'dev'
} else {
    $buildConfiguration = 'Release'
}

$latestTagFull = git describe --tags
$latestTagShort = git describe --tags --abbrev=0

if ($latestTagFull -ceq $latestTagShort) {
    $numberOfCommits = 1
} else {
    $diff = $latestTagFull -replace $latestTagShort, ""
    $numberOfCommits = $diff.Split('-')[1]
}

$fileVersion = "$versionPrefix.$numberOfCommits"

Write-Output "Chosen build configuration: $buildConfiguration"

$serverRoot = Join-Path $PSScriptRoot 'lib\server'
$publishRoot = Join-Path $PSScriptRoot 'out'

& $dotnet restore "$serverRoot\MSBuildProjectTools.sln"
& $dotnet publish "$serverRoot\src\LanguageServer\LanguageServer.csproj" -o "$publishRoot\language-server" /p:VersionPrefix="$versionPrefix" /p:VersionSuffix="$versionSuffix" /p:FileVersion="$fileVersion" -c $buildConfiguration
