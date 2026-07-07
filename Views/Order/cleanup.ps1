$filePath = 'c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml'
$text = [System.IO.File]::ReadAllText($filePath)
$regex = [regex]'(?s)<!-- Edit Model Detail Modal -->.*?<div class="premium-header".*?</div>(.*?)<!-- Edit Model Detail Modal -->'
$newText = $regex.Replace($text, '<!-- Edit Model Detail Modal -->', 1)
[System.IO.File]::WriteAllText($filePath, $newText, [System.Text.Encoding]::UTF8)
Write-Host "Replaced!"
