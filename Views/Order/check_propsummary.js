const fs = require('fs');
const file = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml';
const text = fs.readFileSync(file, 'utf8');

const regex = /\.prop-summary/g;
const matches = [...text.matchAll(regex)];
matches.forEach(m => {
    const startIdx = Math.max(0, m.index - 50);
    console.log(text.substring(startIdx, startIdx + 200));
    console.log("-----------------------");
});
