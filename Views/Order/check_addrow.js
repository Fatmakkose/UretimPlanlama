const fs = require('fs');
const file = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml';
const text = fs.readFileSync(file, 'utf8');

const regex = /function editAddStockMaterialRow[\s\S]{0,4000}/i;
const match = text.match(regex);
if (match) {
    console.log(match[0].substring(2500, 4000));
} else {
    console.log('Not found');
}
