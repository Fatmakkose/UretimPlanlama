const fs = require('fs');

let content = fs.readFileSync('Views/Order/Index.cshtml', 'utf8');

// The goal is to:
// 1. Define acc as empty object or restore its structure so JSON.stringify(acc) works without error
// 2. Add OrderMaterialsJson to orderObj.

// Look for orderObj definition:
const orderObjRegex = /var orderObj = \{[\s\S]*?SelectedAccessoriesJson: JSON\.stringify\(acc\),/g;

// Wait, let's just replace the SelectedAccessoriesJson and add OrderMaterialsJson.
// Also define acc just in case it's used.

content = content.replace('var orderObj = {', 'var acc = {};\n                    var orderObj = {\n                        OrderMaterialsJson: $("#editOrderMaterialsJson").val() || "[]",');

fs.writeFileSync('Views/Order/Index.cshtml', content, 'utf8');
console.log('Fixed OrderObj in Index.cshtml');
