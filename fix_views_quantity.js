const fs = require('fs');

const filePaths = [
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml',
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/ProcessTracking/Index.cshtml',
    'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml'
];

filePaths.forEach(path => {
    if (fs.existsSync(path)) {
        let content = fs.readFileSync(path, 'utf8');
        content = content.replace(/o\.Quantity\.ToString\(/g, 'o.CalculatedQuantity.ToString(');
        content = content.replace(/order\.Quantity\.ToString\(/g, 'order.CalculatedQuantity.ToString(');
        content = content.replace(/firstOrder\.Quantity\.ToString\(/g, 'firstOrder.CalculatedQuantity.ToString(');
        fs.writeFileSync(path, content, 'utf8');
    }
});
