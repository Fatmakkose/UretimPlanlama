const fs = require('fs');
const content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const uretimStart = content.indexOf('<div id="tab-uretim"');
const uretimSubstr = content.substring(uretimStart, uretimStart + 2500);

console.log(uretimSubstr);
