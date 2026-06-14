$path = "c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml"
$content = Get-Content -Path $path -Raw -Encoding UTF8

# 1. Remove Gelişmiş Filtre
$content = $content -replace '(?s)<button style="background: transparent; border: none; color: #0284c7; font-weight: 600; display: flex; align-items: center; gap: 8px; cursor: pointer; padding: 8px; font-size: 0.9rem;">\s*<i class="fa-solid fa-filter"></i> Gelişmiş Filtre\s*</button>', ''

# 2. Update Headers
$oldHeader = '(?s)                <thead>\s*<tr>\s*<th style="padding-left: 24px;">SİPARİŞ KODU</th>\s*<th>MODEL / DETAY</th>\s*<th>ÜRETİM YERİ & KUMAŞÇI</th>\s*<th style="text-align: right;">SİPARİŞ ADEDİ</th>\s*<th style="text-align: center;">KUMAŞ DURUMU</th>\s*<th style="text-align: center;">SİPARİŞ DURUMU</th>\s*<th style="text-align: center; width: 120px; padding-right: 24px;">İŞLEM</th>\s*</tr>\s*</thead>'

$newHeader = '                <thead>
                    <tr>
                        <th style="padding-left: 24px;">SİPARİŞ KODU</th>
                        <th>TARİH</th>
                        <th>MODEL / DETAY</th>
                        <th>ÜRETİM YERİ & KUMAŞÇI</th>
                        <th style="text-align: right;">SİPARİŞ ADEDİ</th>
                        <th style="text-align: center; width: 120px; padding-right: 24px;">İŞLEM</th>
                    </tr>
                </thead>'

$content = $content -replace $oldHeader, $newHeader

# 3. Separate Sipariş Kodu and Tarih
$oldTds = '(?s)<td style="padding-left: 24px;">\s*<a href="#" class="view-order-detail" data-id="@order\.Id" style="color: #0369a1; font-weight: 700; text-decoration: none;">@order\.OrderCode</a>\s*<span style="font-size: 0\.75rem; color: #64748b; display: block; margin-top: 4px; font-weight: 500;">\s*<i class="fa-regular fa-calendar" style="margin-right: 4px;"></i>@order\.OrderDate\.ToString\("dd\.MM\.yyyy"\)\s*</span>\s*</td>\s*<td>'

$newTds = '<td style="padding-left: 24px;">
                                <a href="#" class="view-order-detail" data-id="@order.Id" style="color: #0369a1; font-weight: 700; text-decoration: none;">@order.OrderCode</a>
                            </td>
                            <td>
                                <span style="font-size: 0.85rem; color: #64748b; font-weight: 500;">
                                    <i class="fa-regular fa-calendar" style="margin-right: 4px;"></i>@order.OrderDate.ToString("dd.MM.yyyy")
                                </span>
                            </td>
                            <td>'

$content = $content -replace $oldTds, $newTds

# 4. Remove Kumaş Durumu & Sipariş Durumu tds
$tdRegex = '(?s)<td style="text-align: center;">\s*@\{\s*var fs = order\.FabricStatus.*?</td>\s*<td style="text-align: center;">\s*@\{\s*var status = order\.Status.*?</td>'

$content = $content -replace $tdRegex, ''

# 5. Add search script at the bottom
$searchScript = '
                // Arama İşlemi (Model ve Sipariş Koduna Göre)
                $("#orderSearchInput").on("keyup", function() {
                    var value = $(this).val().toLowerCase();
                    $(".order-row").filter(function() {
                        var siparisKodu = $(this).find("td:eq(0)").text().toLowerCase();
                        var modelDetay = $(this).find("td:eq(2)").text().toLowerCase();
                        $(this).toggle(siparisKodu.indexOf(value) > -1 || modelDetay.indexOf(value) > -1);
                    });
                });
'
$content = $content -replace '(?s)            \}\);\s*</script>', ($searchScript + "`n            });`n        </script>")

[IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
