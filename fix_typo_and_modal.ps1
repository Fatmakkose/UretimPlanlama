$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Planning\Tracking.cshtml"
$lines = Get-Content -Path $path -Encoding UTF8
$newLines = @()
$skip = $false

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match "<!-- Model Detay & Sarfiyat Hesaplama Kartı -->") {
        $skip = $true
    }
    if ($skip -and $lines[$i] -match "<!-- Planlama & Takip Süreçleri Kartı -->") {
        $skip = $false
    }
    
    if (-not $skip) {
        $newLine = $lines[$i]
        if ($newLine -match "LERLEME") {
            $newLine = $newLine -replace "<span>.*?LERLEME</span>", "<span>İLERLEME</span>"
        }
        $newLines += $newLine
    }
}

[IO.File]::WriteAllLines($path, $newLines, [System.Text.Encoding]::UTF8)
