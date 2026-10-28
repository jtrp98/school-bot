# Reuse the embedded original PNG to preserve the mascot artwork.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path $PSScriptRoot -Parent
$iconRoot = Join-Path $projectRoot 'app/build/windows'
$svg = [IO.File]::ReadAllText((Join-Path $projectRoot 'app/frontend/src/assets/logo-school-bot.svg'))
$embedded = [regex]::Match($svg, 'data:image/png;base64,([A-Za-z0-9+/=\r\n]+)')
if (-not $embedded.Success) { throw 'School Bot original logo PNG not found.' }
$logoStream = [IO.MemoryStream]::new([Convert]::FromBase64String($embedded.Groups[1].Value))
$logo = [Drawing.Image]::FromStream($logoStream)
if ($logo.Width -ne 4096 -or $logo.Height -ne 2028) { throw 'Logo geometry changed: review the mascot crop before regenerating.' }
# Crop the wordmark away so desktop icons contain only the mascot.
$mascot = [Drawing.Rectangle]::new(48, 680, 680, 680)
$bitmap = [Drawing.Bitmap]::new(1024, 1024, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [Drawing.Graphics]::FromImage($bitmap)
try {
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.DrawImage($logo, [Drawing.Rectangle]::new(0,0,1024,1024), $mascot, [Drawing.GraphicsUnit]::Pixel)
    $images = @()
    foreach ($size in @(16,20,24,32,48,64,128,256)) {
        $scaled = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $canvas = [Drawing.Graphics]::FromImage($scaled)
        $stream = [IO.MemoryStream]::new()
        try {
            $canvas.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $canvas.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $canvas.DrawImage($bitmap, [Drawing.Rectangle]::new(0,0,$size,$size))
            $scaled.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
            $images += [pscustomobject]@{ Size=$size; Bytes=$stream.ToArray() }
            if ($size -eq 256) {
                [IO.File]::WriteAllBytes((Join-Path $iconRoot 'appicon.png'), $stream.ToArray())
                [IO.File]::WriteAllBytes((Join-Path $projectRoot 'app/frontend/src/assets/school-bot-icon.png'), $stream.ToArray())
            }
        } finally { $stream.Dispose(); $canvas.Dispose(); $scaled.Dispose() }
    }
    $output = [IO.File]::Create((Join-Path $iconRoot 'appicon.ico'))
    $writer = [IO.BinaryWriter]::new($output)
    try {
        $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$images.Count)
        $offset = 6 + 16 * $images.Count
        foreach ($entry in $images) {
            $dimension = if ($entry.Size -eq 256) { 0 } else { $entry.Size }
            $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
            $writer.Write([byte]0); $writer.Write([byte]0)
            $writer.Write([uint16]1); $writer.Write([uint16]32)
            $writer.Write([uint32]$entry.Bytes.Length); $writer.Write([uint32]$offset)
            $offset += $entry.Bytes.Length
        }
        foreach ($entry in $images) { $writer.Write([byte[]]$entry.Bytes) }
    } finally { $writer.Dispose(); $output.Dispose() }
} finally {
    $graphics.Dispose(); $bitmap.Dispose(); $logo.Dispose(); $logoStream.Dispose()
}
Write-Host 'Generated School Bot robot icon: ICO (8 sizes), native PNG and frontend PNG.'
