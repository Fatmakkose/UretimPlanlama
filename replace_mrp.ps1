$filePath = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Planning\Plan.cshtml"
$templatePath = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\mrp_template.txt"

$lines = Get-Content $filePath -Encoding UTF8
$templateLines = Get-Content $templatePath -Encoding UTF8

$startIndex = -1
$endIndex = -1

for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match "var purData = new System.Collections.Generic.Dictionary<string, string>\(\);") {
        # The line before it is @{
        if ($lines[$i-1] -match "@{") {
            $startIndex = $i - 1
            break
        }
    }
}

for ($i = $startIndex; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match "<div style=`"display: flex; justify-content: space-between; margin-top: 20px; align-items: center;`">") {
        $endIndex = $i
        break
    }
}

if ($startIndex -ne -1 -and $endIndex -ne -1) {
    $newLines = @()
    for ($i = 0; $i -lt $startIndex; $i++) {
        $newLines += $lines[$i]
    }
    foreach ($tLine in $templateLines) {
        $newLines += $tLine
    }
    for ($i = $endIndex; $i -lt $lines.Length; $i++) {
        $newLines += $lines[$i]
    }
    
    $newLines | Set-Content $filePath -Encoding UTF8
    Write-Host "Replacement successful."
} else {
    Write-Host "Could not find start or end index. Start: $startIndex, End: $endIndex"
}
