param([switch]$SkipInstall, [string]$OutputDirectory)
. "$PSScriptRoot/common.ps1"
Assert-SchoolBotGo
if ($IsMacOS -and $OutputDirectory) { throw 'OutputDirectory is currently supported by the Windows verification only.' }
$schoolBotOutput = if ($OutputDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
} else {
    Join-Path $SchoolBotRoot 'app/bin'
}
$schoolBotBuildArguments = @{ SkipInstall = $SkipInstall }
if ($OutputDirectory) { $schoolBotBuildArguments.OutputDirectory = $schoolBotOutput }
& "$PSScriptRoot/build-desktop.ps1" @schoolBotBuildArguments
$schoolBotSuffix = if ($env:OS -eq 'Windows_NT') { '.exe' } else { '' }
$schoolBotEngine = Join-Path $schoolBotOutput "demo-engine$schoolBotSuffix"
$schoolBotZide = Join-Path $schoolBotOutput "engines/zide/zide-engine$schoolBotSuffix"
if ($IsMacOS) {
    $schoolBotArch = & $SchoolBotGo env GOHOSTARCH
    $schoolBotBundle = Join-Path $schoolBotOutput "darwin-$schoolBotArch/School Bot.app/Contents/MacOS"
    $schoolBotEngine = Join-Path $schoolBotBundle 'demo-engine'
    $schoolBotZide = Join-Path $schoolBotBundle 'engines/zide/zide-engine'
}
$schoolBotVersion = (Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/frontend/package.json') -Raw | ConvertFrom-Json).version
Push-Location (Join-Path $SchoolBotRoot 'app/frontend')
try { Invoke-SchoolBotCommand $SchoolBotNpm @('run','test:i18n') } finally { Pop-Location }
if ($IsMacOS) {
    # The bundled engine is already signed, so the unit tests build into a throwaway directory.
    $schoolBotEngineTestOutput = Join-Path ([IO.Path]::GetTempPath()) "school-bot-zide-$([guid]::NewGuid())"
    try {
        Invoke-SchoolBotCommand 'bash' @("$PSScriptRoot/build-engine.sh",'zide',$schoolBotEngineTestOutput,'--test')
    } finally { Remove-Item -LiteralPath $schoolBotEngineTestOutput -Recurse -Force -ErrorAction SilentlyContinue }
} else {
    $schoolBotEngineOutput = Join-Path $schoolBotOutput 'engines/zide'
    & "$PSScriptRoot/build-engine.ps1" -Engine zide -Test -OutputDirectory $schoolBotEngineOutput
}
$schoolBotPreviousZide = $env:ZIDE_ENGINE
$env:ZIDE_ENGINE = $schoolBotZide
try {
    Push-Location $SchoolBotRoot
    try {
        Invoke-SchoolBotCommand 'node' @(
            '--test',
            'app/frontend/tests/frontend-adapter.test.mjs',
            'app/frontend/tests/frontend-workspace.test.mjs',
            'engines/zide/tests/ipc.test.mjs'
        )
    } finally { Pop-Location }
    Push-Location (Join-Path $SchoolBotRoot 'app')
    try {
        # The package under test is not "main", so -X names its import path.
        Invoke-SchoolBotCommand $SchoolBotGo @('test','-ldflags',"-X schoolbot.local/app.appVersion=$schoolBotVersion",'./...')
        Invoke-SchoolBotCommand $SchoolBotGo @('vet','./...')
    } finally { Pop-Location }
} finally { $env:ZIDE_ENGINE = $schoolBotPreviousZide }
$schoolBotInput = @{
    protocol = '1.0'; request_id = 'smoke-1'; mode = 'coding-adventure'; action = 'program.run'
    data = @{ source = "hero.move_right(2)`nhero.jump()" }
} | ConvertTo-Json -Compress
$schoolBotResult = $schoolBotInput | & $schoolBotEngine | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $schoolBotResult.status -ne 'success' -or $schoolBotResult.request_id -ne 'smoke-1') {
    throw 'Native JSON Lines smoke test failed.'
}
$schoolBotBlockInput = @{
    protocol = '1.0'; request_id = 'smoke-block'; mode = 'block-programming'; action = 'program.run'
    data = @{ commands = @(@{ op = 'move_right'; value = 2 }) }
} | ConvertTo-Json -Depth 8 -Compress
$schoolBotResult = $schoolBotBlockInput | & $schoolBotEngine | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $schoolBotResult.status -ne 'success' -or $schoolBotResult.request_id -ne 'smoke-block') {
    throw 'Block engine smoke test failed.'
}
$schoolBotZideInput = @{
    protocol = '1.0'; request_id = 'smoke-zide'; mode = 'robot-control'; action = 'servo.set'
    data = @{ transport = 'dry-run'; id = 1; pwm = 1500; time_ms = 300 }
} | ConvertTo-Json -Compress
$schoolBotResult = $schoolBotZideInput | & $schoolBotZide | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or $schoolBotResult.status -ne 'success' -or $schoolBotResult.data.engine -ne 'zide' -or
    $schoolBotResult.data.delivery -ne 'dry-run') {
    throw 'C++ Zide dry-run smoke test failed.'
}
Push-Location (Join-Path $SchoolBotRoot 'web')
try { Invoke-SchoolBotCommand 'dotnet' @('test','SchoolBot.slnx','--disable-build-servers','-m:1','--nologo','-v','minimal') } finally { Pop-Location }
Write-Host 'Frontend, i18n, native desktop build, Go checks, Zide IPC, JSON Lines and C# API tests passed.'
