param([string]$OutputDirectory)
. "$PSScriptRoot/common.ps1"
Add-Type -AssemblyName System.IO.Compression
$schoolBotOutputDir = if ($OutputDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
} else {
    Join-Path $SchoolBotRoot 'dist'
}
New-Item -ItemType Directory -Force -Path $schoolBotOutputDir | Out-Null
$schoolBotOutput = Join-Path $schoolBotOutputDir 'school-bot-team-starter.zip'
$schoolBotFiles = [Collections.Generic.List[IO.FileInfo]]::new()
$schoolBotPending = [Collections.Generic.Stack[string]]::new()
$schoolBotExcludedDirectories = @(
    'node_modules','bin','obj','dist','TestResults','.tools','.build','.git','.agents','.codex'
)
$schoolBotExcludedExtensions = @(
    '.log','.exe','.syso','.pfx','.pem','.key','.p12','.snk','.pdb','.map','.test',
    '.cer','.crt','.der','.jks','.keystore','.msi','.pkg','.zip'
)
foreach ($schoolBotDirectory in @('app','engines','web','contracts','docs','scripts','.github')) {
    $schoolBotPath = Join-Path $SchoolBotRoot $schoolBotDirectory
    if (Test-Path -LiteralPath $schoolBotPath -PathType Container) {
        $schoolBotPending.Push($schoolBotPath)
    }
}
while ($schoolBotPending.Count -gt 0) {
    $schoolBotDirectory = Get-Item -LiteralPath $schoolBotPending.Pop() -Force
    # Junctions can escape the source root or cycle, so neither directory nor file links are followed.
    if ($schoolBotDirectory.Attributes -band [IO.FileAttributes]::ReparsePoint) { continue }
    foreach ($schoolBotItem in Get-ChildItem -LiteralPath $schoolBotDirectory.FullName -Force) {
        if ($schoolBotItem.Attributes -band [IO.FileAttributes]::ReparsePoint) { continue }
        $schoolBotRelative = $schoolBotItem.FullName.Substring($SchoolBotRoot.Length + 1).Replace('\','/')
        if ($schoolBotItem.PSIsContainer) {
            if ($schoolBotItem.Name -in $schoolBotExcludedDirectories) { continue }
            if ($schoolBotRelative -in @('app/frontend/public/blockly-media','web/data')) { continue }
            $schoolBotPending.Push($schoolBotItem.FullName)
        } elseif (
            $schoolBotItem.Name -notlike '.env*' -and
            $schoolBotItem.Name -notlike 'appsettings.*.json' -and
            $schoolBotItem.Extension -notin $schoolBotExcludedExtensions
        ) {
            $schoolBotFiles.Add($schoolBotItem)
        }
    }
}
foreach ($schoolBotName in @(
    'README.md','AGENTS.md','CLAUDE.md','THIRD-PARTY-NOTICES.md',
    '.gitignore','.gitattributes','go.work','go.work.sum'
)) {
    $schoolBotFile = Join-Path $SchoolBotRoot $schoolBotName
    if (Test-Path -LiteralPath $schoolBotFile -PathType Leaf) {
        $schoolBotItem = Get-Item -LiteralPath $schoolBotFile -Force
        if (-not ($schoolBotItem.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            $schoolBotFiles.Add($schoolBotItem)
        }
    }
}
$schoolBotStream = [IO.File]::Open($schoolBotOutput, [IO.FileMode]::Create)
$schoolBotZip = [IO.Compression.ZipArchive]::new($schoolBotStream, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($schoolBotFile in $schoolBotFiles | Sort-Object FullName) {
        $schoolBotRelative = $schoolBotFile.FullName.Substring($SchoolBotRoot.Length + 1).Replace('\','/')
        $schoolBotEntry = $schoolBotZip.CreateEntry($schoolBotRelative, [IO.Compression.CompressionLevel]::Optimal)
        $schoolBotEntryStream = $schoolBotEntry.Open()
        try {
            $schoolBotInputStream = [IO.File]::OpenRead($schoolBotFile.FullName)
            try { $schoolBotInputStream.CopyTo($schoolBotEntryStream) }
            finally { $schoolBotInputStream.Dispose() }
        } finally { $schoolBotEntryStream.Dispose() }
    }
} finally { $schoolBotZip.Dispose(); $schoolBotStream.Dispose() }
Write-Host "Source package: $schoolBotOutput ($($schoolBotFiles.Count) files; known secrets/artifacts excluded)."
