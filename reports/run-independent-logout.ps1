$ErrorActionPreference = 'Stop'
$root = (Get-Location).Path
$cache = 'C:/Users/jabja/.nuget/packages'
$bundlePath = 'C:/src/schoolbright/reports/school-bot/normal-089/grpc-independent-bundle.txt'

if (![IO.File]::Exists($bundlePath)) {
    throw "Bundle not found: $bundlePath"
}

# Ensure solution is built first so binaries exist
dotnet build web/SchoolBot.slnx --no-restore -v quiet

# Gather reference assemblies for compilation
$refPaths = [System.Collections.Generic.List[string]]::new()

# .NET and ASP.NET Core Ref packs
$netPack = 'C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/10.0.0/ref/net10.0'
$aspPack = 'C:/Program Files/dotnet/packs/Microsoft.AspNetCore.App.Ref/10.0.0/ref/net10.0'
foreach ($f in (Get-ChildItem -LiteralPath $netPack -Filter '*.dll')) { $refPaths.Add($f.FullName) }
foreach ($f in (Get-ChildItem -LiteralPath $aspPack -Filter '*.dll')) { $refPaths.Add($f.FullName) }

# Package dependencies
$deps = @(
    "$cache/grpc.core.api/2.76.0/lib/netstandard2.1/Grpc.Core.Api.dll",
    "$cache/grpc.net.common/2.76.0/lib/net10.0/Grpc.Net.Common.dll",
    "$cache/grpc.net.client/2.76.0/lib/net10.0/Grpc.Net.Client.dll",
    "$cache/grpc.aspnetcore.server/2.76.0/lib/net10.0/Grpc.AspNetCore.Server.dll",
    "$cache/google.protobuf/3.34.0/lib/net5.0/Google.Protobuf.dll",
    "$cache/microsoft.aspnetcore.testhost/10.0.0/lib/net10.0/Microsoft.AspNetCore.TestHost.dll"
)
foreach ($d in $deps) { $refPaths.Add($d) }

# Project assemblies (from target repo)
$projects = @(
    "$root/web/src/SchoolBot.Domain/bin/Debug/net10.0/SchoolBot.Domain.dll",
    "$root/web/src/SchoolBot.Application/bin/Debug/net10.0/SchoolBot.Application.dll",
    "$root/web/src/SchoolBot.Infrastructure/bin/Debug/net10.0/SchoolBot.Infrastructure.dll",
    "$root/web/src/SchoolBot.Api/bin/Debug/net10.0/SchoolBot.Api.dll"
)
foreach ($p in $projects) { $refPaths.Add($p) }

$refs = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($p in $refPaths) {
    $bytes = [IO.File]::ReadAllBytes($p)
    $refs.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromImage($bytes))
}

# Read bundle code
$bundleCode = [IO.File]::ReadAllText($bundlePath)

# Add runner method to invoke the 150 test suite assertions
$runnerCode = @'
public static class IndependentLogoutCheckRunner
{
    public static async System.Threading.Tasks.Task<int> RunAll()
    {
        await IndependentLogoutCheckV7.Direct();
        await LogoutWireCheckV7.Run();
        await LogoutExtraCheckV7.Run();

        var messages = CheckLogRecorderV7.Messages.ToArray();
        IndependentLogoutCheckV7.Assert("wire logs captured",
            messages.Length > 0, "expected=>0 actual=" + messages.Length);
        IndependentLogoutCheckV7.Assert("wire logs redact token",
            !string.Join("\n", messages).Contains(IndependentLogoutCheckV7.Token), "expected=no raw token");
        IndependentLogoutCheckV7.Assert("wire logs redact provider secret",
            !string.Join("\n", messages).Contains("SECRET:"), "expected=no SECRET provider detail");

        return IndependentLogoutCheckV7.Result();
    }
}
'@

$parseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.
    WithLanguageVersion([Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Preview)

$trees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($bundleCode, $parseOptions, 'grpc-independent-bundle.cs'))
$trees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($runnerCode, $parseOptions, 'runner.cs'))

$compOptions = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new(
    [Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary)

$compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
    'IndependentLogoutAssembly', $trees, $refs, $compOptions)

$stream = [IO.MemoryStream]::new()
$emitResult = $compilation.Emit($stream)

if (!$emitResult.Success) {
    foreach ($diag in ($emitResult.Diagnostics | Where-Object Severity -eq Error)) {
        Write-Error $diag.ToString()
    }
    exit 2
}

# Pre-load runtime assemblies in memory by reading bytes without locking files
$knownAssemblies = [System.Collections.Concurrent.ConcurrentDictionary[string, System.Reflection.Assembly]]::new([System.StringComparer]::OrdinalIgnoreCase)

[System.Runtime.Loader.AssemblyLoadContext]::Default.add_Resolving({
    param($ctx, $name)
    $found = $null
    if ($knownAssemblies.TryGetValue($name.Name, [ref]$found)) {
        return $found
    }
    foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
        if ([string]::Equals($asm.GetName().Name, $name.Name, [System.StringComparison]::OrdinalIgnoreCase)) {
            $knownAssemblies.TryAdd($name.Name, $asm) | Out-Null
            return $asm
        }
    }
    return $null
})

$aspRuntime = 'C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App/10.0.0'
foreach ($f in (Get-ChildItem -LiteralPath $aspRuntime -Filter '*.dll')) {
    try {
        $bytes = [IO.File]::ReadAllBytes($f.FullName)
        $asm = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromStream([IO.MemoryStream]::new($bytes))
        $knownAssemblies.TryAdd($asm.GetName().Name, $asm) | Out-Null
    } catch {}
}
foreach ($d in $deps) {
    try {
        $bytes = [IO.File]::ReadAllBytes($d)
        $asm = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromStream([IO.MemoryStream]::new($bytes))
        $knownAssemblies.TryAdd($asm.GetName().Name, $asm) | Out-Null
    } catch {}
}
foreach ($p in $projects) {
    try {
        $bytes = [IO.File]::ReadAllBytes($p)
        $asm = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromStream([IO.MemoryStream]::new($bytes))
        $knownAssemblies.TryAdd($asm.GetName().Name, $asm) | Out-Null
    } catch {}
}

$testBytes = $stream.ToArray()
$testAssembly = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromStream([IO.MemoryStream]::new($testBytes))
$runnerType = $testAssembly.GetType('IndependentLogoutCheckRunner')
$task = $runnerType.GetMethod('RunAll').Invoke($null, @())
$exitCode = $task.GetAwaiter().GetResult()
exit $exitCode
