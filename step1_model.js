const fs = require('fs');
const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Models/Order.cs';
let content = fs.readFileSync(path, 'utf8');

const searchStr = `public string? CuttingProcessJson { get; set; } // Günlük Kesim Kayıtları`;
const replaceStr = `public string? CuttingProcessJson { get; set; } // Günlük Kesim Kayıtları\n        public string? PlannedCuttingJson { get; set; } // Kesim Planlaması Kayıtları`;

if (content.includes(searchStr)) {
    content = content.replace(searchStr, replaceStr);
    fs.writeFileSync(path, content, 'utf8');
} else {
    console.log("Could not find insertion point.");
}
