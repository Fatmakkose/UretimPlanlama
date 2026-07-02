import re

with open(r"c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

# Replace the HTML of editModelDetailModal
html_to_replace = r'<div id="editModelDetailModal"(.*?)<!-- Edit Model Detail Modal -->\s*<div id="editModelDetailModal".*?</div>\s*</div>\s*</div>\s*</div>'

new_html = """<!-- Edit Model Detail Modal -->
        <div id="editModelDetailModal" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(15, 23, 42, 0.5); backdrop-filter: blur(8px); z-index:1100; align-items:center; justify-content:center; animation: fadeIn 0.3s ease;">
            <div class="premium-modal-content" style="background:white; width: 850px; max-width: 95%; display:flex; flex-direction:column; border-radius:12px; box-shadow:0 20px 25px -5px rgba(0,0,0,0.1); overflow:hidden;">
                <div class="premium-header" style="display:flex; justify-content:space-between; align-items:center; padding: 16px 24px; border-bottom: 1px solid #e2e8f0; background:#f8fafc;">
                    <h5 style="margin:0; font-weight:800; font-size: 1.25rem; color:#1e293b; display:flex; align-items:center;"><i class="fa-solid fa-tags" style="margin-right:12px; font-size: 1.4rem;"></i>Model Detayları (Kumaş, Malzeme, Etiket, Tela)</h5>
                    <button type="button" onclick="closeEditModelDetailModal()" style="background:transparent; border:none; font-size:1.2rem; cursor:pointer; color:#64748b;"><i class="fa-solid fa-xmark"></i></button>
                </div>
                <div style="padding: 32px; overflow-y:auto; max-height:70vh; display:flex; flex-direction:column; gap:24px; background: #fafafa;">
                    <div style="background:#ffffff; padding:24px; border:1px solid #e2e8f0; border-radius:20px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02);">
                        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:16px;">
                            <label style="font-size:1rem; font-weight:700; color:#1e293b; margin:0; display:flex; align-items:center; gap:8px;"><i class="fa-solid fa-boxes-stacked" style="color: #3b82f6;"></i> Kullanılacak Malzemeler (Stoktan Seçim)</label>
                            <button type="button" id="btnEditAddStockMaterial" class="btn-planner" style="padding:8px 16px; font-size:0.85rem; border-radius:10px;"><i class="fa-solid fa-plus"></i> Ürün Ekle</button>
                        </div>
                        <div id="editStockMaterialsContainer" style="display: flex; flex-direction: column; gap: 20px;">
                        </div>
                        <input type="hidden" name="OrderMaterialsJson" id="editOrderMaterialsJson" value="[]" />
                    </div>
                </div>
                <div style="padding: 16px 24px; border-top: 1px solid #e2e8f0; background:#f8fafc; display:flex; justify-content:flex-end;">
                    <button type="button" class="btn-planner" onclick="closeEditModelDetailModal()"><i class="fa-solid fa-check" style="margin-right:6px;"></i> Detayları Kaydet</button>
                </div>
            </div>
        </div>"""

content = re.sub(r'<!-- Edit Model Detail Modal -->.*?</div>\s*</div>\s*</div>\s*</div>', new_html, content, flags=re.DOTALL)


# Now inject JS logic

js_injection = """
            var stokKartlariList = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(ViewBag.StokKartlari ?? new List<UretimPlanlama.Models.StokKarti>()));

            function updateEditStockMaterialsJson() {
                var materials = [];
                $('#editStockMaterialsContainer .stock-material-row').each(function() {
                    var stokId = $(this).find('.sm-stokkarti').val();
                    var miktar = $(this).find('.sm-miktar').val();
                    var aciklama = $(this).find('.sm-aciklama').val();
                    var ozellikler = {};
                    $(this).find('.sm-ozellik').each(function() {
                        var key = $(this).data('key');
                        var val = $(this).val();
                        if (key && val) ozellikler[key] = val;
                    });
                    
                    if (stokId && miktar) {
                        materials.push({
                            StokKartiId: parseInt(stokId),
                            Miktar: parseFloat(miktar),
                            Aciklama: aciklama,
                            OzelliklerJson: Object.keys(ozellikler).length > 0 ? JSON.stringify(ozellikler) : null
                        });
                    }
                });
                $('#editOrderMaterialsJson').val(JSON.stringify(materials));
            }

            function editAddStockMaterialRow(stokKartiId = '', miktar = '', aciklama = '', ozelliklerObj = {}) {
                var options = '<option value="">-- Stok Kartı Seç --</option>';
                stokKartlariList.forEach(function(s) {
                    var selected = s.Id == stokKartiId ? 'selected' : '';
                    options += `<option value="${s.Id}" ${selected}>${s.StokKodu} - ${s.StokAdi}</option>`;
                });
                var miktarVal = miktar !== null && miktar !== undefined ? miktar : '';
                var aciklamaVal = aciklama !== null && aciklama !== undefined ? aciklama : '';

                var trHtml = `
                    <div class="stock-material-row" style="background: #ffffff; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px; box-shadow: 0 2px 4px rgba(0,0,0,0.02); transition: all 0.2s;">
                        <div style="display: flex; gap: 16px; align-items: flex-end; margin-bottom: 12px; flex-wrap: wrap;">
                            <div style="flex: 3; min-width: 250px;">
                                <label style="font-size: 0.75rem; color: #64748b; margin-bottom: 6px; display: block; font-weight: 600;">Stok Kartı Seçimi</label>
                                <select class="form-control sm-stokkarti" style="width:100%; background: #f8fafc; padding: 10px 14px; border-radius: 8px; font-weight: 600; color: #1e293b;">
                                    ${options}
                                </select>
                            </div>
                            <div style="flex: 1; min-width: 100px;">
                                <label style="font-size: 0.75rem; color: #64748b; margin-bottom: 6px; display: block; font-weight: 600;">Miktar</label>
                                <input type="number" step="0.01" class="form-control sm-miktar" value="${miktarVal}" placeholder="Örn: 100" style="background: #f8fafc; padding: 10px 14px; border-radius: 8px; font-weight: 600;" />
                            </div>
                            <div style="flex: 2; min-width: 180px;">
                                <label style="font-size: 0.75rem; color: #64748b; margin-bottom: 6px; display: block; font-weight: 600;">Açıklama</label>
                                <input type="text" class="form-control sm-aciklama" value="${aciklamaVal}" placeholder="Örn: Ön panel için..." style="background: #f8fafc; padding: 10px 14px; border-radius: 8px;" />
                            </div>
                            <div>
                                <button type="button" class="btnEditRemoveStockMaterial" style="background: #fee2e2; color: #ef4444; border: none; width: 42px; height: 42px; border-radius: 10px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.2s;" title="Sil"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                        <div class="stok-detaylari" style="display:none; border-top: 1px dashed #cbd5e1; padding-top: 16px; margin-top: 0;" data-init-props='${JSON.stringify(ozelliklerObj)}'></div>
                    </div>
                `;
                var newRow = $(trHtml);
                $('#editStockMaterialsContainer').append(newRow);
                
                var newSelect = newRow.find('.sm-stokkarti');
                newSelect.select2({
                    theme: 'bootstrap-5',
                    width: '100%',
                    placeholder: '-- Stok Kartı Seç --',
                    allowClear: true
                });

                if (stokKartiId) {
                    newSelect.trigger('change');
                }
            }

            $(document).on('click', '#btnEditAddStockMaterial', function() {
                editAddStockMaterialRow();
            });

            $(document).on('click', '.btnEditRemoveStockMaterial', function() {
                $(this).closest('.stock-material-row').remove();
                updateEditStockMaterialsJson();
            });

            $(document).on('change', '.sm-stokkarti', function() {
                var select = $(this);
                var stokId = select.val();
                var detayDiv = select.closest('.stock-material-row').find('.stok-detaylari');
                
                if (!stokId) {
                    detayDiv.hide().empty();
                    updateEditStockMaterialsJson();
                    return;
                }

                var stok = stokKartlariList.find(s => s.Id == stokId);
                if (stok) {
                    var detayHtml = `<div style="display: flex; gap: 12px; margin-bottom: 20px; flex-wrap: wrap;">
                                        <div style="background: #f1f5f9; padding: 8px 16px; border-radius: 10px; font-size: 0.85rem; color: #475569; display: flex; align-items: center; gap: 8px; border: 1px solid #e2e8f0;"><i class="fa-solid fa-tag" style="color: #94a3b8;"></i> Kategori: <b style="color: #1e293b; margin-left: 4px;">${stok.Kategori || '-'}</b></div> 
                                        <div style="background: #f1f5f9; padding: 8px 16px; border-radius: 10px; font-size: 0.85rem; color: #475569; display: flex; align-items: center; gap: 8px; border: 1px solid #e2e8f0;"><i class="fa-solid fa-ruler" style="color: #94a3b8;"></i> Birim: <b style="color: #1e293b; margin-left: 4px;">${stok.Birim || '-'}</b></div> 
                                        <div style="background: #f0fdf4; border: 1px solid #bbf7d0; padding: 8px 16px; border-radius: 10px; font-size: 0.85rem; color: #166534; display: flex; align-items: center; gap: 8px;"><i class="fa-solid fa-boxes-stacked" style="color: #22c55e;"></i> Mevcut: <b style="font-size: 1rem; color: #15803d; margin-left: 4px;">${stok.MevcutMiktar || 0}</b></div>
                                     </div>`;
                    var ozellikler = [];
                    if (stok.OzelliklerJson) {
                        try {
                            ozellikler = JSON.parse(stok.OzelliklerJson);
                            if (!Array.isArray(ozellikler)) ozellikler = [];
                        } catch(e) { ozellikler = []; }
                    }
                    var initPropsStr = detayDiv.attr('data-init-props');
                    var initProps = {};
                    if(initPropsStr) {
                        try { initProps = JSON.parse(initPropsStr); } catch(e){}
                        detayDiv.removeAttr('data-init-props');
                    }

                    if (ozellikler.length > 0) {
                        var inputsHtml = '<div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; background: #f8fafc; padding: 16px; border-radius: 12px; border: 1px dashed #cbd5e1;">';
                        ozellikler.forEach(function(oz) {
                            var pVal = initProps[oz] || '';
                            inputsHtml += `<div>
                                <label style="font-size: 0.75rem; color: #475569; margin-bottom: 6px; display: block; font-weight: 600;">${oz}</label>
                                <input type="text" class="form-control sm-ozellik" data-key="${oz}" value="${pVal}" placeholder="${oz} giriniz" style="border-radius: 8px; font-size: 0.85rem;" />
                            </div>`;
                        });
                        inputsHtml += '</div>';
                        detayHtml += inputsHtml;
                    }
                    
                    detayDiv.html(detayHtml).hide().fadeIn(300);
                } else {
                    detayDiv.hide().empty();
                }
                updateEditStockMaterialsJson();
            });

            $(document).on('input change', '.sm-miktar, .sm-aciklama, .sm-ozellik', function() {
                updateEditStockMaterialsJson();
            });
"""

# Inject before $(document).ready
content = content.replace("$(document).ready(function() {", js_injection + "\n            $(document).ready(function() {", 1)


# Also in btnEditOrder success callback:
# $("#editProductionJson").val(order.productionJson || "");
# We should add logic to clear and populate #editStockMaterialsContainer
edit_population = """
                                $("#editProductionJson").val(order.productionJson || "");
                                
                                $('#editStockMaterialsContainer').empty();
                                if (order.orderMaterialsJson) {
                                    try {
                                        var mats = JSON.parse(order.orderMaterialsJson);
                                        mats.forEach(mat => {
                                            var ozObj = {};
                                            if(mat.OzelliklerJson) { try { ozObj = JSON.parse(mat.OzelliklerJson); }catch(e){} }
                                            editAddStockMaterialRow(mat.StokKartiId, mat.Miktar, mat.Aciklama, ozObj);
                                        });
                                    } catch(e){}
                                }
"""
content = content.replace('$("#editProductionJson").val(order.productionJson || "");', edit_population)

# Also in editOrderForm.on("submit":
# var acc = { KetenKarti: ...
# Remove the old acc gathering, replace with capturing editOrderMaterialsJson
submit_logic_old = """var acc = {
                        KetenKarti: $("#editAcc_KetenKarti").is(":checked"),
                        Jelatin: $("#editAcc_Jelatin").is(":checked"),
                        Cember: $("#editAcc_Cember").is(":checked"),
                        Kelebek: $("#editAcc_Kelebek").is(":checked"),
                        LotPoseti: $("#editAcc_LotPoseti").is(":checked"),

                        Tela1: parseFloat($("#editAcc_Tela1").val()) || null,
                        Tela2: parseFloat($("#editAcc_Tela2").val()) || null,
                        Cep: parseFloat($("#editAcc_Cep").val()) || null,
                        Roba: parseFloat($("#editAcc_Roba").val()) || null,
                        Apolet: parseFloat($("#editAcc_Apolet").val()) || null,
                        Diger: parseFloat($("#editAcc_Diger").val()) || null
                    };"""
submit_logic_new = "updateEditStockMaterialsJson();"

content = content.replace(submit_logic_old, submit_logic_new)

# In var formData = ..., we need to add OrderMaterialsJson. But serialize() handles the hidden input because it's inside the form!
# Wait, let's see if editOrderMaterialsJson is inside the form:
# Yes, it is in editModelDetailModal. Wait, editModelDetailModal is OUTSIDE the form!
# In Index.cshtml, the form is <form id="editOrderForm"> at line 501. It ends at line 691.
# editModelDetailModal is at line 694.
# So #editOrderMaterialsJson won't be serialized. I should append it to formData.

formdata_old = "var formData = $(this).serialize();"
formdata_new = """var formData = $(this).serialize();
                    formData += "&OrderMaterialsJson=" + encodeURIComponent($("#editOrderMaterialsJson").val() || "[]");
"""
content = content.replace(formdata_old, formdata_new)


with open(r"c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
