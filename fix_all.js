const fs = require('fs');

let planHtml = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');
let kesimHtml = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/extract_kesim_replaced.txt', 'utf8');

// 1. Replace plannedCutQty with Model.CalculatedQuantity in Plan.cshtml
planHtml = planHtml.replace(/\\@plannedCutQty/g, '@Model.CalculatedQuantity');

// 2. Modify KesimHtml for the new requirements
// Rename title
kesimHtml = kesimHtml.replace('Kesim Süreci (Günlük Takip)', 'Kesim Planlaması');
// Rename Tarih column
kesimHtml = kesimHtml.replace(/<th>Tarih<\/th>/g, '<th>Planlanan Tarih</th>');
kesimHtml = kesimHtml.replace(/<th style="(.*?)">Tarih<\/th>/g, '<th style="$1">Planlanan Tarih</th>');

// Remove the outer tab div and replace with a simple block div
const outerDivStart = kesimHtml.indexOf('<div id="tab-kesim"');
const outerDivEnd = kesimHtml.indexOf('>', outerDivStart) + 1;
kesimHtml = kesimHtml.substring(0, outerDivStart) + '<div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">' + kesimHtml.substring(outerDivEnd);

// 3. Insert KesimHtml BEFORE the IsProductionCompleted div
const isProdCompletedStr = '<input type="checkbox" name="IsProductionCompleted"';
const isProdCompletedIndex = planHtml.indexOf(isProdCompletedStr);

if (isProdCompletedIndex === -1) {
    console.log("Could not find IsProductionCompleted");
    process.exit(1);
}

// Find the parent div of IsProductionCompleted
const parentDivIndex = planHtml.lastIndexOf('<div style="display: flex; justify-content: space-between; margin-top: 20px; align-items: center;">', isProdCompletedIndex);

if (parentDivIndex === -1) {
    console.log("Could not find parentDivIndex");
    process.exit(1);
}

// Insert it!
planHtml = planHtml.substring(0, parentDivIndex) + kesimHtml + '\n<br/>\n' + planHtml.substring(parentDivIndex);

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', planHtml, 'utf8');
console.log("Fix applied.");
