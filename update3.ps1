$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml"
$content = Get-Content -Path $path -Raw -Encoding UTF8

$regex = '(?s)\s*\$\("#orderSearchInput"\)\.on\("keyup", function\(\) \{\s*var value = \$\(this\)\.val\(\)\.toLowerCase\(\);\s*\$\("#orderTableBody tr"\)\.filter\(function\(\) \{\s*\$\(this\)\.toggle\(\$\(this\)\.text\(\)\.toLowerCase\(\)\.indexOf\(value\) > -1\)\s*\}\);\s*\}\);'
$content = $content -replace $regex, ''

[IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
