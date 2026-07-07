const fs = require('fs');
const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/ProcessTracking/Track.cshtml';
let c = fs.readFileSync(path, 'utf8');

c = c.replace(/min-width:\s*70px/g, 'min-width: 95px; padding: 4px;');

fs.writeFileSync(path, c, 'utf8');
