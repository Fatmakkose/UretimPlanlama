const fs = require('fs');
const files = [
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml',
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Edit.cshtml',
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Create.cshtml'
];

const target1 = 'System.Text.Json.JsonSerializer.Serialize(ViewBag.StokKartlari ?? new List<UretimPlanlama.Models.StokKarti>())';
const replacement1 = 'System.Text.Json.JsonSerializer.Serialize(ViewBag.StokKartlari ?? new List<UretimPlanlama.Models.StokKarti>(), new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles })';

const target2 = 'System.Text.Json.JsonSerializer.Serialize(ViewBag.StokKartlari)';
const replacement2 = 'System.Text.Json.JsonSerializer.Serialize(ViewBag.StokKartlari, new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles })';

for (let file of files) {
    if (fs.existsSync(file)) {
        let content = fs.readFileSync(file, 'utf8');
        content = content.replace(target1, replacement1);
        content = content.replace(target2, replacement2);
        fs.writeFileSync(file, content, 'utf8');
    }
}
