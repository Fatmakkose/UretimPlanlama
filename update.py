import sys
import re

filepath = r'c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Remove Gelişmiş Filtre
content = content.replace('<button style="background: transparent; border: none; color: #0284c7; font-weight: 600; display: flex; align-items: center; gap: 8px; cursor: pointer; padding: 8px; font-size: 0.9rem;">\n                <i class="fa-solid fa-filter"></i> Gelişmiş Filtre\n            </button>', '')

# 2. Update headers
old_header = """                <thead>
                    <tr>
                        <th style="padding-left: 24px;">SİPARİŞ KODU</th>
                        <th>MODEL / DETAY</th>
                        <th>ÜRETİM YERİ & KUMAŞÇI</th>
                        <th style="text-align: right;">SİPARİŞ ADEDİ</th>
                        <th style="text-align: center;">KUMAŞ DURUMU</th>
                        <th style="text-align: center;">SİPARİŞ DURUMU</th>
                        <th style="text-align: center; width: 120px; padding-right: 24px;">İŞLEM</th>
                    </tr>
                </thead>"""

new_header = """                <thead>
                    <tr>
                        <th style="padding-left: 24px;">SİPARİŞ KODU</th>
                        <th>TARİH</th>
                        <th>MODEL / DETAY</th>
                        <th>ÜRETİM YERİ & KUMAŞÇI</th>
                        <th style="text-align: right;">SİPARİŞ ADEDİ</th>
                        <th style="text-align: center; width: 120px; padding-right: 24px;">İŞLEM</th>
                    </tr>
                </thead>"""

content = content.replace(old_header, new_header)

# 3. Update orderCode / Date tds
old_tds = r'<td style="padding-left: 24px;">\s*<a href="#" class="view-order-detail" data-id="@order\.Id" style="color: #0369a1; font-weight: 700; text-decoration: none;">@order\.OrderCode</a>\s*<span style="font-size: 0\.75rem; color: #64748b; display: block; margin-top: 4px; font-weight: 500;">\s*<i class="fa-regular fa-calendar" style="margin-right: 4px;"></i>@order\.OrderDate\.ToString\("dd\.MM\.yyyy"\)\s*</span>\s*</td>\s*<td>'

new_tds = '''<td style="padding-left: 24px;">
                                <a href="#" class="view-order-detail" data-id="@order.Id" style="color: #0369a1; font-weight: 700; text-decoration: none;">@order.OrderCode</a>
                            </td>
                            <td>
                                <span style="font-size: 0.85rem; color: #64748b; font-weight: 500;">
                                    <i class="fa-regular fa-calendar" style="margin-right: 4px;"></i>@order.OrderDate.ToString("dd.MM.yyyy")
                                </span>
                            </td>
                            <td>'''

content = re.sub(old_tds, new_tds, content)

# 4. Remove the two tds for KUMAŞ DURUMU and SİPARİŞ DURUMU
content = re.sub(r'<td style="text-align: center;">\s*@\{\s*var fs = order\.FabricStatus.*?</td>\s*<td style="text-align: center;">\s*@\{\s*var status = order\.Status.*?</td>', '', content, flags=re.DOTALL)

# 5. Add search script at the end before </script>
search_script = """
                // Arama İşlemi (Model ve Sipariş Koduna Göre)
                $("#orderSearchInput").on("keyup", function() {
                    var value = $(this).val().toLowerCase();
                    $(".order-row").filter(function() {
                        var siparisKodu = $(this).find("td:eq(0)").text().toLowerCase();
                        var modelDetay = $(this).find("td:eq(2)").text().toLowerCase();
                        $(this).toggle(siparisKodu.indexOf(value) > -1 || modelDetay.indexOf(value) > -1);
                    });
                });
"""

content = content.replace('            });\n        </script>', search_script + '\n            });\n        </script>')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Python script executed successfully.")
