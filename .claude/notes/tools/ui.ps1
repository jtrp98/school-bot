param(
  [Parameter(Mandatory)][ValidateSet('shot','dump','invoke','click','type','key','xy','freeze')][string]$Action,
  [string]$Target,
  [string]$Text,
  [int]$Index = 0,
  [string]$Out = "$PSScriptRoot\shot.png",
  [string]$Filter,
  [switch]$All
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing, System.Windows.Forms
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class W {
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
  [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, IntPtr e);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
}
"@
[void][W]::SetProcessDPIAware()
$A = [System.Windows.Automation.AutomationElement]
$proc = Get-Process -Name school-bot | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'school-bot window not found' }
$win = $A::FromHandle($proc.MainWindowHandle)

function All { $win.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) }
function Find([string]$name) {
  $hits = @(All | Where-Object { $_.Current.Name -like $name -and -not $_.Current.IsOffscreen })
  if ($hits.Count -le $Index) { throw "not found: $name (hits $($hits.Count))" }
  $hits[$Index]
}
function ClickAt($el) {
  $r = $el.Current.BoundingRectangle
  [void][W]::SetForegroundWindow($proc.MainWindowHandle)
  [void][W]::SetCursorPos([int]($r.X + $r.Width / 2), [int]($r.Y + $r.Height / 2))
  Start-Sleep -Milliseconds 80
  [W]::mouse_event(2, 0, 0, 0, [IntPtr]::Zero); [W]::mouse_event(4, 0, 0, 0, [IntPtr]::Zero)
}

switch ($Action) {
  'shot' {
    $r = $win.Current.BoundingRectangle
    $bmp = New-Object System.Drawing.Bitmap ([int]$r.Width), ([int]$r.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen([int]$r.X, [int]$r.Y, 0, 0, $bmp.Size)
    $bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
    "saved $Out ($([int]$r.Width)x$([int]$r.Height))"
  }
  'dump' {
    All | Where-Object { $_.Current.Name -and ($All -or -not $_.Current.IsOffscreen) } | ForEach-Object {
      $c = $_.Current
      $line = '{0,-14} {1}' -f ($c.ControlType.ProgrammaticName -replace 'ControlType\.', ''), $c.Name
      if (-not $Filter -or $line -match $Filter) { $line }
    }
  }
  'invoke' {
    $el = Find $Target
    $p = $null
    if ($el.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) { $p.Invoke(); "invoked: $($el.Current.Name)" }
    else { ClickAt $el; "clicked: $($el.Current.Name)" }
  }
  'click' { $el = Find $Target; ClickAt $el; "clicked: $($el.Current.Name)" }
  'type' {
    $el = Find $Target
    $el.SetFocus(); Start-Sleep -Milliseconds 100
    $p = $null
    if ($el.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$p)) { $p.SetValue($Text); "set value" }
    else { [System.Windows.Forms.SendKeys]::SendWait($Text); "sent keys" }
  }
  'key' { [void][W]::SetForegroundWindow($proc.MainWindowHandle); [System.Windows.Forms.SendKeys]::SendWait($Text); "key $Text" }
  'freeze' {
    $follow = Find $Text
    $p = $null
    if ($follow.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$p)) { $p.Invoke() } else { ClickAt $follow }
    $row = @(All | Where-Object { $_.Current.Name -like $Target })[0]
    $sp = $null
    if ($row -and $row.TryGetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$sp)) { $sp.ScrollIntoView() }
    Start-Sleep -Milliseconds 250
    $r = $win.Current.BoundingRectangle
    $bmp = New-Object System.Drawing.Bitmap ([int]$r.Width), ([int]$r.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen([int]$r.X, [int]$r.Y, 0, 0, $bmp.Size)
    $bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
    "froze and saved $Out (row found: $([bool]$row), scroll: $([bool]$sp))"
  }
  'xy' {
    $x, $y = $Text -split ',' | ForEach-Object { [int]$_ }
    [void][W]::SetForegroundWindow($proc.MainWindowHandle)
    [void][W]::SetCursorPos($x, $y); Start-Sleep -Milliseconds 80
    [W]::mouse_event(2, 0, 0, 0, [IntPtr]::Zero); [W]::mouse_event(4, 0, 0, 0, [IntPtr]::Zero)
    "clicked $x,$y"
  }
}
