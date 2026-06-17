$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Planning\Tracking.cshtml"
$lines = Get-Content -Path $path -Encoding UTF8
$newFunctions = Get-Content -Path "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\new_functions.txt" -Encoding UTF8

$pre = $lines[0..6]
$post = $lines[161..($lines.Count - 1)]

$newLines = @()
$newLines += $pre
$newLines += $newFunctions
$newLines += $post

[IO.File]::WriteAllLines($path, $newLines, [System.Text.Encoding]::UTF8)
