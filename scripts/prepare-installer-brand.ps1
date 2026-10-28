param(
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateSet('th','en','zh')][string]$Language = 'th'
)
. "$PSScriptRoot/common.ps1"
Add-Type -AssemblyName System.Drawing
$brandDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
New-Item -ItemType Directory -Path $brandDirectory -Force | Out-Null
$locale = switch ($Language) {
    'th' { @{ Culture = 'th-TH'; Codepage = '874'; LanguageId = '1054' } }
    'en' { @{ Culture = 'en-US'; Codepage = '1252'; LanguageId = '1033' } }
    'zh' { @{ Culture = 'zh-CN'; Codepage = '936'; LanguageId = '2052' } }
}
$catalogPath = Join-Path $SchoolBotRoot "app/frontend/src/i18n/locales/$Language.json"
$catalog = Get-Content -LiteralPath $catalogPath -Raw | ConvertFrom-Json
[IO.File]::WriteAllText((Join-Path $brandDirectory 'strings.json'),
    ($catalog.installer | ConvertTo-Json -Depth 5), [Text.UTF8Encoding]::new($false))
$xmlSettings = [Xml.XmlWriterSettings]::new()
$xmlSettings.Indent = $true
$xmlSettings.Encoding = [Text.UTF8Encoding]::new($false)
$writer = [Xml.XmlWriter]::Create((Join-Path $brandDirectory 'installer.wxl'), $xmlSettings)
try {
    $writer.WriteStartElement('WixLocalization', 'http://wixtoolset.org/schemas/v4/wxl')
    $writer.WriteAttributeString('Culture', $locale.Culture)
    $writer.WriteAttributeString('Codepage', $locale.Codepage)
    foreach ($property in $catalog.installer.PSObject.Properties) {
        $writer.WriteStartElement('String')
        $writer.WriteAttributeString('Id', "Installer_$($property.Name)")
        $writer.WriteAttributeString('Value', [string]$property.Value)
        $writer.WriteEndElement()
    }
    $writer.WriteStartElement('String')
    $writer.WriteAttributeString('Id', 'Installer_languageId')
    $writer.WriteAttributeString('Value', $locale.LanguageId)
    $writer.WriteEndElement()
    $writer.WriteEndElement()
} finally { $writer.Dispose() }

$logoSvg = Get-Content -LiteralPath (Join-Path $SchoolBotRoot 'app/frontend/src/assets/logo-school-bot.svg') -Raw
$embedded = [regex]::Match($logoSvg, 'data:image/png;base64,([A-Za-z0-9+/=\r\n]+)')
if (-not $embedded.Success) { throw 'School Bot embedded logo PNG not found.' }
$logoStream = [IO.MemoryStream]::new([Convert]::FromBase64String($embedded.Groups[1].Value))
$logo = [Drawing.Image]::FromStream($logoStream)
$hero = [Drawing.Image]::FromFile((Join-Path $SchoolBotRoot 'app/build/windows/installer/hero-robot.png'))
$logoCrop = [Drawing.Rectangle]::new(0, 680, 4096, 680)
if ($logo.Width -ne 4096 -or $logo.Height -ne 2028) {
    throw 'Review the installer wordmark crop: original logo dimensions changed.'
}
function New-BrandCanvas([int]$Width, [int]$Height) {
    $bitmap = [Drawing.Bitmap]::new($Width, $Height, [Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([Drawing.Color]::White)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    return @{ Bitmap = $bitmap; Graphics = $graphics }
}
function Write-BrandCanvas($Canvas, [string]$Name) {
    try { $Canvas.Bitmap.Save((Join-Path $brandDirectory $Name), [Drawing.Imaging.ImageFormat]::Bmp) }
    finally { $Canvas.Graphics.Dispose(); $Canvas.Bitmap.Dispose() }
}
try {
    $wordmark = [Drawing.Bitmap]::new(1024, 170)
    $wordmarkGraphics = [Drawing.Graphics]::FromImage($wordmark)
    try {
        $wordmarkGraphics.Clear([Drawing.Color]::Transparent)
        $wordmarkGraphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $wordmarkGraphics.DrawImage($logo, [Drawing.Rectangle]::new(0, 0, 1024, 170),
            $logoCrop, [Drawing.GraphicsUnit]::Pixel)
        $wordmark.Save((Join-Path $brandDirectory 'wordmark.png'), [Drawing.Imaging.ImageFormat]::Png)
    } finally { $wordmarkGraphics.Dispose(); $wordmark.Dispose() }
    $scene = New-BrandCanvas 986 720
    $blueTint = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#E4F2FC'))
    $orangeTint = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#FFEDE0'))
    $blue = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#9CCDF0'))
    $orange = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#FF7A62'))
    try {
        $scene.Graphics.FillRectangle($blueTint, 0, 0, 347, 720)
        $scene.Graphics.FillEllipse([Drawing.Brushes]::White, -85, 145, 520, 520)
        $scene.Graphics.FillEllipse($orangeTint, -160, 525, 430, 430)
        $scene.Graphics.FillEllipse($blue, 277, 110, 22, 22)
        $scene.Graphics.FillEllipse($orange, 48, 550, 16, 16)
        $scene.Graphics.FillEllipse($orange, 280, 602, 10, 10)
        $scene.Graphics.DrawImage($hero, [Drawing.Rectangle]::new(45, 180, 267, 382))
        $scene.Graphics.DrawImage($logo, [Drawing.Rectangle]::new(400, 64, 540, 90), $logoCrop, [Drawing.GraphicsUnit]::Pixel)
        Write-BrandCanvas $scene 'scene.bmp'

        $dialog = New-BrandCanvas 986 624
        $dialog.Graphics.FillRectangle($blueTint, 0, 0, 347, 624)
        $dialog.Graphics.FillEllipse([Drawing.Brushes]::White, -70, 130, 470, 470)
        $dialog.Graphics.DrawImage($hero, [Drawing.Rectangle]::new(45, 160, 267, 382))
        Write-BrandCanvas $dialog 'dialog.bmp'

        $banner = New-BrandCanvas 986 116
        $banner.Graphics.FillRectangle($blueTint, 0, 110, 986, 6)
        $banner.Graphics.DrawImage($logo, [Drawing.Rectangle]::new(730, 28, 232, 39), $logoCrop, [Drawing.GraphicsUnit]::Pixel)
        Write-BrandCanvas $banner 'banner.bmp'
    } finally { $blueTint.Dispose(); $orangeTint.Dispose(); $blue.Dispose(); $orange.Dispose() }
} finally { $hero.Dispose(); $logo.Dispose(); $logoStream.Dispose() }
Write-Host "School Bot installer branding and $($locale.Culture) strings ready: $brandDirectory"
