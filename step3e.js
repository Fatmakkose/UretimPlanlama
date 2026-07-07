const fs = require('fs');

let planPath = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml';
let planHtml = fs.readFileSync(planPath, 'utf8');

planHtml = planHtml.replace(/\@plannedCutQty/g, '@Model.CalculatedQuantity');

fs.writeFileSync(planPath, planHtml, 'utf8');
