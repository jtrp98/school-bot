param(
    [Parameter(Mandatory)][string]$InstallerPath,
    [Parameter(Mandatory)][string]$BrandDirectory,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [switch]$Preview
)
. "$PSScriptRoot/common.ps1"
$project = Join-Path $SchoolBotRoot 'app/installer'
$version = (Get-Content -LiteralPath "$SchoolBotRoot/app/frontend/package.json" -Raw | ConvertFrom-Json).version
$package = (Resolve-Path -LiteralPath $InstallerPath).Path
$brand = (Resolve-Path -LiteralPath $BrandDirectory).Path
$output = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$packages = Join-Path $SchoolBotRoot '.tools/nuget-installer'
$hash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $brand 'payload.json'),
    (@{ Version = $version; Sha256 = $hash } | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$notices = Join-Path $brand 'runtime-notices.txt'
$arguments = @(
    "-p:InstallerPackage=$package", "-p:BrandDirectory=$brand", "-p:Version=$version",
    "-p:RuntimeNotices=$notices", "-p:RestorePackagesPath=$packages"
)
Push-Location $project
try {
    Invoke-SchoolBotCommand 'dotnet' (@('restore','--disable-build-servers') + $arguments)
    $projectProperties = ([xml](Get-Content -LiteralPath "$project/SchoolBot.Installer.csproj")).Project.PropertyGroup
    $runtimeVersion = $projectProperties.RuntimeFrameworkVersion
    $noticeParts = @((Get-Content -LiteralPath "$SchoolBotRoot/THIRD-PARTY-NOTICES.md" -Raw),
        (Get-Content -LiteralPath "$project/Assets/Fonts/OFL.txt" -Raw))
    foreach ($component in @('microsoft.netcore.app.runtime.win-x64','microsoft.windowsdesktop.app.runtime.win-x64')) {
        $names = if ($component -eq 'microsoft.netcore.app.runtime.win-x64') {
            @('LICENSE.TXT','THIRD-PARTY-NOTICES.TXT')
        } else { @('LICENSE') }
        foreach ($name in $names) {
            $noticePath = Join-Path $packages "$component/$runtimeVersion/$name"
            if (-not (Test-Path -LiteralPath $noticePath)) { throw "Missing redistributed runtime notice: $noticePath" }
            $noticeParts += "`n$component $runtimeVersion / $name`n"
            $noticeParts += Get-Content -LiteralPath $noticePath -Raw
        }
    }
    foreach ($component in @('wpf','winforms')) {
        $noticeParts += Get-Content -LiteralPath "$project/Licenses/$component-NOTICES.txt" -Raw
    }
    [IO.File]::WriteAllText($notices, ($noticeParts -join "`n"), [Text.UTF8Encoding]::new($false))
    if ($Preview) {
        Invoke-SchoolBotCommand 'dotnet' (@('build','-c','Release','--no-restore','--disable-build-servers',
            '-p:SelfContained=false','-p:PublishSingleFile=false','-p:DefineConstants=SETUP_PREVIEW',
            '-o',$output) + $arguments)
        $process = Start-Process -FilePath (Join-Path $output 'SchoolBotSetup.exe') -WindowStyle Hidden -Wait -PassThru `
            -ArgumentList @('--verify-and-render', ('"' + (Join-Path $SchoolBotRoot 'dist/installer-preview') + '"'))
        if ($process.ExitCode -ne 0) { throw "Installer preview checks failed: $($process.ExitCode)" }
        Write-Host (Get-Content -LiteralPath "$SchoolBotRoot/dist/installer-preview/checks.txt")
    } else {
        Invoke-SchoolBotCommand 'dotnet' (@('publish','-c','Release','--no-restore','--disable-build-servers',
            '-o',$output) + $arguments)
        $unexpected = @(Get-ChildItem -LiteralPath $output -File | Where-Object Name -ne 'SchoolBotSetup.exe')
        if ($unexpected.Count -gt 0) { throw "Unexpected files in single-file installer output: $($unexpected.Name -join ', ')" }
        $exe = Join-Path $output 'SchoolBotSetup.exe'
        if ((Get-Item -LiteralPath $exe).VersionInfo.ProductVersion -ne $version) { throw 'Installer version does not match package.json.' }
        $assembly = Join-Path $project "bin/Release/$($projectProperties.TargetFramework)/win-x64/SchoolBotSetup.dll"
        $assemblyBytes = [IO.File]::ReadAllBytes($assembly)
        if ([Text.Encoding]::UTF8.GetString($assemblyBytes).Contains('SetupPreview') -or
            [Text.Encoding]::Unicode.GetString($assemblyBytes).Contains('--verify-and-render')) {
            throw 'Preview tools must not be included in the release installer.'
        }
        Write-Host "Self-contained School Bot installer ready: $exe"
    }
} finally { Pop-Location }
