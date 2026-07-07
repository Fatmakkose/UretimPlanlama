const fs = require('fs');
let c = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml', 'utf8');

c = c.replace('<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@firstOrder.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\\n                                        ', '');
c = c.replace('<button class="action-btn" title="Yeni Renk Ekle (Çoğalt)" onclick="event.stopPropagation(); cloneOrder(@order.Id)"><i class="fa-solid fa-copy" style="color: #10b981;"></i></button>\\n                                            ', '');

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml', c);
