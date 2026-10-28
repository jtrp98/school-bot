$ErrorActionPreference = 'Stop'
$schoolBotRepository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$schoolBotFixture = Join-Path ([IO.Path]::GetTempPath()) "school-bot-source-test-$([guid]::NewGuid())"
New-Item -ItemType Directory -Path "$schoolBotFixture/scripts","$schoolBotFixture/app" -Force | Out-Null
try {
    foreach ($schoolBotName in @('common.ps1','package-source.ps1')) {
        Copy-Item -LiteralPath (Join-Path $schoolBotRepository "scripts/$schoolBotName") `
            -Destination (Join-Path $schoolBotFixture "scripts/$schoolBotName")
    }
    foreach ($schoolBotName in @('README.md','AGENTS.md','CLAUDE.md','app/source.go')) {
        [IO.File]::WriteAllText((Join-Path $schoolBotFixture $schoolBotName), 'fixture')
    }
    foreach ($schoolBotName in @(
        '.env','.env.production','signing.pfx','certificate.pem','private.key','signing.p12',
        'assembly.snk','symbols.pdb','source.map','test.test','certificate.crt','certificate.cer',
        'certificate.der','store.jks','store.keystore','appsettings.Production.json'
    )) {
        [IO.File]::WriteAllText((Join-Path $schoolBotFixture "app/$schoolBotName"), 'excluded fixture')
    }
    foreach ($schoolBotDirectory in @(
        'app/node_modules','app/bin','app/.build','app/frontend/public/blockly-media','web/data'
    )) {
        New-Item -ItemType Directory -Path (Join-Path $schoolBotFixture $schoolBotDirectory) -Force | Out-Null
        [IO.File]::WriteAllText((Join-Path $schoolBotFixture "$schoolBotDirectory/excluded.txt"), 'excluded fixture')
    }
    $schoolBotJunction = Join-Path $schoolBotFixture 'app/loop'
    New-Item -ItemType Junction -Path $schoolBotJunction -Target $schoolBotFixture | Out-Null
    & (Join-Path $schoolBotFixture 'scripts/package-source.ps1')
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $schoolBotArchive = [IO.Compression.ZipFile]::OpenRead(
        (Join-Path $schoolBotFixture 'dist/school-bot-team-starter.zip')
    )
    try {
        $schoolBotNames = @($schoolBotArchive.Entries.FullName | Sort-Object)
        $schoolBotExpected = @(
            'AGENTS.md','CLAUDE.md','README.md','app/source.go','scripts/common.ps1','scripts/package-source.ps1'
        ) | Sort-Object
        if (Compare-Object $schoolBotExpected $schoolBotNames) {
            throw "Unexpected archive entries: $($schoolBotNames -join ', ')"
        }
    } finally { $schoolBotArchive.Dispose() }
    Write-Host 'Source package tests passed: rules included, secrets/artifacts pruned, junction skipped.'
} finally {
    $schoolBotAbsolute = [IO.Path]::GetFullPath($schoolBotFixture)
    $schoolBotTempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $schoolBotAbsolute.StartsWith($schoolBotTempRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture escaped the temp directory: $schoolBotAbsolute"
    }
    # Unlink the cycle before cleanup so deletion cannot traverse outside the fixture.
    if (Test-Path -LiteralPath "$schoolBotFixture/app/loop") {
        [IO.Directory]::Delete("$schoolBotFixture/app/loop")
    }
    $schoolBotLinks = Get-ChildItem -LiteralPath $schoolBotAbsolute -Recurse -Force |
        Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint }
    if ($schoolBotLinks) { throw 'Unexpected fixture link; refusing recursive cleanup.' }
    Remove-Item -LiteralPath $schoolBotAbsolute -Recurse -Force
}
