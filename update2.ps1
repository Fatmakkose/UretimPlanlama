$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml"
$content = Get-Content -Path $path -Raw -Encoding UTF8

$content = $content -replace '(?s)\s*<button style="background: transparent; border: none; color: #0284c7; font-weight: 600; display: flex; align-items: center; gap: 8px; cursor: pointer; padding: 8px; font-size: 0.9rem;">\s*<i class="fa-solid fa-filter"></i> Gelişmiş Filtre\s*</button>', ''

$content = $content -replace '<th>MODEL / DETAY</th>', "<th>TARİH</th>`r`n                        <th>MODEL / DETAY</th>"
$content = $content -replace '\s*<th style="text-align: center;">KUMAŞ DURUMU</th>', ''
$content = $content -replace '\s*<th style="text-align: center;">SİPARİŞ DURUMU</th>', ''

[IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
