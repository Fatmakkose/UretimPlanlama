
            
            var stokKartlariList = [];

            function updateEditStockMaterialsJson() {
                var materials = [];
                $('#editStockMaterialsContainer .stock-material-row').each(function() {
                    var stokId = $(this).find('.sm-stokkarti').val();
                    var miktar = $(this).find('.sm-miktar').val();
                    var aciklama = $(this).find('.sm-aciklama').val();
                    
                    var ozellikler = [];
                    var propStrs = [];
                    $(this).find('.sm-dyn-prop').each(function() {
                        var key = $(this).data('key');
                        var val = $(this).val();
                        if (key !== undefined && val !== undefined) {
                            ozellikler.push({ Key: key, Value: val });
                            if (!["HESAP ADETİ", "BİRİM KULLANIM", "FİRE (%)", "BİRİM METRAJ", "TOPLAM SİPARİŞ", "AÇIK ADET", "BİRİM ASORTİ", "LOT BAŞI BEDEN", "TELA BİRİM", "GRAM", "RENK"].includes((key + "").toUpperCase())) {
                                propStrs.push(key + ": " + val);
                            }
                        }
                    });
                    
                    var autoAciklama = propStrs.join(' | ');
                    $(this).find('.sm-aciklama').val(autoAciklama);
                    $(this).find('.prop-summary').text(autoAciklama);
                    aciklama = autoAciklama;
                    
                    if (stokId && (miktar || miktar === 0 || miktar === "0")) {
                        materials.push({
                            StokKartiId: parseInt(stokId),
                            Miktar: parseFloat(miktar) || 0,
                            Aciklama: aciklama,
                            OzelliklerJson: ozellikler.length > 0 ? JSON.stringify(ozellikler) : null
                        });
                    }
                });
                $('#editOrderMaterialsJson').val(JSON.stringify(materials));
            }

            function editAddStockMaterialRow(stokKartiId = '', miktar = '', aciklama = '', ozelliklerObj = {}) {
                var options = '<option value="">-- Stok Seçin --</option>';
                stokKartlariList.forEach(function(s) {
                    var selected = s.Id == stokKartiId ? 'selected' : '';
                    options += `<option value="${s.Id}" ${selected}>${s.StokAdi} (${s.StokKodu}) - Mevcut: ${s.MevcutMiktar}</option>`;
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
                                <input type="hidden" class="sm-miktar" value="${miktarVal}" />
                                <input type="hidden" class="sm-aciklama" value="${aciklamaVal}" />
                                <div class="prop-summary" style="font-size: 0.8rem; color: #3b82f6; margin-top: 6px; font-weight: 500;">${aciklamaVal}</div>
                            </div>
                            <div>
                                <button type="button" class="btnEditRemoveStockMaterial" style="background: #fee2e2; color: #ef4444; border: none; width: 42px; height: 42px; border-radius: 10px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.2s;" title="Sil"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                        <div class="stok-detaylari" style="display:none; border-top: 1px dashed #cbd5e1; padding-top: 16px; margin-top: 0;" data-init-props='${JSON.stringify(ozelliklerObj)}'></div>
                    </div>
                `;
                var newRow = $(trHtml);
                $('.stok-detaylari').slideUp(300);
                $('#editStockMaterialsContainer').append(newRow);
                
                var newSelect = newRow.find('.sm-stokkarti');
                newSelect.select2({
                    theme: 'bootstrap-5',
                    width: '100%',
                    placeholder: '-- Stok Seçin --',
                    allowClear: true
                });

                if (stokKartiId) {
                    newSelect.trigger('change');
                }
            }

            $(document).on('click', '#btnEditAddStockMaterial', function() {
                $('#editStockMaterialsContainer .stok-detaylari').slideUp(200);
                editAddStockMaterialRow();
            });

            $(document).on('click', '.btnEditRemoveStockMaterial', function() {
                $(this).closest('.stock-material-row').remove();
                updateEditStockMaterialsJson();
            });

            function getFormTotalQty() {
                return parseInt($('#editRowTotalQtyInput').val()) || 0;
            }

            $(document).on('change', '.sm-stokkarti', function() {
                var select = $(this);
                var stokId = select.val();
                var row = select.closest('.stock-material-row');
                var detayDiv = row.find('.stok-detaylari');
                
                if (!stokId) {
                    detayDiv.hide().empty();
                    row.find('.sm-miktar').prop('readonly', false).css('background', '#f8fafc');
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
                    
                    var initPropsStr = detayDiv.attr('data-init-props');
                    var initProps = {};
                    if(initPropsStr) {
                        try { 
                            var parsed = JSON.parse(initPropsStr); 
                            if (Array.isArray(parsed)) {
                                parsed.forEach(function(p) {
                                    if (p.Key && p.Value) initProps[p.Key.toUpperCase()] = p.Value;
                                });
                            } else {
                                for(var k in parsed) initProps[k.toUpperCase()] = parsed[k];
                            }
                        } catch(e){}
                        detayDiv.removeAttr('data-init-props');
                    }

                    var cat = (stok.Kategori || "").trim().toUpperCase();
                    var calcFields = [];
                    var isCalculable = false;
                    var totalQty = getFormTotalQty();

                    if (cat === "KUMAŞ" || cat === "KUMAS") {
                        isCalculable = true;
                        calcFields.push({ Key: "BİRİM METRAJ", Type: "number", Def: "" });
                    } else if (cat === "MALZEME") {
                        isCalculable = true;
                        calcFields.push({ Key: "BİRİM KULLANIM", Type: "number", Def: "" });
                        calcFields.push({ Key: "FİRE (%)", Type: "number", Def: "0" });
                    } else if (cat === "ETİKET" || cat === "ETIKET") {
                        isCalculable = true;
                        calcFields.push({ Key: "FİRE (%)", Type: "number", Def: "0" });
                    } else if (cat === "TELA") {
                        isCalculable = true;
                        calcFields.push({ Key: "TELA BİRİM", Type: "select", Def: "0.010,0.015,0.020,0.025,0.030,0.035,0.040,0.045,0.050,0.055,0.060,0.065,0.070,0.080,0.090,0.100,0.120,0.150,0.200" });
                        calcFields.push({ Key: "GRAM", Type: "select", Def: "10,15,20,25,30,35,40,45,50,55,60,65,70,80,90,100,115,120,130,140,150" });
                        calcFields.push({ Key: "RENK", Type: "text", Def: "" });
                    }

                    var extraOzellikler = [];
                    if (stok.OzelliklerJson) {
                        try {
                            var parsedOz = JSON.parse(stok.OzelliklerJson);
                            if (Array.isArray(parsedOz)) {
                                parsedOz.forEach(function(o) {
                                    if (o.Key && !calcFields.find(c => c.Key === o.Key.toUpperCase())) {
                                        extraOzellikler.push(o);
                                    }
                                });
                            }
                        } catch(e){}
                    }

                    if (isCalculable) row.find('.sm-miktar').prop('readonly', true).css('background', '#e2e8f0');
                    else row.find('.sm-miktar').prop('readonly', false).css('background', '#f8fafc');

                    if (calcFields.length > 0 || extraOzellikler.length > 0) {
                        detayHtml += `<div style="display: flex; align-items: center; gap: 10px; margin-bottom: 16px;">
                                            <div style="width: 4px; height: 18px; background: linear-gradient(135deg, #3b82f6, #2563eb); border-radius: 4px;"></div>
                                            <h6 style="color: #1e293b; font-weight: 700; font-size: 0.95rem; margin: 0; letter-spacing: -0.2px;">Hesaplama ve Özellikler</h6>
                                       </div>
                                       <div class="calc-fields-container" data-cat="${cat}" style="display:grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap:16px; background: #f8fafc; padding: 20px; border-radius: 12px; border: 1px dashed #cbd5e1;">`;
                        
                        var allFields = [];
                        calcFields.forEach(function(cf) {
                            allFields.push({ Key: cf.Key.toUpperCase(), Type: cf.Type, Value: cf.Def, IsCalc: true });
                        });
                        extraOzellikler.forEach(function(oz) {
                            allFields.push({ Key: oz.Key.toUpperCase(), Type: oz.Type, Value: oz.Value, IsCalc: false });
                        });

                        if (stok.StokAdi && stok.StokAdi.toUpperCase().includes("DÜĞME")) {
                            var orderMap = { "BOYUT": 1, "BİRİM KULLANIM": 2, "FİRE (%)": 3, "RENK": 4 };
                            allFields.sort(function(a, b) {
                                var orderA = orderMap[a.Key] || 99;
                                var orderB = orderMap[b.Key] || 99;
                                return orderA - orderB;
                            });
                        }

                        allFields.forEach(function(f) {
                             var pVal = initProps[f.Key] !== undefined ? initProps[f.Key] : (f.Value || "");
                             var inputHtml = '';
                             var calcClass = f.IsCalc ? "sm-calc-trigger" : "";
                             
                             if (f.Type === 'select') {
                                 var options = f.Value ? f.Value.split(',') : [];
                                 inputHtml = `<select class="form-control sm-dyn-prop ${calcClass}" data-key="${f.Key}" style="background: #ffffff; border-color: #cbd5e1;">
                                                <option value="">Seçiniz</option>`;
                                 options.forEach(function(opt) {
                                     var selected = (opt.trim() === pVal) ? 'selected' : '';
                                     inputHtml += `<option value="${opt.trim()}" ${selected}>${opt.trim()}</option>`;
                                 });
                                 inputHtml += `</select>`;
                             } else {
                                 var typeAttr = f.Type === 'number' ? 'number' : 'text';
                                 var stepAttr = f.Type === 'number' ? 'step="any"' : '';
                                 inputHtml = `<input type="${typeAttr}" ${stepAttr} class="form-control sm-dyn-prop ${calcClass}" data-key="${f.Key}" value="${pVal}" placeholder="${f.Value || '...'}" style="background: #ffffff; border-color: #cbd5e1; font-weight:600;" />`;
                             }
                             
                             detayHtml += `<div>
                                            <label style="font-size:0.75rem; font-weight:700; color:#64748b; display:block; margin-bottom:6px;">${f.Key}</label>
                                            ${inputHtml}
                                          </div>`;
                        });
                        
                        detayHtml += `</div>`;
                    }
                    
                    $('.stok-detaylari').not(detayDiv).slideUp(200);
                    detayDiv.html(detayHtml).hide().slideDown(200);
                    if (isCalculable) row.find('.sm-calc-trigger').first().trigger('input');
                } else {
                    detayDiv.hide().empty();
                    row.find('.sm-miktar').prop('readonly', false).css('background', '#f8fafc');
                }
                updateEditStockMaterialsJson();
            });

            $(document).on('input change', '.sm-calc-trigger', function() {
                var row = $(this).closest('.stock-material-row');
                var container = row.find('.calc-fields-container');
                var cat = container.data('cat');
                var miktarInput = row.find('.sm-miktar');
                
                function getVal(key) {
                    var v = container.find(`.sm-dyn-prop[data-key="${key}"]`).val();
                    return parseFloat(v) || 0;
                }

                var miktar = 0;
                if (cat === "KUMAŞ" || cat === "KUMAS") {
                    var adet = getFormTotalQty();
                    var metraj = getVal("BİRİM METRAJ");
                    miktar = adet * metraj;
                } else if (cat === "MALZEME") {
                    // Malzeme için hesap adeti satın alma planlamasından geldiği için burada Miktar = 0 olarak kalır, özellikler kaydedilir.
                    miktar = 0;
                } else if (cat === "ETİKET" || cat === "ETIKET") {
                    // Etiket için adetler renk tablosundan satın alma planlamasına gidecek. Burada Miktar = 0 kalır.
                    miktar = 0;
                } else if (cat === "TELA") {
                    var toplam = getFormTotalQty();
                    var birim = getVal("TELA BİRİM");
                    miktar = toplam * birim;
                }

                if (miktarInput.prop('readonly')) {
                    miktarInput.val(Number.isInteger(miktar) ? miktar : miktar.toFixed(3));
                }
                updateEditStockMaterialsJson();
            });

            $(document).on('input change', '.sm-miktar, .sm-aciklama, .sm-dyn-prop, .sm-ozellik', function() {
                if (!$(this).hasClass('sm-calc-trigger')) {
                    updateEditStockMaterialsJson();
                }
            });

            $(document).on('click', '.stock-material-row', function(e) {
                if ($(e.target).closest('.btnRemoveStockMaterial, .btnEditRemoveStockMaterial, .select2-container, .sm-stokkarti, .stok-detaylari').length > 0) return;
                
                var detayDiv = $(this).find('.stok-detaylari');
                if (detayDiv.children().length === 0) return;
                
                if (detayDiv.is(':visible')) {
                    detayDiv.slideUp(200);
                } else {
                    $('.stok-detaylari').not(detayDiv).slideUp(200);
                    detayDiv.slideDown(200);
                }
            });

            $(document).ready(function() {
                var colorOptionsHtml = '<option value="">-- Renk Seçin --</option>';
        
        }





                $(document).on("click", ".fabric-status-badge, .order-status-badge", function(e) {
                    e.stopPropagation();
                    var menu = $(this).siblings(".custom-dropdown-menu");
                    $(".custom-dropdown-menu").not(menu).fadeOut(100); 
                    menu.fadeToggle(150);
                });

                $(document).click(function() {
                    $(".custom-dropdown-menu").fadeOut(150);
                });


                $(document).on("click", ".order-status-menu .dropdown-item", function(e) {
                    e.preventDefault();
                    var orderId = $(this).data("id");
                    var newStatus = $(this).data("status");
                    var badge = $(this).closest(".custom-dropdown-container").find(".order-status-badge");

                    $.ajax({
                        url: "/Order/UpdateStatus",
                        type: "POST",
                        data: { id: orderId, status: newStatus },
                        success: function(response) {
                            if (response.success) {

                                badge.html(newStatus + ' <i class="fa-solid fa-chevron-down" style="font-size: 0.65rem;"></i>');


                                badge.removeClass("badge-progress badge-medium badge-high badge-scheduled");
                                if (newStatus === "Tamamlandı") {
                                    badge.addClass("badge-progress");
                                } else if (newStatus === "İptal Edildi") {
                                    badge.addClass("badge-high");
                                } else if (newStatus === "Yeni Kayıt") {
                                    badge.addClass("badge-scheduled");
                                } else {
                                    badge.addClass("badge-medium");
                                }

                                showToast("Sipariş durumu başarıyla güncellendi: " + newStatus, "success");
                            } else {
                                showToast("Güncelleme başarısız: " + response.message, "danger");
                            }
                        },
                        error: function() {
                            showToast("Sunucuyla bağlantı kurulurken hata oluştu.", "danger");
                        }
                    });
                });

                $(document).on("click", ".fabric-status-menu .dropdown-item", function(e) {
                    e.preventDefault();
                    var orderId = $(this).data("id");
                    var newStatus = $(this).data("status");
                    var badge = $(this).closest(".custom-dropdown-container").find(".fabric-status-badge");

                    $.ajax({
                        url: "/Order/UpdateFabricStatus",
                        type: "POST",
                        data: { id: orderId, status: newStatus },
                        success: function(response) {
                            if (response.success) {
                                badge.html(newStatus + ' <i class="fa-solid fa-chevron-down" style="font-size: 0.65rem;"></i>');

                                badge.removeClass("badge-progress badge-medium badge-high badge-scheduled");
                                if (newStatus === "Tamamlandı") {
                                    badge.addClass("badge-progress");
                                } else if (newStatus === "Kısmi Geldi") {
                                    badge.addClass("badge-medium");
                                } else {
                                    badge.addClass("badge-high");
                                }

                                showToast("Kumaş durumu başarıyla güncellendi: " + newStatus, "success");
                            } else {
                                showToast("Güncelleme başarısız: " + response.message, "danger");
                            }
                        },
                        error: function() {
                            showToast("Sunucuyla bağlantı kurulurken hata oluştu.", "danger");
                        }
                    });
                });


                function showToast(message, type) {

                    $("#floatingToast").remove();

                    var bgColor = type === "success" ? "var(--success-bg)" : "var(--danger-bg)";
                    var textColor = type === "success" ? "var(--success-text)" : "var(--danger-text)";
                    var borderClr = type === "success" ? "rgba(13, 148, 136, 0.2)" : "rgba(220, 38, 38, 0.2)";
                    var icon = type === "success" ? "fa-solid fa-circle-check" : "fa-solid fa-circle-exclamation";

                    var toast = $('<div id="floatingToast" style="position: fixed; top: 20px; right: 20px; background-color: ' + bgColor + '; color: ' + textColor + '; border: 1px solid ' + borderClr + '; padding: 16px 24px; border-radius: 12px; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1); z-index: 9999; display: flex; align-items: center; gap: 10px; font-weight: 500; font-size: 0.95rem; min-width: 300px; animation: slideInRight 0.3s cubic-bezier(0.16, 1, 0.3, 1);">' +
                        '<i class="' + icon + '" style="font-size: 1.2rem;"></i>' +
                        '<span>' + message + '</span>' +
                        '</div>');

                    $("body").append(toast);


                    if ($("#toastStyles").length === 0) {
                        $("head").append('<style id="toastStyles">' +
                            '@0 slideInRight {' +
                            'from { transform: translateX(100%); opacity: 0; }' +
                            'to { transform: translateX(0); opacity: 1; }' +
                            '}' +
                            '</style>');
                    }


                    setTimeout(function() {
                        $("#floatingToast").fadeOut(400, function() {
                            $(this).remove();
                        });
                    }, 3000);
                }

                // Sipariş Detay Modalı İşlemleri
                $(document).on("click", ".view-order-detail", function(e) {
                    e.preventDefault();
                    var orderId = $(this).data("id");

                    $.ajax({
                        url: "/Order/GetDetail",
                        type: "GET",
                        data: { id: orderId },
                        success: function(response) {
                            if (response.success) {
                                var order = response.data;

                                // Genel Bilgiler
                                $("#detOrderCode").text(order.orderCode || "-");
                                $("#detOrderDate").html(formatDetailDate(order.orderDate));
                                $("#detPlannedPackagingEndDate").html(formatDetailDate(order.plannedPackagingEndDate));
                                
                                var revizeTerminDet = "";
                                if (order.productionJson) {
                                    try {
                                        var prodJsonDet = JSON.parse(order.productionJson);
                                        revizeTerminDet = prodJsonDet["prod_knn_revize_termin"] || "";
                                    } catch(e) {}
                                }
                                $("#detKnnRevizeTermin").html(formatDetailDate(revizeTerminDet));
                                $("#detModelName").text(order.modelName || "-");
                                $("#detColor").text(order.color || "-");
                                $("#detBrand").text(order.brand || "-");
                                $("#detCustomer").text(order.customer || "LC Waikiki");
                                $("#detDeliveryPlace").text(order.deliveryPlace || "-");
                                $("#detDescription").text(order.goodsDescription || "-");
                                $("#detUnitPrice").html(formatCurrency(order.unitPrice || order.componentUnitPrice));
                                $("#detQuantity").text((order.quantity || 0).toLocaleString('tr-TR') + " Adet");

                                // Rozetler (Badges)
                                var status = order.status || "Yeni Kayıt";
                                $("#detStatusBadge").text(status);
                                $("#detStatusBadge").removeClass("badge-progress badge-medium badge-high badge-scheduled");
                                if (status === "Tamamlandı") $("#detStatusBadge").addClass("badge-progress");
                                else if (status === "İptal Edildi") $("#detStatusBadge").addClass("badge-high");
                                else if (status === "Yeni Kayıt") $("#detStatusBadge").addClass("badge-scheduled");
                                else $("#detStatusBadge").addClass("badge-medium");

                                var fStatus = order.fabricStatus || "Bekleniyor";
                                $("#detFabricBadge").text("Kumaş: " + fStatus);
                                $("#detFabricBadge").removeClass("badge-progress badge-medium badge-high badge-scheduled");
                                if (fStatus === "Tamamlandı" || fStatus === "Kumaş Tam") $("#detFabricBadge").addClass("badge-progress");
                                else if (fStatus === "Kısmi Geldi" || fStatus === "Gecikti") $("#detFabricBadge").addClass("badge-medium");
                                else $("#detFabricBadge").addClass("badge-high");

                                $("#detJITBadge").hide();

                                // Beden Dağılımı (Dinamik)
                                var openDist = {};
                                var asortiDist = {};
                                try { openDist = JSON.parse(order.sizeDistributionJson || "{}"); } catch(e){}
                                try { asortiDist = JSON.parse(order.asortiDistributionJson || "{}"); } catch(e){}
                                
                                var allSizesSet = new Set();
                                Object.keys(openDist).forEach(k => allSizesSet.add(k));
                                Object.keys(asortiDist).forEach(k => allSizesSet.add(k));
                                
                                var standardOrder = ["XXS", "XS", "S", "M", "L", "XL", "2XL", "3XL", "4XL", "5XL", "6XL", "36", "38", "40", "42", "44", "46", "2 Yaş", "4 Yaş", "6 Yaş", "8 Yaş", "10 Yaş", "12 Yaş"];
                                var allSizes = Array.from(allSizesSet).sort(function(a, b) {
                                    var iA = standardOrder.indexOf(a);
                                    var iB = standardOrder.indexOf(b);
                                    if (iA !== -1 && iB !== -1) return iA - iB;
                                    if (iA !== -1) return -1;
                                    if (iB !== -1) return 1;
                                    return a.localeCompare(b);
                                });

                                var tableHtml = '<table style="width: 100%; border-collapse: collapse; text-align: center; font-size: 0.9rem;"><thead><tr style="border-bottom: 2px solid var(--border-color); font-weight: 600; color: var(--text-muted);"><th style="padding: 8px; text-align: left;">Beden Türü</th>';
                                allSizes.forEach(s => { tableHtml += '<th style="padding: 8px;">' + s + '</th>'; });
                                tableHtml += '<th style="padding: 8px; border-left: 1px solid var(--border-color); color: var(--text-main);">Toplam</th></tr></thead><tbody>';

                                // Açık Adetler
                                var openTotal = 0;
                                tableHtml += '<tr style="border-bottom: 1px solid var(--border-color);"><td style="padding: 12px 8px; text-align: left; font-weight: 600; color: #0284c7;">Açık Adet</td>';
                                allSizes.forEach(s => { 
                                    var val = parseInt(openDist[s]) || 0;
                                    openTotal += val;
                                    tableHtml += '<td style="padding: 12px 8px;">' + val + '</td>';
                                });
                                tableHtml += '<td style="padding: 12px 8px; border-left: 1px solid var(--border-color); font-weight: 700; color: #0284c7; background: #e0f2fe;">' + openTotal + '</td></tr>';

                                // Asorti Oran
                                var asortiRatioTotal = 0;
                                tableHtml += '<tr style="border-bottom: 1px solid var(--border-color);"><td style="padding: 12px 8px; text-align: left; font-weight: 600; color: #16a34a;">Asorti Oran</td>';
                                allSizes.forEach(s => { 
                                    var val = parseInt(asortiDist[s]) || 0;
                                    asortiRatioTotal += val;
                                    tableHtml += '<td style="padding: 12px 8px;">' + val + '</td>';
                                });
                                tableHtml += '<td style="padding: 12px 8px; border-left: 1px solid var(--border-color); font-weight: 700; color: #16a34a; background: #dcfce7;">' + asortiRatioTotal + '</td></tr>';

                                // Asorti Toplam
                                var asortiCount = order.asortiCount || 0;
                                var asortiFinalQty = asortiRatioTotal * asortiCount;
                                tableHtml += '<tr><td style="padding: 12px 8px; text-align: left; font-weight: 600; color: #15803d;">Asorti Toplam</td>';
                                tableHtml += '<td colspan="' + allSizes.length + '" style="padding: 12px 8px; text-align: right; color: var(--text-muted); font-style: italic;">Lot Sayısı: <strong style="color: var(--text-main); font-style: normal; margin-right: 15px;">' + asortiCount + '</strong> (Lot Sayısı × Asorti Oran Toplamı) =</td>';
                                tableHtml += '<td style="padding: 12px 8px; border-left: 1px solid var(--border-color); font-weight: 700; color: #15803d; background: #bbf7d0;">' + asortiFinalQty + '</td></tr>';

                                tableHtml += '</tbody></table>';

                                $("#detDynamicSizesContainer").html(tableHtml);

                                // Model Detayları & Aksesuarlar (Dinamik HTML)
                                 var accHtml = '';
                                 var materials = [];
                                 try { materials = JSON.parse(order.orderMaterialsJson || "[]"); } catch(e){}
                                 
                                 if (materials && materials.length > 0) {
                                     var grouped = {};
                                     materials.forEach(function(m) {
                                         var cat = (m.Kategori || "Diğer").trim().toUpperCase();
                                         if (!grouped[cat]) grouped[cat] = [];
                                         grouped[cat].push(m);
                                     });
                                     
                                     for (var category in grouped) {
                                         accHtml += '<div style="margin-top:12px;"><h5 style="margin:0 0 8px 0; font-size:0.85rem; color:#475569; border-bottom: 1px solid #e2e8f0; padding-bottom:4px;">' + category + '</h5>';
                                         accHtml += '<div style="display: grid; grid-template-columns: 1fr; gap: 8px;">';
                                         grouped[category].forEach(function(item) {
                                             var displayName = (item.StokAdi || item.StokKodu || 'Bilinmeyen Ürün');
                                             var qtyDisplay = '';
                                             if (category !== 'KUMAŞ' && category !== 'MALZEME' && category !== 'ETİKET' && category !== 'TELA') {
                                                 qtyDisplay = '<br/><span style="color:#64748b; font-size:0.8rem;">Miktar: ' + item.Miktar + '</span>';
                                             }
                                             var aciklamaDisplay = item.Aciklama ? '<br/><span style="color:#0284c7; font-size:0.8rem; font-weight: 500;">' + item.Aciklama + '</span>' : '';
                                             accHtml += '<div style="background:#f8fafc; padding:8px; border-radius:6px; border:1px solid #e2e8f0; font-size:0.85rem;">' +
                                                        '<strong>' + displayName + '</strong>' + qtyDisplay + aciklamaDisplay + '</div>';
                                         });
                                         accHtml += '</div></div>';
                                     }
                                 } else {
                                     accHtml = '<div style="color:#94a3b8; font-size:0.85rem; font-style:italic;">Bu siparişte model detayı seçilmemiştir.</div>';
                                 }

                                 $("#detAccessoriesContainer").html(accHtml);

                                // Planlama & İş Takvimi
                                // 1. Kumaş
                                $("#detFabricSupplier").text(order.fabricSupplier || "-");
                                $("#detTargetFabric").text(order.targetFabricQty ? parseFloat(order.targetFabricQty).toLocaleString('tr-TR', { maximumFractionDigits: 2 }) + " kg" : "-");
                                $("#detActualFabric").text(order.actualFabricQty ? parseFloat(order.actualFabricQty).toLocaleString('tr-TR', { maximumFractionDigits: 2 }) + " kg" : "-");
                                $("#detFabricAgreedDate").html(formatDetailDate(order.fabricArrivalAgreedDate));
                                $("#detFabricActualDate").html(formatDetailDate(order.fabricArrivalActualDate));
                                $("#detFabricMeterage").text(order.fabricMeterage ? order.fabricMeterage.toLocaleString('tr-TR') + " m" : "-");

                                // 2. Kesim
                                $("#detPlannedCuttingStart").html(formatDetailDate(order.plannedCuttingStartDate));
                                $("#detCuttingStart").html(formatDetailDate(order.cuttingStartDate));
                                $("#detPlannedCuttingEnd").html(formatDetailDate(order.plannedCuttingEndDate));
                                $("#detCuttingEnd").html(formatDetailDate(order.cuttingEndDate));

                                // 3. Dikim
                                $("#detSewingWorkshop").text(order.sewingWorkshop || "-");
                                $("#detPlannedSewingStart").html(formatDetailDate(order.plannedSewingStartDate));
                                $("#detSewingStart").html(formatDetailDate(order.sewingStartDate));
                                $("#detPlannedSewingEnd").html(formatDetailDate(order.plannedSewingEndDate));
                                $("#detSewingEnd").html(formatDetailDate(order.sewingEndDate));

                                // 4. Paket & Kalite
                                $("#detPlannedPackagingStart").html(formatDetailDate(order.plannedPackagingStartDate));
                                $("#detPackagingStart").html(formatDetailDate(order.packagingStartDate));
                                $("#detPlannedPackagingEnd").html(formatDetailDate(order.plannedPackagingEndDate));
                                $("#detPackagingEnd").html(formatDetailDate(order.packagingEndDate));
                                $("#detPlannedLastInspection").html(formatDetailDate(order.plannedLastInspectionDate));
                                $("#detLastInspection").html(formatDetailDate(order.lastInspectionDate));

                                // 5. Sevkiyat
                                $("#detDepartureDate").html(formatDetailDate(order.departureDate));
                                $("#detWarehouseDate").html(formatDetailDate(order.warehouseArrivalDate));

                                // Modalı Aç
                                $("#orderDetailModal").css("display", "flex").hide().fadeIn(200, function() {
                                    $(this).addClass("active");
                                });
                            } else {
                                showToast("Hata: " + response.message, "danger");
                            }
                        },
                        error: function() {
                            showToast("Sipariş detayları alınırken bir hata oluştu.", "danger");
                        }
                    });
                });

                function closeDetailModal() {
                    $("#orderDetailModal").removeClass("active").fadeOut(200);
                }

                $("#btnCloseDetailModal, #btnCloseDetailModalBottom, #orderDetailModal").click(function(e) {
                    if (e.target === this || $(e.target).closest('#btnCloseDetailModal, #btnCloseDetailModalBottom').length > 0) {
                        closeDetailModal();
                    }
                });

                function formatDetailDate(dateStr) {
                    if (!dateStr) return '<span style="color:var(--text-muted); font-size:0.8rem;">Girilmedi</span>';
                    var d = new Date(dateStr);
                    if (isNaN(d.getTime())) return '<span style="color:var(--text-muted); font-size:0.8rem;">-</span>';
                    var day = String(d.getDate()).padStart(2, '0');
                    var month = String(d.getMonth() + 1).padStart(2, '0');
                    var year = d.getFullYear();
                    return `${day}.${month}.${year}`;
                }

                function formatCurrency(val) {
                    if (val === null || val === undefined) return '<span style="color:var(--text-muted);">Girilmedi</span>';
                    return "₺ " + parseFloat(val).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }

                // Arama İşlemi (Sipariş Kodu, Model Adı ve Detaylara Göre)
                $("#orderSearchInput").on("keyup", function() {
                    var value = $(this).val().toLowerCase();
                    $(".main-order-row").filter(function() {
                        var siparisKodu = $(this).find("td:eq(0)").text().toLowerCase();
                        var modelName = $(this).find("td:eq(2)").text().toLowerCase();
                        var colors = $(this).find("td:eq(3)").text().toLowerCase();
                        var isMatch = siparisKodu.indexOf(value) > -1 || modelName.indexOf(value) > -1 || colors.indexOf(value) > -1;
                        $(this).toggle(isMatch);
                        if (!isMatch) {
                            var groupClass = $(this).data("groupclass");
                            if (groupClass) {
                                $("." + groupClass).hide();
                                $(this).find('.toggle-icon').removeClass('fa-rotate-90');
                            }
                        }
                    });
                });

                // Tekil Sipariş Rengi Silme İşlemi
                $(document).on("click", ".btnDeleteOrder", function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var orderId = $(this).data("id");
                    if (confirm("Bu renk kaydını silmek istediğinizden emin misiniz?")) {
                        $.ajax({
                            url: "/Order/Delete",
                            type: "POST",
                            data: { id: orderId },
                            success: function(response) {
                                if (response.success) {
                                    showToast("Kayıt başarıyla silindi.", "success");
                                    setTimeout(function() {
                                        window.location.reload();
                                    }, 1000);
                                } else {
                                    showToast("Silme hatası: " + response.message, "danger");
                                }
                            },
                            error: function() {
                                showToast("Kayıt silinirken sunucu hatası oluştu.", "danger");
                            }
                        });
                    }
                });

                // Sipariş Grubunu (Komple) Silme İşlemi
                $(document).on("click", ".btnDeleteOrderGroup", function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var idsString = $(this).data("ids");
                    var idsArray = String(idsString).split(',').map(Number);
                    
                    if (confirm("Bu siparişe ait TÜM renk kayıtlarını silmek istediğinizden emin misiniz?")) {
                        $.ajax({
                            url: "/Order/DeleteMultiple",
                            type: "POST",
                            contentType: "application/json",
                            data: JSON.stringify(idsArray),
                            success: function(response) {
                                if (response.success) {
                                    showToast("Sipariş grubu başarıyla silindi.", "success");
                                    setTimeout(function() {
                                        window.location.reload();
                                    }, 1000);
                                } else {
                                    showToast("Silme hatası: " + response.message, "danger");
                                }
                            },
                            error: function() {
                                showToast("Silinirken sunucu hatası oluştu.", "danger");
                            }
                        });
                    }
                });

                // Sipariş Düzenleme Modalı Kapatma
                function closeEditModal() {
                    $("#editOrderModal").removeClass("active").fadeOut(200);
                    $("#editOrderForm")[0].reset();
                }

                $("#btnEditCloseModal, #btnEditCancelModal, #editOrderModal").click(function(e) {
                    if (e.target === this || $(e.target).closest('#btnEditCloseModal, #btnEditCancelModal').length > 0) {
                        closeEditModal();
                    }
                });

                // Düzenle formundaki Inspection Tarihi durum kontrolü
                $('#editInspectionType').change(function() {
                    if ($(this).val()) {
                        $('#editInspectionDate').prop('disabled', false);
                    } else {
                        $('#editInspectionDate').prop('disabled', true);
                        $('#editInspectionDate').val('');
                    }
                });

                // --- DİNAMİK BEDEN DEĞİŞKENLERİ VE FONKSİYONLARI (EDİT MODAL) ---
                var editOpenSizes = {};
                var editAsortiSizes = {};
                var activeSizes = ["S", "M", "L", "XL", "2XL", "3XL", "4XL", "5XL", "6XL", "7XL", "8XL", "34", "36", "38", "40", "42", "44", "46", "48", "50", "52", "54", "56", "58", "60", "STD"];

                function renderEditSizeBadgesToContainer(type) {
                    var containerId = type === 'open' ? '#editDynSizesContainerOpen' : '#editDynSizesContainerAsorti';
                    var sizesObj = type === 'open' ? editOpenSizes : editAsortiSizes;
                    var $container = $(containerId);
                    $container.empty();
                    var total = 0;
                    
                    for (var size in sizesObj) {
                        var val = sizesObj[size];
                        total += val;
                        var badgeHtml = `
                            <div class="size-badge" style="display:flex; align-items:center; background:#f1f5f9; border:1px solid #cbd5e1; border-radius:6px; overflow:hidden;">
                                <span style="background:#e2e8f0; padding:4px 8px; font-weight:700; font-size:0.8rem; color:#334155; border-right:1px solid #cbd5e1;">${size}</span>
                                <input type="number" min="0" class="edit-dyn-size-input" data-type="${type}" data-size="${size}" value="${val}" style="width:40px; border:none; padding:4px; font-size:0.85rem; text-align:center; outline:none; background:transparent;" />
                            </div>`;
                        $container.append(badgeHtml);
                    }
                    
                    if (type === 'open') {
                        $("#editOpenTotalInput").val(total);
                        $("#editOpenTotalText").text(total);
                    } else {
                        $("#editAsortiTotalInput").val(total);
                        $("#editAsortiTotalText").text(total);
                    }
                    calculateEditTotals();
                }

                function populateEditCheckboxes(type) {
                    var containerId = type === 'open' ? '#editOpenCheckboxesContainer' : '#editAsortiCheckboxesContainer';
                    var sizesObj = type === 'open' ? editOpenSizes : editAsortiSizes;
                    var $container = $(containerId);
                    $container.empty();

                    activeSizes.forEach(function(size) {
                        var isChecked = sizesObj.hasOwnProperty(size) ? "checked" : "";
                        var chkHtml = `<label style="display:flex; align-items:center; gap:6px; font-size:0.85rem; cursor:pointer; padding:4px 8px; border-radius:4px; transition:background 0.2s;" onmouseover="this.style.background='#f1f5f9'" onmouseout="this.style.background='transparent'">
                                        <input type="checkbox" class="edit-dyn-size-checkbox" data-type="${type}" value="${size}" ${isChecked} /> ${size}
                                       </label>`;
                        $container.append(chkHtml);
                    });
                }

                $(document).on("click", ".btn-edit-size-select", function(e) {
                    e.stopPropagation();
                    var $dropdown = $(this).siblings(".edit-size-dropdown-content");
                    $(".edit-size-dropdown-content").not($dropdown).hide();
                    $dropdown.toggle();
                });

                $(document).click(function() {
                    $(".edit-size-dropdown-content").hide();
                });

                $(".edit-size-dropdown-content").click(function(e) {
                    e.stopPropagation();
                });

                $(document).on("change", ".edit-dyn-size-checkbox", function() {
                    var type = $(this).data("type");
                    var size = $(this).val();
                    var isChecked = $(this).is(":checked");
                    
                    var sizesObj = type === 'open' ? editOpenSizes : editAsortiSizes;

                    if (isChecked) {
                        if (!sizesObj.hasOwnProperty(size)) {
                            sizesObj[size] = 0;
                        }
                    } else {
                        delete sizesObj[size];
                    }
                    renderEditSizeBadgesToContainer(type);
                });

                $(document).on("input", ".edit-dyn-size-input", function() {
                    var type = $(this).data("type");
                    var size = $(this).data("size");
                    var val = parseInt($(this).val()) || 0;
                    
                    var sizesObj = type === 'open' ? editOpenSizes : editAsortiSizes;
                    if (sizesObj.hasOwnProperty(size)) {
                        sizesObj[size] = val;
                    }
                    
                    var total = 0;
                    for(var k in sizesObj) { total += sizesObj[k]; }
                    
                    if (type === 'open') {
                        $("#editOpenTotalInput").val(total);
                        $("#editOpenTotalText").text(total);
                    } else {
                        $("#editAsortiTotalInput").val(total);
                        $("#editAsortiTotalText").text(total);
                    }
                    calculateEditTotals();
                });

                window.openEditModelDetailModal = function() {
                    $("#editModelDetailModal").css("display", "flex");
                };
                window.closeEditModelDetailModal = function() {
                    $("#editModelDetailModal").hide();
                };

                // Düzenle Modalı Hesaplamaları
                function calculateEditTotals() {
                    var openTotal = parseInt($("#editOpenTotalInput").val()) || 0;
                    var asortiTotalRatio = parseInt($("#editAsortiTotalInput").val()) || 0;
                    
                    var asortiCount = parseInt($("#editAsortiCountInput").val()) || 0;
                    var asortiFinal = asortiTotalRatio * asortiCount;
                    $("#editAsortiFinalInput").val(asortiFinal);

                    var grandQty = openTotal + asortiFinal;
                    $("#editRowTotalQtyInput").val(grandQty);
                    $("#editQty").val(grandQty);

                    // Finansal hesaplamalar
                    var price = parseFloat($('#editUnitPrice').val()) || 0;
                    var total = grandQty * price;
                    var vat = total * 0.10;
                    var grandTotal = total + vat;

                    $('#editTotalAmt').val(total.toFixed(2));
                    $('#editVatAmt').val(vat.toFixed(2));
                    $('#editGrandTotal').val(grandTotal.toFixed(2));
                }

                $(document).on("input", ".edit-asorti-count, .edit-calc-trigger", function() {
                    calculateEditTotals();
                });

                // Sipariş Düzenle Modalı Bilgi Yükleme ve Açma
                $(document).on("click", ".btnEditOrder", function(e) {
                    e.preventDefault();
                    var orderId = $(this).data("id");

                    $.ajax({
                        url: "/Order/GetDetail",
                        type: "GET",
                        data: { id: orderId },
                        success: function(response) {
                            if (response.success) {
                                var order = response.data;

                                $("#editOrderId").val(order.id);
                                var $submitBtn = $("#editOrderForm button[type='submit']");
                                if($submitBtn.length === 0) { $submitBtn = $(".btn-planner").first(); }
                                $submitBtn.html('<i class="fa-solid fa-save"></i> Siparişi Güncelle')
                                         .addClass('btn-planner')
                                         .css({'background-color': '', 'border-color': ''});

                                if (order.orderDate) {
                                    $("#editOrderDate").val(order.orderDate.substring(0, 10));
                                }
                                if (order.plannedPackagingEndDate) {
                                    $("#editPlannedPackagingEndDate").val(order.plannedPackagingEndDate.substring(0, 10));
                                } else {
                                    $("#editPlannedPackagingEndDate").val("");
                                }
                                
                                $("#editProductionJson").val(order.productionJson || "");

                                $('#editStockMaterialsContainer').empty();
                                if (order.orderMaterialsJson) {
                                    try {
                                        console.log("Raw OrderMaterialsJson:", order.orderMaterialsJson);
                                        var mats = JSON.parse(order.orderMaterialsJson);
                                        console.log("Parsed mats:", mats);
                                        mats.forEach(mat => {
                                            try {
                                                var ozObj = {};
                                                var ozJson = mat.OzelliklerJson || mat.ozelliklerJson;
                                                if (ozJson) {
                                                    if (typeof ozJson === 'string') {
                                                        try { ozObj = JSON.parse(ozJson); } catch(e){}
                                                    } else {
                                                        ozObj = ozJson;
                                                    }
                                                }
                                                var sId = mat.StokKartiId || mat.stokKartiId;
                                                var mik = mat.Miktar !== undefined ? mat.Miktar : (mat.miktar || '');
                                                var acik = mat.Aciklama || mat.aciklama || '';
                                                editAddStockMaterialRow(sId, mik, acik, ozObj);
                                            } catch(err) {
                                                console.error("Error adding material row:", err, mat);
                                            }
                                        });
                                    } catch(e) {
                                        console.error("Error parsing orderMaterialsJson:", e);
                                    }
                                }
                                var revizeTermin = "";
                                if (order.productionJson) {
                                    try {
                                        var prodJson = JSON.parse(order.productionJson);
                                        revizeTermin = prodJson["prod_knn_revize_termin"] || "";
                                    } catch(e) {}
                                }
                                $("#editKnnRevizeTermin").val(revizeTermin);

                                $("#editOrderCode").val(order.orderCode || "");
                                $("#editPaymentMethod").val(order.paymentMethod || "");
                                $("#editManufacturerCompany").val(order.manufacturerCompany || "");
                                $("#editCustomer").val(order.customer || "LC Waikiki");
                                $("#editModelName").val(order.modelName || "");
                                $("#editColor").val(order.color || "");
                                $("#editGoodsDescription").val(order.goodsDescription || "");
                                $("#editBrand").val(order.brand || "LC WAIKIKI");
                                $("#editFabricSupplier").val(order.fabricSupplier || "");

                                // Bedenler - Dinamik JSON Parse
                                try {
                                    editOpenSizes = order.sizeDistributionJson ? JSON.parse(order.sizeDistributionJson) : {};
                                } catch(e) { editOpenSizes = {}; }
                                
                                try {
                                    editAsortiSizes = order.asortiDistributionJson ? JSON.parse(order.asortiDistributionJson) : {};
                                } catch(e) { editAsortiSizes = {}; }
                                
                                // Geriye dönük uyumluluk için
                                if (Object.keys(editOpenSizes).length === 0) {
                                    if(order.sizeS > 0) editOpenSizes["S"] = order.sizeS;
                                    if(order.sizeM > 0) editOpenSizes["M"] = order.sizeM;
                                    if(order.sizeL > 0) editOpenSizes["L"] = order.sizeL;
                                    if(order.sizeXL > 0) editOpenSizes["XL"] = order.sizeXL;
                                    if(order.size2XL > 0) editOpenSizes["2XL"] = order.size2XL;
                                    if(order.size3XL > 0) editOpenSizes["3XL"] = order.size3XL;
                                }
                                
                                if (Object.keys(editAsortiSizes).length === 0) {
                                    if(order.asortiSizeS > 0) editAsortiSizes["S"] = order.asortiSizeS;
                                    if(order.asortiSizeM > 0) editAsortiSizes["M"] = order.asortiSizeM;
                                    if(order.asortiSizeL > 0) editAsortiSizes["L"] = order.asortiSizeL;
                                    if(order.asortiSizeXL > 0) editAsortiSizes["XL"] = order.asortiSizeXL;
                                    if(order.asortiSize2XL > 0) editAsortiSizes["2XL"] = order.asortiSize2XL;
                                    if(order.asortiSize3XL > 0) editAsortiSizes["3XL"] = order.asortiSize3XL;
                                }
                                
                                populateEditCheckboxes('open');
                                populateEditCheckboxes('asorti');
                                renderEditSizeBadgesToContainer('open');
                                renderEditSizeBadgesToContainer('asorti');

                                $("#editAsortiCountInput").val(order.asortiCount || 0);
                                $("#editUnitPrice").val(order.unitPrice || order.componentUnitPrice || "");

                                // Model Detayları ve Aksesuarlar
                                $("#editUnitFabricMeterage").val(order.unitFabricMeterage || "");
                                $("#editFabricUnit").val(order.fabricUnit || "Metraj (m)");
                                $("#editLargeButtonCount").val(order.largeButtonCount || "");
                                $("#editSmallButtonCount").val(order.smallButtonCount || "");
                                $("#editKusakAstarGram").val(order.kusakAstarGram || "");
                                $("#editKusakTelaTipi").val(order.kusakTelaTipi || "");
                                $("#editKusakTelaRenk").val(order.kusakTelaRenk || "");
                                $("#editYakaAstarGram").val(order.yakaAstarGram || "");
                                $("#editYakaTelaTipi").val(order.yakaTelaTipi || "");
                                $("#editYakaTelaRenk").val(order.yakaTelaRenk || "");
                                $("#editMansetAstarGram").val(order.mansetAstarGram || "");
                                $("#editMansetTelaTipi").val(order.mansetTelaTipi || "");
                                $("#editMansetTelaRenk").val(order.mansetTelaRenk || "");
                                $("#editKapakAstarGram").val(order.kapakAstarGram || "");
                                $("#editKapakTelaTipi").val(order.kapakTelaTipi || "");
                                $("#editKapakTelaRenk").val(order.kapakTelaRenk || "");
                                $("#editBossAstarGram").val(order.bossAstarGram || "");
                                $("#editBossTelaTipi").val(order.bossTelaTipi || "");
                                $("#editBossTelaRenk").val(order.bossTelaRenk || "");
                                $("#editPatAstarGram").val(order.patAstarGram || "");
                                $("#editPatTelaTipi").val(order.patTelaTipi || "");
                                $("#editPatTelaRenk").val(order.patTelaRenk || "");

                                $("#editHasPriceCard").prop("checked", order.hasPriceCard || false);
                                $("#editHasWashingInstruction").prop("checked", order.hasWashingInstruction || false);
                                $("#editHasInnerBarcode").prop("checked", order.hasInnerBarcode || false);
                                $("#editHasYokeLabel").prop("checked", order.hasYokeLabel || false);
                                $("#editHasFifLabel").prop("checked", order.hasFifLabel || false);
                                $("#editHasOtherCard").prop("checked", order.hasOtherCard || false);

                                // Fabrics
                                $('#editFabricsContainer').empty();
                                var editFabrics = [];
                                try {
                                    editFabrics = order.fabricsJson ? JSON.parse(order.fabricsJson) : [];
                                } catch(e) { editFabrics = []; }
                                
                                if (editFabrics && editFabrics.length > 0) {
                                    editFabrics.forEach(function(f) {
                                        addEditFabricRow(f.Name, f.Meterage, f.Unit, f.WastageRate, f.Button1Size, f.Button1Qty, f.Button2Size, f.Button2Qty);
                                    });
                                } else if (order.unitFabricMeterage > 0) {
                                    addEditFabricRow('Ana Kumaş', order.unitFabricMeterage, order.fabricUnit, order.wastageRate, '24/', order.largeButtonCount, '14/', order.smallButtonCount);
                                } else {
                                    addEditFabricRow();
                                }
                                
                                // Tela check setup
                                $('.chk-edit-tela').each(function() {
                                    var target = $(this).data('target');
                                    var astarInput = $('#' + target + 'AstarGram');
                                    var telaTipiInput = $('#' + target + 'TelaTipi');
                                    var telaInput = $('#' + target + 'TelaRenk');
                                    if (astarInput.val() || telaTipiInput.val() || telaInput.val()) {
                                        $(this).prop('checked', true);
                                        astarInput.prop('disabled', false).css('background', 'white');
                                        telaTipiInput.prop('disabled', false).css('background', 'white');
                                        telaInput.prop('disabled', false).css('background', 'white');
                                    } else {
                                        $(this).prop('checked', false);
                                        astarInput.prop('disabled', true).css('background', '#f8fafc');
                                        telaTipiInput.prop('disabled', true).css('background', '#f8fafc');
                                        telaInput.prop('disabled', true).css('background', '#f8fafc');
                                    }
                                });

                                var acc = {};
                                try {
                                    acc = order.selectedAccessoriesJson ? JSON.parse(order.selectedAccessoriesJson) : {};
                                } catch(e) { acc = {}; }
                                
                                $("#editAcc_KetenKarti").prop("checked", acc.KetenKarti || false);
                                $("#editAcc_Jelatin").prop("checked", acc.Jelatin || false);
                                $("#editAcc_Cember").prop("checked", acc.Cember || false);
                                $("#editAcc_Kelebek").prop("checked", acc.Kelebek || false);
                                $("#editAcc_LotPoseti").prop("checked", acc.LotPoseti || false);

                                $("#editAcc_Tela1").val(acc.Tela1 || "");
                                $("#editAcc_Tela2").val(acc.Tela2 || "");
                                $("#editAcc_Cep").val(acc.Cep || "");
                                $("#editAcc_Roba").val(acc.Roba || "");
                                $("#editAcc_Apolet").val(acc.Apolet || "");
                                $("#editAcc_Diger").val(acc.Diger || "");

                                calculateEditTotals();

                                $("#editOrderModal").css("display", "flex").hide().fadeIn(200, function() {
                                    $(this).addClass("active");
                                });
                            } else {
                                showToast("Hata: " + response.message, "danger");
                            }
                        },
                        error: function() {
                            showToast("Sipariş bilgileri yüklenirken hata oluştu.", "danger");
                        }
                    });
                });

                // Edit Fabrics Logic
                function addEditFabricRow(name = '', meterage = '', unit = 'm', wastage = '', btn1Size = '24/', btn1Qty = '', btn2Size = '14/', btn2Qty = '') {
                    var rowHtml = `
                        <div class="edit-fabric-row" style="display:grid; grid-template-columns: 2fr 2fr 1fr 1fr 1fr auto; gap:12px; align-items:end; background:white; padding:12px; border:1px solid #cbd5e1; border-radius:6px; position:relative;">
                            <div>
                                <label style="font-size:0.75rem; font-weight:600; color:#64748b; margin-bottom:4px; display:block;">Kumaş Adı / Kodu</label>
                                <input type="text" class="form-control edit-fabric-name" value="${name}" placeholder="Örn: Ana Kumaş" />
                            </div>
                            <div>
                                <label style="font-size:0.75rem; font-weight:600; color:#64748b; margin-bottom:4px; display:block;">Birim Metraj</label>
                                <div style="display:flex; gap:4px;">
                                    <input type="number" step="0.01" class="form-control edit-fabric-meterage no-spinner" value="${meterage}" style="flex:1; min-width:70px; text-align:center; padding: 0.375rem 0.5rem;" />
                                    <select class="form-control edit-fabric-unit" style="width:65px; padding:0 4px; flex-shrink:0;">
                                        <option value="m" ${unit === 'm' || unit === 'Metraj (m)' ? 'selected' : ''}>m</option>
                                        <option value="Kg" ${unit === 'Kg' ? 'selected' : ''}>Kg</option>
                                    </select>
                                </div>
                            </div>
                            <div>
                                <label style="font-size:0.75rem; font-weight:600; color:#64748b; margin-bottom:4px; display:block; white-space:nowrap;">Fire (%)</label>
                                <input type="number" step="0.01" class="form-control edit-fabric-wastage no-spinner" value="${wastage}" placeholder="5" style="text-align:center;" />
                            </div>
                            <div>
                                <select class="form-control edit-fabric-btn1-size" style="font-size:0.75rem; font-weight:600; color:#64748b; margin-bottom:4px; border:none; background:transparent; padding:0; height:auto; cursor:pointer;">
                                    <option value="">1. Düğme Boyu</option>
                                    <option value="14/" ${btn1Size === '14/' ? 'selected' : ''}>14/ Düğme</option>
                                    <option value="16/" ${btn1Size === '16/' ? 'selected' : ''}>16/ Düğme</option>
                                    <option value="18/" ${btn1Size === '18/' ? 'selected' : ''}>18/ Düğme</option>
                                    <option value="20/" ${btn1Size === '20/' ? 'selected' : ''}>20/ Düğme</option>
                                    <option value="24/" ${btn1Size === '24/' ? 'selected' : ''}>24/ Düğme</option>
                                    <option value="28/" ${btn1Size === '28/' ? 'selected' : ''}>28/ Düğme</option>
                                    <option value="32/" ${btn1Size === '32/' ? 'selected' : ''}>32/ Düğme</option>
                                </select>
                                <input type="number" step="1" class="form-control edit-fabric-btn1-qty no-spinner" value="${btn1Qty}" style="text-align:center;" placeholder="Adet" />
                            </div>
                            <div>
                                <select class="form-control edit-fabric-btn2-size" style="font-size:0.75rem; font-weight:600; color:#64748b; margin-bottom:4px; border:none; background:transparent; padding:0; height:auto; cursor:pointer;">
                                    <option value="">2. Düğme Boyu</option>
                                    <option value="14/" ${btn2Size === '14/' ? 'selected' : ''}>14/ Düğme</option>
                                    <option value="16/" ${btn2Size === '16/' ? 'selected' : ''}>16/ Düğme</option>
                                    <option value="18/" ${btn2Size === '18/' ? 'selected' : ''}>18/ Düğme</option>
                                    <option value="20/" ${btn2Size === '20/' ? 'selected' : ''}>20/ Düğme</option>
                                    <option value="24/" ${btn2Size === '24/' ? 'selected' : ''}>24/ Düğme</option>
                                    <option value="28/" ${btn2Size === '28/' ? 'selected' : ''}>28/ Düğme</option>
                                    <option value="32/" ${btn2Size === '32/' ? 'selected' : ''}>32/ Düğme</option>
                                </select>
                                <input type="number" step="1" class="form-control edit-fabric-btn2-qty no-spinner" value="${btn2Qty}" style="text-align:center;" placeholder="Adet" />
                            </div>
                            <div style="padding-bottom: 6px;">
                                <button type="button" class="btnRemoveEditFabric" style="background:transparent; border:none; color:#dc2626; cursor:pointer; padding:4px;" title="Sil"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                    `;
                    $('#editFabricsContainer').append(rowHtml);
                    updateEditFabricsJson();
                }

                $(document).on('click', '#btnEditAddFabric', function() {
                    addEditFabricRow();
                });

                $(document).on('click', '.btnRemoveEditFabric', function() {
                    $(this).closest('.edit-fabric-row').remove();
                    updateEditFabricsJson();
                });

                $(document).on('input change', '.edit-fabric-row input, .edit-fabric-row select', function() {
                    updateEditFabricsJson();
                });

                function updateEditFabricsJson() {
                    var fabrics = [];
                    $('.edit-fabric-row').each(function(index) {
                        var meterage = parseFloat($(this).find('.edit-fabric-meterage').val()) || null;
                        var unit = $(this).find('.edit-fabric-unit').val();
                        var wastage = parseFloat($(this).find('.edit-fabric-wastage').val()) || null;
                        var btn1Size = $(this).find('.edit-fabric-btn1-size').val();
                        var btn1Qty = parseInt($(this).find('.edit-fabric-btn1-qty').val()) || null;
                        var btn2Size = $(this).find('.edit-fabric-btn2-size').val();
                        var btn2Qty = parseInt($(this).find('.edit-fabric-btn2-qty').val()) || null;

                        fabrics.push({
                            Name: $(this).find('.edit-fabric-name').val() || ("Kumaş " + (index + 1)),
                            Meterage: meterage,
                            Unit: unit === 'm' ? 'Metraj (m)' : 'Kg',
                            WastageRate: wastage,
                            Button1Size: btn1Size,
                            Button1Qty: btn1Qty,
                            Button2Size: btn2Size,
                            Button2Qty: btn2Qty,
                            LargeButtonCount: btn1Qty,
                            SmallButtonCount: btn2Qty
                        });
                        
                        if (index === 0) {
                            $('#editUnitFabricMeterage').val(meterage || '');
                            $('#editFabricUnit').val(unit === 'm' ? 'Metraj (m)' : 'Kg');
                            $('#editWastageRate').val(wastage || '');
                            $('#editLargeButtonCount').val(btn1Qty || '');
                            $('#editSmallButtonCount').val(btn2Qty || '');
                        }
                    });

                    if (fabrics.length === 0) {
                        $('#editUnitFabricMeterage').val('');
                        $('#editWastageRate').val('');
                        $('#editLargeButtonCount').val('');
                        $('#editSmallButtonCount').val('');
                    }

                    $('#editFabricsJson').val(JSON.stringify(fabrics));
                }

                // Tela check logic
                $(document).on('change', '.chk-edit-tela', function() {
                    var targetPrefix = $(this).data('target');
                    var isChecked = $(this).is(':checked');
                    
                    var astarInput = $('#' + targetPrefix + 'AstarGram');
                    var telaTipiInput = $('#' + targetPrefix + 'TelaTipi');
                    var telaInput = $('#' + targetPrefix + 'TelaRenk');
                    
                    if (isChecked) {
                        astarInput.prop('disabled', false).css('background', 'white');
                        telaTipiInput.prop('disabled', false).css('background', 'white');
                        telaInput.prop('disabled', false).css('background', 'white');
                    } else {
                        astarInput.prop('disabled', true).css('background', '#f8fafc').val('');
                        telaTipiInput.prop('disabled', true).css('background', '#f8fafc').val('');
                        telaInput.prop('disabled', true).css('background', '#f8fafc').val('');
                    }
                });

                // Düzenleme Formu Gönderimi (AJAX)
                $("#editOrderForm").on("submit", function(e) {
                    e.preventDefault();

                    updateEditStockMaterialsJson();

                    Object.keys(editOpenSizes).forEach(function(key) {
                        if (editOpenSizes[key] <= 0) delete editOpenSizes[key];
                    });

                    Object.keys(editAsortiSizes).forEach(function(key) {
                        if (editAsortiSizes[key] <= 0) delete editAsortiSizes[key];
                    });

                    var calculatedOpenTotal = 0;
                    for (var k in editOpenSizes) { calculatedOpenTotal += editOpenSizes[k]; }
                    
                    var calculatedAsortiRatio = 0;
                    for (var k in editAsortiSizes) { calculatedAsortiRatio += editAsortiSizes[k]; }
                    
                    var asortiCountVal = parseInt($("#editAsortiCountInput").val()) || 0;
                    var finalQuantity = calculatedOpenTotal + (calculatedAsortiRatio * asortiCountVal);
                    
                    var unitPriceVal = parseFloat($("#editUnitPrice").val()) || 0;
                    var finalTotal = finalQuantity * unitPriceVal;
                    var finalVat = finalTotal * 0.10;
                    var finalGrand = finalTotal + finalVat;

                    var acc = {};
                    var orderObj = {
                        OrderMaterialsJson: $("#editOrderMaterialsJson").val() || "[]",
                        Id: parseInt($("#editOrderId").val()),
                        OrderDate: $("#editOrderDate").val(),
                        OrderCode: $("#editOrderCode").val(),
                        PaymentMethod: $("#editPaymentMethod").val(),
                        ManufacturerCode: $("#editManufacturerCode").val(),
                        ManufacturerCompany: $("#editManufacturerCompany").val(),
                        Customer: $("#editCustomer").val(),
                        ModelName: $("#editModelName").val(),
                        Color: $("#editColor").val(),
                        GoodsDescription: $("#editGoodsDescription").val(),
                        Brand: $("#editBrand").val(),
                        FabricSupplier: $("#editFabricSupplier").val(),

                        ComponentUnitPrice: parseFloat($("#editUnitPrice").val()) || null,
                        UnitPrice: parseFloat($("#editUnitPrice").val()) || null,
                        AsortiCount: parseInt($("#editAsortiCountInput").val()) || 0,
                        PlannedPackagingEndDate: $("#editPlannedPackagingEndDate").val() || null,
                        Quantity: finalQuantity,
                        TotalAmount: finalTotal,
                        VatAmount: finalVat,
                        TotalAmountWithVat: finalGrand,

                        ProductionJson: (function() {
                            var prodJson = {};
                            var existingStr = $("#editProductionJson").val();
                            if (existingStr) {
                                try { prodJson = JSON.parse(existingStr); } catch(e){}
                            }
                            var rTermin = $("#editKnnRevizeTermin").val();
                            if (rTermin) {
                                prodJson["prod_knn_revize_termin"] = rTermin;
                            } else {
                                delete prodJson["prod_knn_revize_termin"];
                            }
                            return JSON.stringify(prodJson);
                        })(),

                        SizeDistributionJson: JSON.stringify(editOpenSizes),
                        AsortiDistributionJson: JSON.stringify(editAsortiSizes),

                        UnitFabricMeterage: parseFloat($("#editUnitFabricMeterage").val()) || null,
                        FabricUnit: $("#editFabricUnit").val(),
                        WastageRate: parseFloat($("#editWastageRate").val()) || null,
                        LargeButtonCount: parseInt($("#editLargeButtonCount").val()) || null,
                        SmallButtonCount: parseInt($("#editSmallButtonCount").val()) || null,
                        KusakAstarGram: $("#editKusakAstarGram").val() || null,
                        KusakTelaTipi: $("#editKusakTelaTipi").val() || null,
                        KusakTelaRenk: $("#editKusakTelaRenk").val() || null,
                        YakaAstarGram: $("#editYakaAstarGram").val() || null,
                        YakaTelaTipi: $("#editYakaTelaTipi").val() || null,
                        YakaTelaRenk: $("#editYakaTelaRenk").val() || null,
                        MansetAstarGram: $("#editMansetAstarGram").val() || null,
                        MansetTelaTipi: $("#editMansetTelaTipi").val() || null,
                        MansetTelaRenk: $("#editMansetTelaRenk").val() || null,
                        KapakAstarGram: $("#editKapakAstarGram").val() || null,
                        KapakTelaTipi: $("#editKapakTelaTipi").val() || null,
                        KapakTelaRenk: $("#editKapakTelaRenk").val() || null,
                        BossAstarGram: $("#editBossAstarGram").val() || null,
                        BossTelaTipi: $("#editBossTelaTipi").val() || null,
                        BossTelaRenk: $("#editBossTelaRenk").val() || null,
                        PatAstarGram: $("#editPatAstarGram").val() || null,
                        PatTelaTipi: $("#editPatTelaTipi").val() || null,
                        PatTelaRenk: $("#editPatTelaRenk").val() || null,

                        HasPriceCard: $("#editHasPriceCard").is(":checked"),
                        HasWashingInstruction: $("#editHasWashingInstruction").is(":checked"),
                        HasInnerBarcode: $("#editHasInnerBarcode").is(":checked"),


                // Tela check logic
                $(document).on('change', '.chk-edit-tela', function() {
                    var targetPrefix = $(this).data('target');
                    var isChecked = $(this).is(':checked');
                    
                    var astarInput = $('#' + targetPrefix + 'AstarGram');
                    var telaTipiInput = $('#' + targetPrefix + 'TelaTipi');
                    var telaInput = $('#' + targetPrefix + 'TelaRenk');
                    
                    if (isChecked) {
                        astarInput.prop('disabled', false).css('background', 'white');
                        telaTipiInput.prop('disabled', false).css('background', 'white');
                        telaInput.prop('disabled', false).css('background', 'white');
                    } else {
                        astarInput.prop('disabled', true).css('background', '#f8fafc').val('');
                        telaTipiInput.prop('disabled', true).css('background', '#f8fafc').val('');
                        telaInput.prop('disabled', true).css('background', '#f8fafc').val('');
                    }
                });

                // Düzenleme Formu Gönderimi (AJAX)
                $("#editOrderForm").on("submit", function(e) {
                    e.preventDefault();

                    updateEditStockMaterialsJson();

                    Object.keys(editOpenSizes).forEach(function(key) {
                        if (editOpenSizes[key] <= 0) delete editOpenSizes[key];
                    });

                    Object.keys(editAsortiSizes).forEach(function(key) {
                        if (editAsortiSizes[key] <= 0) delete editAsortiSizes[key];
                    });

                    var calculatedOpenTotal = 0;
                    for (var k in editOpenSizes) { calculatedOpenTotal += editOpenSizes[k]; }
                    
                    var calculatedAsortiRatio = 0;
                    for (var k in editAsortiSizes) { calculatedAsortiRatio += editAsortiSizes[k]; }
                    
                    var asortiCountVal = parseInt($("#editAsortiCountInput").val()) || 0;
                    var finalQuantity = calculatedOpenTotal + (calculatedAsortiRatio * asortiCountVal);
                    
                    var unitPriceVal = parseFloat($("#editUnitPrice").val()) || 0;
                    var finalTotal = finalQuantity * unitPriceVal;
                    var finalVat = finalTotal * 0.10;
                    var finalGrand = finalTotal + finalVat;

                    var acc = {};
                    var orderObj = {
                        OrderMaterialsJson: $("#editOrderMaterialsJson").val() || "[]",
                        Id: parseInt($("#editOrderId").val()),
                        OrderDate: $("#editOrderDate").val(),
                        OrderCode: $("#editOrderCode").val(),
                        PaymentMethod: $("#editPaymentMethod").val(),
                        ManufacturerCode: $("#editManufacturerCode").val(),
                        ManufacturerCompany: $("#editManufacturerCompany").val(),
                        Customer: $("#editCustomer").val(),
                        ModelName: $("#editModelName").val(),
                        Color: $("#editColor").val(),
                        GoodsDescription: $("#editGoodsDescription").val(),
                        Brand: $("#editBrand").val(),
                        FabricSupplier: $("#editFabricSupplier").val(),

                        ComponentUnitPrice: parseFloat($("#editUnitPrice").val()) || null,
                        UnitPrice: parseFloat($("#editUnitPrice").val()) || null,
                        AsortiCount: parseInt($("#editAsortiCountInput").val()) || 0,
                        PlannedPackagingEndDate: $("#editPlannedPackagingEndDate").val() || null,
                        Quantity: finalQuantity,
                        TotalAmount: finalTotal,
                        VatAmount: finalVat,
                        TotalAmountWithVat: finalGrand,

                        ProductionJson: (function() {
                            var prodJson = {};
                            var existingStr = $("#editProductionJson").val();
                            if (existingStr) {
                                try { prodJson = JSON.parse(existingStr); } catch(e){}
                            }
                            var rTermin = $("#editKnnRevizeTermin").val();
                            if (rTermin) {
                                prodJson["prod_knn_revize_termin"] = rTermin;
                            } else {
                                delete prodJson["prod_knn_revize_termin"];
                            }
                            return JSON.stringify(prodJson);
                        })(),

                        SizeDistributionJson: JSON.stringify(editOpenSizes),
                        AsortiDistributionJson: JSON.stringify(editAsortiSizes),

                        UnitFabricMeterage: parseFloat($("#editUnitFabricMeterage").val()) || null,
                        FabricUnit: $("#editFabricUnit").val(),
                        WastageRate: parseFloat($("#editWastageRate").val()) || null,
                        LargeButtonCount: parseInt($("#editLargeButtonCount").val()) || null,
                        SmallButtonCount: parseInt($("#editSmallButtonCount").val()) || null,
                        KusakAstarGram: $("#editKusakAstarGram").val() || null,
                        KusakTelaTipi: $("#editKusakTelaTipi").val() || null,
                        KusakTelaRenk: $("#editKusakTelaRenk").val() || null,
                        YakaAstarGram: $("#editYakaAstarGram").val() || null,
                        YakaTelaTipi: $("#editYakaTelaTipi").val() || null,
                        YakaTelaRenk: $("#editYakaTelaRenk").val() || null,
                        MansetAstarGram: $("#editMansetAstarGram").val() || null,
                        MansetTelaTipi: $("#editMansetTelaTipi").val() || null,
                        MansetTelaRenk: $("#editMansetTelaRenk").val() || null,
                        KapakAstarGram: $("#editKapakAstarGram").val() || null,
                        KapakTelaTipi: $("#editKapakTelaTipi").val() || null,
                        KapakTelaRenk: $("#editKapakTelaRenk").val() || null,
                        BossAstarGram: $("#editBossAstarGram").val() || null,
                        BossTelaTipi: $("#editBossTelaTipi").val() || null,
                        BossTelaRenk: $("#editBossTelaRenk").val() || null,
                        PatAstarGram: $("#editPatAstarGram").val() || null,
                        PatTelaTipi: $("#editPatTelaTipi").val() || null,
                        PatTelaRenk: $("#editPatTelaRenk").val() || null,

                        HasPriceCard: $("#editHasPriceCard").is(":checked"),
                        HasWashingInstruction: $("#editHasWashingInstruction").is(":checked"),
                        HasInnerBarcode: $("#editHasInnerBarcode").is(":checked"),
                        HasYokeLabel: $("#editHasYokeLabel").is(":checked"),
                        HasFifLabel: $("#editHasFifLabel").is(":checked"),
                        HasOtherCard: $("#editHasOtherCard").is(":checked"),

                        SelectedAccessoriesJson: JSON.stringify(acc),
                        FabricsJson: $("#editFabricsJson").val(),

                        SizeS: editOpenSizes["S"] || 0,
                        SizeM: editOpenSizes["M"] || 0,
                        SizeL: editOpenSizes["L"] || 0,
                        SizeXL: editOpenSizes["XL"] || 0,
                        Size2XL: editOpenSizes["2XL"] || 0,
                        Size3XL: editOpenSizes["3XL"] || 0,

                        AsortiSizeS: editAsortiSizes["S"] || 0,
                        AsortiSizeM: editAsortiSizes["M"] || 0,
                        AsortiSizeL: editAsortiSizes["L"] || 0,
                        AsortiSizeXL: editAsortiSizes["XL"] || 0,
                        AsortiSize2XL: editAsortiSizes["2XL"] || 0,
                        AsortiSize3XL: editAsortiSizes["3XL"] || 0
                    };
                    var isNewColor = $(document.activeElement).attr("id") === "btnSaveAsNewColor";
                    var targetUrl = '/Order/Edit';
                    var targetData = orderObj;

                    if (isNewColor) {
                        orderObj.Id = 0;
                        targetUrl = '/Order/CreateMultiple';
                        targetData = [orderObj];
                    }

                    $.ajax({
                        url: targetUrl,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(targetData),
                        success: function(res) {
                            if (res.success) {
                                window.location.reload();
                            } else {
                                showToast("Hata: " + res.message, "danger");
                            }
                        },
                        error: function() {
                            showToast("Kayıt sırasında sunucu hatası oluştu.", "danger");
                        }
                    });
                });

                window.cloneOrder = function(id) {
                    if (!confirm('Bu siparişi kopyalayarak yeni bir renk eklemek istediğinize emin misiniz?')) return;
                    $.post('/Order/CloneOrder', { id: id }, function(res) {
                        if (res.success) {
                            window.location.href = '/Order/Edit/' + res.newId + '?isClone=true';
                        } else {
                            showToast("Hata: " + res.message, "danger");
                        }
                    }).fail(function() {
                        showToast("Kayıt sırasında sunucu hatası oluştu.", "danger");
                    });
                };

            });
        