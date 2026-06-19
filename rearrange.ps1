$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Home\Index.cshtml"
$content = Get-Content -Path $path -Raw

# 1. Extract the bottom 3 cards
$startMarker = "<div class=`"middle-grid`" style=`"grid-template-columns: 1fr 1fr 1fr; margin-top: 24px;`">"
$endMarker = "<!-- Son Hareketler / Bildirimler -->"

$indexStart = $content.IndexOf($startMarker)
if ($indexStart -ge 0) {
    # Find the end of this div by looking for the script tag
    $scriptStart = $content.IndexOf("<script src=`"https://cdn.jsdelivr.net/npm/chart.js`"></script>")
    if ($scriptStart -ge 0) {
        $extractedPart = $content.Substring($indexStart, $scriptStart - $indexStart).TrimEnd()
        
        # Remove the extracted part from original
        $content = $content.Remove($indexStart, $scriptStart - $indexStart)
        
        # Change margin-top to margin-bottom in extracted part
        $extractedPart = $extractedPart.Replace("margin-top: 24px;", "margin-bottom: 24px;")
        
        # Insert it before the main middle-grid
        $mainGridMarker = "<div class=`"middle-grid`">"
        $insertIndex = $content.IndexOf($mainGridMarker)
        if ($insertIndex -ge 0) {
            $content = $content.Insert($insertIndex, $extractedPart + "`n`n")
        }
    }
}

# 2. Redesign the Workshop items
$workshopRegex = '(?s)<li class="workshop-item" style="padding: 16px 20px; border-bottom: 1px solid #e2e8f0; display: flex; flex-direction: column; gap: 12px;">.*?</li>'

$newWorkshopItem = @"
<li class="workshop-item" style="padding: 16px 20px; border-bottom: 1px solid #e2e8f0;">
    <div style="font-weight: 700; font-size: 1rem; color: #1e293b; display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px;">
        <span><i class="fa-solid fa-industry" style="color: var(--primary-color); margin-right: 6px;"></i> @wStatus.Workshop.Name</span>
        <span style="font-size: 0.75rem; padding: 4px 10px; border-radius: 12px; background: @(wStatus.StatusClass == `"badge-high`" ? `"#fee2e2`" : wStatus.StatusClass == `"badge-medium`" ? `"#fef9c3`" : `"#dcfce7`"); color: @(wStatus.StatusClass == `"badge-high`" ? `"#991b1b`" : wStatus.StatusClass == `"badge-medium`" ? `"#854d0e`" : `"#166534`"); font-weight: 700;">@wStatus.StatusLabel</span>
    </div>
    <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px;">
        <!-- Günlük -->
        <div style="background: #f8fafc; padding: 10px; border-radius: 6px; text-align: center; border: 1px solid #e2e8f0;">
            <div style="font-size: 0.7rem; color: #64748b; font-weight: 600; text-transform: uppercase; margin-bottom: 4px;">Günlük</div>
            <div style="font-size: 1.1rem; font-weight: 800; color: @(wStatus.DailyOccupancyRate >= 90 ? `"#dc2626`" : wStatus.DailyOccupancyRate >= 70 ? `"#eab308`" : `"#10b981`");">@wStatus.DailyOccupancyRate%</div>
            <div style="font-size: 0.7rem; color: #94a3b8; margin-top: 2px;">@wStatus.DailyUsage.ToString("N0") Adet</div>
        </div>
        <!-- Aylık -->
        <div style="background: #f8fafc; padding: 10px; border-radius: 6px; text-align: center; border: 1px solid #e2e8f0;">
            <div style="font-size: 0.7rem; color: #64748b; font-weight: 600; text-transform: uppercase; margin-bottom: 4px;">Aylık</div>
            <div style="font-size: 1.1rem; font-weight: 800; color: @(wStatus.MonthlyOccupancyRate >= 90 ? `"#dc2626`" : wStatus.MonthlyOccupancyRate >= 70 ? `"#eab308`" : `"#10b981`");">@wStatus.MonthlyOccupancyRate%</div>
            <div style="font-size: 0.7rem; color: #94a3b8; margin-top: 2px;">@wStatus.MonthlyUsage.ToString("N0") Adet</div>
        </div>
        <!-- Yıllık -->
        <div style="background: #f8fafc; padding: 10px; border-radius: 6px; text-align: center; border: 1px solid #e2e8f0;">
            <div style="font-size: 0.7rem; color: #64748b; font-weight: 600; text-transform: uppercase; margin-bottom: 4px;">Yıllık</div>
            <div style="font-size: 1.1rem; font-weight: 800; color: @(wStatus.AnnualOccupancyRate >= 90 ? `"#dc2626`" : wStatus.AnnualOccupancyRate >= 70 ? `"#eab308`" : `"#10b981`");">@wStatus.AnnualOccupancyRate%</div>
            <div style="font-size: 0.7rem; color: #94a3b8; margin-top: 2px;">@wStatus.AnnualUsage.ToString("N0") Adet</div>
        </div>
    </div>
</li>
"@

$content = [System.Text.RegularExpressions.Regex]::Replace($content, $workshopRegex, $newWorkshopItem)

Set-Content -Path $path -Value $content
Write-Output "Done"
