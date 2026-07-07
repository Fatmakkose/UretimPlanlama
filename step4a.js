const fs = require('fs');
const content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const kesimStart = content.indexOf('<!-- KESİM SÜRECİ TAB -->');
console.log(content.substring(kesimStart - 500, kesimStart + 200));
