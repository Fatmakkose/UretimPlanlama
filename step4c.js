const fs = require('fs');
const content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const uretimStart = content.indexOf('<div id="tab-uretim"');
const formEnd = content.indexOf('</form>', uretimStart);
console.log(content.substring(formEnd - 200, formEnd + 200));
