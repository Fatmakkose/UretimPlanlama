$lines = Get-Content -Path "Views\Planning\Plan.cshtml"
$newLines = @()

for ($i = 0; $i -lt $lines.Count; $i++) {
    # Skip the broken section we accidentally injected earlier
    if ($i -ge 395 -and $i -le 425) {
        continue
    }

    # When we hit the original broken section, we replace it
    if ($i -eq 894) { # index 894 is line 895 "<!-- Kesim ve Dikim Kartı -->"
        $newLines += "                    <!-- Kesim ve Dikim Kartı -->"
        $newLines += "                    <div class=`"content-section`">"
        $newLines += "                        <div class=`"section-title`">"
        $newLines += "                            <i class=`"fa-solid fa-scissors`"></i> Kesim ve Dikim"
        $newLines += "                        </div>"
        $newLines += "                        <div style=`"display: flex; flex-direction: column; gap: 16px;`">"
        $newLines += "                            <div>"
        $newLines += "                                <div class=`"data-label`">DİKİM ATÖLYESİ</div>"
        $newLines += "                                <select name=`"prod_dikim_atolyesi`" class=`"input-field`">"
        $newLines += "                                    <option value=`"`">-- Atölye Seçin --</option>"
        $newLines += "                                    @if (ViewBag.Workshops != null)"
        $newLines += "                                    {"
        $newLines += "                                        var prodDikim = GetProdVal(`"prod_dikim_atolyesi`");"
        $newLines += "                                        if (string.IsNullOrEmpty(prodDikim)) { prodDikim = Model.SewingWorkshop ?? Model.ProductionPlace; }"
        $newLines += "                                        foreach (var w in (System.Collections.Generic.List<UretimPlanlama.Models.Workshop>)ViewBag.Workshops)"
        $newLines += "                                        {"
        $newLines += "                                            if (!string.IsNullOrEmpty(prodDikim) && prodDikim.Equals(w.Name, StringComparison.OrdinalIgnoreCase))"
        $newLines += "                                            {"
        $newLines += "                                                <option value=`"@w.Name`" selected>@w.Name</option>"
        $newLines += "                                            }"
        $newLines += "                                            else"
        $newLines += "                                            {"
        $newLines += "                                                <option value=`"@w.Name`">@w.Name</option>"
        $newLines += "                                            }"
        $newLines += "                                        }"
        $newLines += "                                    }"
        $newLines += "                                </select>"
        $newLines += "                            </div>"

        # Skip the broken remaining lines (896, 897, 898, 899 -> indices 895, 896, 897, 898, 899)
        $i += 5
    } else {
        $newLines += $lines[$i]
    }
}

Set-Content -Path "Views\Planning\Plan.cshtml" -Value $newLines -Encoding UTF8
