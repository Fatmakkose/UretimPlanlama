const fs = require('fs');
const text = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml', 'utf8');
const regex = /editOrderForm[\s\S]{0,100}submit/i;
const match = text.match(regex);
if (match) {
    const startIdx = Math.max(0, match.index - 50);
    console.log(text.substring(startIdx + 5000, startIdx + 8000));
} else {
    console.log("Could not find match");
}
