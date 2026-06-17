$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Planning\Tracking.cshtml"
$content = Get-Content -Path $path -Raw -Encoding UTF8

$searchStr = '                                @if (order.IsJIT)
                                {
                                    <span class="badge badge-high" style="font-size: 0.7rem; padding: 3px 6px;">JIT</span>
                                }
                            </div>'

$progressHtml = '
                            @{
                                int progress = 0;
                                if (order.FabricArrivalActualDate.HasValue) progress += 25;
                                if (order.CuttingEndDate.HasValue) progress += 25;
                                if (order.SewingEndDate.HasValue) progress += 25;
                                if (order.PackagingEndDate.HasValue) progress += 25;
                                
                                string pColor = progress == 100 ? "#10b981" : "#0ea5e9";
                            }
                            <div style="margin-top: 12px; width: 100%; max-width: 140px;">
                                <div style="display: flex; justify-content: space-between; font-size: 0.65rem; color: var(--text-muted); font-weight:600; margin-bottom: 3px;">
                                    <span>İLERLEME</span>
                                    <span style="color:var(--text-main);">%@progress</span>
                                </div>
                                <div style="width: 100%; height: 6px; background-color: #e2e8f0; border-radius: 4px; overflow: hidden;">
                                    <div style="width: @progress%; height: 100%; background-color: @pColor; border-radius: 4px; transition: width 0.3s ease;"></div>
                                </div>
                            </div>'

$regexStr = '(?s)\s*@if \(order\.IsJIT\)\s*\{\s*<span class="badge badge-high" style="font-size: 0\.7rem; padding: 3px 6px;">JIT</span>\s*\}\s*</div>'

$replacement = $searchStr + $progressHtml
$content = $content -replace $regexStr, $replacement

[IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
