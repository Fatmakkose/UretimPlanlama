const fs = require('fs');

const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml';
let text = fs.readFileSync(path, 'utf8');

text = text.replace('<button class="action-btn btnEditOrder" data-id="@firstOrder.Id" title="Rengi Düzenle"><i class="fa-solid fa-pen"></i></button>',
'<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@firstOrder.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\\n                                        <a href="/Order/Edit/@firstOrder.Id" class="action-btn" title="Siparişi Güncelle"><i class="fa-solid fa-pen"></i></a>');

text = text.replace('<button class="action-btn btnDeleteOrder" data-id="@firstOrder.Id" title="Rengi Sil"><i class="fa-solid fa-trash"></i></button>', '');
text = text.replace('<button class="action-btn btnDeleteOrderGroup" data-ids="@allIds" title="Siparişi Komple Sil"><i class="fa-solid fa-trash" style="color: #ef4444;"></i></button>', '');

text = text.replace('<button class="action-btn btnEditOrder" data-id="@order.Id" title="Rengi Düzenle"><i class="fa-solid fa-pen"></i></button>',
'<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@order.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\\n                                            <a href="/Order/Edit/@order.Id" class="action-btn" title="Siparişi Güncelle"><i class="fa-solid fa-pen"></i></a>');

text = text.replace('<button class="action-btn btnDeleteOrder" data-id="@order.Id" title="Rengi Sil"><i class="fa-solid fa-trash"></i></button>', '');

let idx = text.indexOf('// Tekil Sipariş Rengi Silme İşlemi');
if (idx > -1) {
    let idx2 = text.indexOf('// Sipariş Düzenleme Modalı Kapatma');
    if (idx2 > -1) {
        text = text.substring(0, idx) + '// Sipariş Düzenleme Modalı Kapatma' + text.substring(idx2 + '// Sipariş Düzenleme Modalı Kapatma'.length);
    }
}

let idx_edit_js = text.indexOf('// Sipariş Düzenle Modalı Bilgi Yükleme ve Açma');
if (idx_edit_js > -1) {
    let idx_edit_end = text.lastIndexOf('});\\n        </script>');
    if (idx_edit_end === -1) {
        idx_edit_end = text.lastIndexOf('});\\r\\n        </script>');
    }
    
    const clone_js = `                window.cloneOrder = function(id) {
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
                };\\n`;
    if (idx_edit_end > -1) {
        text = text.substring(0, idx_edit_js) + clone_js + text.substring(idx_edit_end);
    }
}

let lines = text.split('\\n');
if (lines.length > 2100) {
    text = lines.slice(0, 1963).concat(lines.slice(2084)).join('\\n');
}

fs.writeFileSync(path, text, 'utf8');
