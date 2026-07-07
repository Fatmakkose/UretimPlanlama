const fs = require('fs');
const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/ProcessTracking/Track.cshtml';
let c = fs.readFileSync(path, 'utf8');

c = c.replace(/Model.PurchasingDetails/g, 'Model.PurchasingMaterialsJson');

fs.writeFileSync(path, c, 'utf8');
