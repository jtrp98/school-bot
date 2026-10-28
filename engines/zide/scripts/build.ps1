param([string]$OutputDirectory, [switch]$Test)
$ErrorActionPreference = 'Stop'
$zideSource = Split-Path $PSScriptRoot -Parent
$zideRoot = [IO.Path]::GetFullPath((Join-Path $zideSource '../..'))
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $zideSource 'bin/windows-amd64' }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$zideDestination = Join-Path $OutputDirectory 'zide-engine.exe'
if (Test-Path -LiteralPath $zideDestination -PathType Leaf) {
    try {
        $zideProbe = [IO.File]::Open($zideDestination, 'Open', 'ReadWrite', 'None')
        $zideProbe.Dispose()
    } catch {
        throw "Engine output is in use or not writable: $zideDestination. Use -OutputDirectory for a separate build."
    }
}
$zideBuild = Join-Path $zideSource '.build/windows-amd64'
New-Item -ItemType Directory -Force -Path $zideBuild | Out-Null
function Invoke-ZideCompiler([string]$Executable, [string[]]$CompilerArguments) {
    & $Executable @CompilerArguments
    if ($LASTEXITCODE -ne 0) { throw "Zide C++ build failed (exit $LASTEXITCODE)." }
}
$zidePortable = Join-Path $zideRoot '.tools/zig-x86_64-windows-0.15.2/zig.exe'
$zideZig = if (Test-Path -LiteralPath $zidePortable) {
    $zidePortable
} else {
    (Get-Command zig -ErrorAction SilentlyContinue).Source
}
if ($zideZig) {
    $env:ZIG_GLOBAL_CACHE_DIR = Join-Path $zideRoot '.tools/zig-cache'
    $zideCommon = @(
        '-target','x86_64-windows-gnu','-mcpu=baseline',
        '-O2','-DNDEBUG','-DHIDAPI_STATIC','-DNOMINMAX','-DWIN32_LEAN_AND_MEAN',
        "-ffile-prefix-map=$zideRoot=.","-I$zideSource/include",
        "-I$zideSource/third_party","-I$zideSource/third_party/hidapi"
    )
    $zideHid = Join-Path $zideBuild 'hid.o'
    Invoke-ZideCompiler $zideZig (@('cc') + $zideCommon + @(
        '-c',"$zideSource/third_party/hidapi/windows/hid.c",'-o',$zideHid
    ))
    $zideProtocol = Join-Path $zideBuild 'protocol.o'
    Invoke-ZideCompiler $zideZig (@('c++','-std=c++17') + $zideCommon + @(
        '-c',"$zideSource/src/protocol.cpp",'-o',$zideProtocol
    ))
    $zideObjects = @($zideProtocol,$zideHid)
    foreach ($zideUnit in @(
        'transport','transport_hid','transport_serial','discovery','actions','request','line_reader'
    )) {
        $zideObject = Join-Path $zideBuild "$zideUnit.o"
        Invoke-ZideCompiler $zideZig (@('c++','-std=c++17') + $zideCommon + @(
            '-c',"$zideSource/src/$zideUnit.cpp",'-o',$zideObject
        ))
        $zideObjects += $zideObject
    }
    $zideObjects += @('-lsetupapi','-ladvapi32')
    Invoke-ZideCompiler $zideZig (@('c++','-std=c++17','-s') + $zideCommon + @(
        "$zideSource/src/main.cpp"
    ) + $zideObjects + @('-o',"$OutputDirectory/zide-engine.exe"))
    if ($Test) {
        Invoke-ZideCompiler $zideZig (@('c++','-std=c++17') + $zideCommon + @(
            "$zideSource/tests/protocol_test.cpp",$zideProtocol,'-o',"$zideBuild/zide-protocol-tests.exe"
        ))
        & "$zideBuild/zide-protocol-tests.exe"
        if ($LASTEXITCODE -ne 0) { throw 'Zide protocol tests failed.' }
    }
} else {
    $zideCmake = Get-Command cmake -ErrorAction SilentlyContinue
    if (-not $zideCmake) {
        throw ('Install CMake + a C++17 toolchain with Windows SDK (Visual Studio C++ workload), ' +
            'or Zig 0.15.2. See engines/zide/docs/API.md.')
    }
    Invoke-ZideCompiler $zideCmake.Source @('-S',$zideSource,'-B',$zideBuild,'-A','x64')
    Invoke-ZideCompiler $zideCmake.Source @('--build',$zideBuild,'--config','Release')
    Copy-Item -LiteralPath "$zideBuild/Release/zide-engine.exe" -Destination "$OutputDirectory/zide-engine.exe" -Force
    if ($Test) {
        & "$zideBuild/Release/zide-protocol-tests.exe"
        if ($LASTEXITCODE -ne 0) { throw 'Zide protocol tests failed.' }
    }
}
Copy-Item -LiteralPath "$zideSource/THIRD-PARTY-NOTICES.md" `
    -Destination "$OutputDirectory/THIRD-PARTY-NOTICES.md" -Force
Copy-Item -LiteralPath "$zideSource/third_party/hidapi/LICENSE-bsd.txt" `
    -Destination "$OutputDirectory/HIDAPI-LICENSE.txt" -Force
Copy-Item -LiteralPath "$zideSource/third_party/nlohmann/LICENSE.MIT" `
    -Destination "$OutputDirectory/JSON-LICENSE.txt" -Force
Copy-Item -LiteralPath "$zideSource/engine.json" -Destination "$OutputDirectory/engine.json" -Force
$zideHash = (Get-FileHash -LiteralPath "$OutputDirectory/zide-engine.exe" -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$OutputDirectory/zide-engine.exe.sha256", "$zideHash  zide-engine.exe`n")
Write-Host "C++ engine ready: $OutputDirectory/zide-engine.exe"
