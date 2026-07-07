import re
import os

path = r'c:\Users\Fatma\Downloads\UretimPlanlama-master\UretimPlanlama-master\Views\Order\Index.cshtml'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Remove duplicate HTML
# Find the start of the duplicate block
# It starts around: <div class="modal-overlay" id="editOrderModal">
# Wait, let's just find the exact text block
idx_start = text.find('<!-- Sipariş Düzenleme Modalı -->')
if idx_start > -1:
    idx_end = text.find('<!-- Sipariş Düzenleme Modalı -->', idx_start + 10)
    if idx_end > -1:
        # There are two of them! We must delete the FIRST one which is garbage, or the second one?
        # Let's delete the first one that appears.
        # Actually, let's just find the duplicate <script> tag.
        pass

# Let's just do targeted replacements!

# Replace buttons in row 1
text = text.replace('<button class="action-btn btnEditOrder" data-id="@firstOrder.Id" title="Rengi Düzenle"><i class="fa-solid fa-pen"></i></button>',
'<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@firstOrder.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\n                                        <a href="/Order/Edit/@firstOrder.Id" class="action-btn" title="Siparişi Güncelle"><i class="fa-solid fa-pen"></i></a>')

text = text.replace('<button class="action-btn btnDeleteOrder" data-id="@firstOrder.Id" title="Rengi Sil"><i class="fa-solid fa-trash"></i></button>', '')
text = text.replace('<button class="action-btn btnDeleteOrderGroup" data-ids="@allIds" title="Siparişi Komple Sil"><i class="fa-solid fa-trash" style="color: #ef4444;"></i></button>', '')

# Replace buttons in row 2
text = text.replace('<button class="action-btn btnEditOrder" data-id="@order.Id" title="Rengi Düzenle"><i class="fa-solid fa-pen"></i></button>',
'<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@order.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\n                                            <a href="/Order/Edit/@order.Id" class="action-btn" title="Siparişi Güncelle"><i class="fa-solid fa-pen"></i></a>')

text = text.replace('<button class="action-btn btnDeleteOrder" data-id="@order.Id" title="Rengi Sil"><i class="fa-solid fa-trash"></i></button>', '')

# Remove delete JS
idx = text.find('// Tekil Sipariş Rengi Silme İşlemi')
if idx > -1:
    idx2 = text.find('// Sipariş Düzenleme Modalı Kapatma')
    if idx2 > -1:
        text = text[:idx] + '// Sipariş Düzenleme Modalı Kapatma' + text[idx2 + len('// Sipariş Düzenleme Modalı Kapatma'):]

# Replace edit JS logic with cloneOrder logic
idx_edit_js = text.find('// Sipariş Düzenle Modalı Bilgi Yükleme ve Açma')
if idx_edit_js > -1:
    idx_edit_end = text.rfind('});\n        </script>')
    if idx_edit_end == -1:
        idx_edit_end = text.rfind('});\r\n        </script>')
    
    clone_js = """                window.cloneOrder = function(id) {
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
"""
    if idx_edit_end > -1:
        text = text[:idx_edit_js] + clone_js + text[idx_edit_end:]

# Now deal with the duplicate HTML!
# The file has a duplicate `<form id="editOrderForm">` and `<script>`!
# We can just use the powershell line logic that worked before!
lines = text.split('\n')
if len(lines) > 2100:
    # Remove lines 1963 to 2083 (inclusive). Remember lines are 0-indexed.
    # In powershell it was $lines[0..1962] + $lines[2084..]
    text = '\n'.join(lines[:1963] + lines[2084:])

with open(path, 'w', encoding='utf-8') as f:
    f.write(text)
