param([int]$Runs = 5, [string]$Clear, [string]$Send, [string]$Opened, [string]$Done)
# Sends to the board while reading, waits for reading to resume, and reports the lines around
# the resume point. Thai labels come in as parameters because PowerShell 5.1 reads this file as ANSI.
$s = Join-Path $PSScriptRoot "ui.ps1"
foreach ($run in 1..$Runs) {
  try { & $s -Action invoke -Target $Done | Out-Null } catch {}
  & $s -Action invoke -Target $Clear | Out-Null
  Start-Sleep -Milliseconds 400
  & $s -Action invoke -Target $Send | Out-Null
  $t0 = Get-Date
  do { Start-Sleep -Milliseconds 300; $d = & $s -Action dump -All } until (($d -match $Opened) -or ((Get-Date) - $t0).TotalSeconds -gt 120)
  Start-Sleep -Milliseconds 2200
  $rows = @(& $s -Action dump -All | Where-Object { $_ -match '^Text' } | ForEach-Object { $_ -replace '^Text\s+', '' } |
    Where-Object { $_ -notmatch '^(\d+:\d+|\S+:)$' })
  $i = [array]::FindIndex($rows, [Predicate[string]]{ param($x) $x -match $Opened })
  $bad = @($rows | Where-Object { $_.Contains([char]0xFFFD) }).Count
  "run {0}: before='{1}' after='{2}' | '{3}' | FFFD rows={4}" -f $run, $rows[$i - 1], $rows[$i + 1], $rows[$i + 2], $bad
}
