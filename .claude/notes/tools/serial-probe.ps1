param([string]$Port = "COM3", [int]$Runs = 6, [int]$Ms = 3000)
# Opens the port the way the engine does (DTR on, so the UNO resets) and prints when each byte
# arrived, to see what comes before the program's first line.
foreach ($run in 1..$Runs) {
  $sp = New-Object System.IO.Ports.SerialPort $Port, 9600, ([System.IO.Ports.Parity]::None), 8, ([System.IO.Ports.StopBits]::One)
  $sp.DtrEnable = $true
  $sp.ReadTimeout = 50
  $events = New-Object System.Collections.Generic.List[string]
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $sp.Open()
  $buf = New-Object byte[] 256
  while ($sw.ElapsedMilliseconds -lt $Ms) {
    try {
      $n = $sp.Read($buf, 0, $buf.Length)
      if ($n -gt 0) {
        $hex = ($buf[0..($n - 1)] | ForEach-Object { $_.ToString("X2") }) -join " "
        $events.Add(("{0,5}ms {1}" -f $sw.ElapsedMilliseconds, $hex))
      }
    } catch [System.TimeoutException] { }
  }
  $sp.Close()
  "--- run $run"
  $events | Select-Object -First 4
  Start-Sleep -Milliseconds 700
}
