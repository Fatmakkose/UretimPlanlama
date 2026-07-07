const fs = require('fs');
const file = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml';
const text = fs.readFileSync(file, 'utf8');

const regex = /\$\(document\)\.on\(['"]click['"],\s*['"]\.stock-material-row['"][\s\S]{0,1000}/i;
const match = regex.exec(text);
if (match) {
    console.log(match[0].substring(0, 1000));
} else {
    console.log('Not found');
}
