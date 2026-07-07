const fs = require('fs');

let trackHtml = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/ProcessTracking/Track.cshtml', 'utf8');

// Find the start of cutting script
const startVar = trackHtml.indexOf('let cuttingItems');
const endVar = trackHtml.indexOf(';', startVar) + 1;
let varCode = trackHtml.substring(startVar, endVar);

const startFunc = trackHtml.indexOf('// --- TAB 3: KESİM SÜRECİ ---');
const endFunc = trackHtml.indexOf('function showToast', startFunc); // just before showToast, or grab until the end of script
let funcCode = trackHtml.substring(startFunc, endFunc);

let combinedJS = "\n" + varCode + "\n\n" + funcCode;

// Replace for Plan.cshtml
const replacements = [
    ['Model.CuttingProcessJson', 'Model.PlannedCuttingJson'],
    ['cuttingItems', 'planCuttingItems'],
    ['saveCutting', 'planSaveCuttingData'],
    ['renderCuttingTable', 'planRenderCuttingTable'],
    ['calculateAcik', 'planCalculateAcik'],
    ['calculateAsorti', 'planCalculateAsorti'],
    ['addCutToGlobalAndRender', 'planAddCutToGlobalAndRender'],
    ['_addCutToGlobalAndRender', '_planAddCutToGlobalAndRender'],
    ['removeCuttingItem', 'planRemoveCuttingItem'],
    ['toggleCutForm', 'planToggleCutForm'],
    ['saveAcikRow', 'planSaveAcikRow'],
    ['saveAsortiRow', 'planSaveAsortiRow'],
    ['/ProcessTracking/UpdateCuttingProcess', '/Planning/UpdatePlannedCutting'],
    ['lblRemainingQty', 'planLblRemainingQty'],
    ['tblCuttingAcik', 'planTblCuttingAcik'],
    ['cuttingBodyAcik', 'planCuttingBodyAcik'],
    ['cuttingEntryAcik', 'planCuttingEntryAcik'],
    ['tblCuttingAsorti', 'planTblCuttingAsorti'],
    ['cuttingBodyAsorti', 'planCuttingBodyAsorti'],
    ['cuttingEntryAsorti', 'planCuttingEntryAsorti'],
    ['txtAcikMultiplier', 'planTxtAcikMultiplier'],
    ['txtAcikSize_', 'planTxtAcikSize_'],
    ['txtAcikTotal', 'planTxtAcikTotal'],
    ['txtAcikMarkerLength', 'planTxtAcikMarkerLength'],
    ['txtAcikUnitMeterage', 'planTxtAcikUnitMeterage'],
    ['txtAcikDate', 'planTxtAcikDate'],
    ['txtAsortiMultiplier', 'planTxtAsortiMultiplier'],
    ['txtAsortiSize_', 'planTxtAsortiSize_'],
    ['txtAsortiTotal', 'planTxtAsortiTotal'],
    ['txtAsortiMarkerLength', 'planTxtAsortiMarkerLength'],
    ['txtAsortiUnitMeterage', 'planTxtAsortiUnitMeterage'],
    ['txtAsortiDate', 'planTxtAsortiDate'],
    ['acikTotalCut', 'planAcikTotalCut'],
    ['asortiTotalCut', 'planAsortiTotalCut'],
    ['grandTotalCut', 'planGrandTotalCut'],
    ['divRemainingCut', 'planDivRemainingCut'],
    ['btnEkleKesim', 'planBtnEkleKesim']
];

for (let [search, replace] of replacements) {
    combinedJS = combinedJS.split(search).join(replace);
}

// ensure endpoint JSON property is correct
combinedJS = combinedJS.replace(/CuttingProcessJson:/g, 'PlannedCuttingJson:');

// Insert combinedJS into Plan.cshtml script section
let planPath = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml';
let planHtml = fs.readFileSync(planPath, 'utf8');

const docReadyIndex = planHtml.indexOf('$(document).ready(function() {');
if (docReadyIndex !== -1) {
    let part1 = planHtml.substring(0, docReadyIndex);
    let part2 = planHtml.substring(docReadyIndex);
    planHtml = part1 + combinedJS + '\n    ' + part2;
    
    // Also add planRenderCuttingTable() into doc ready
    planHtml = planHtml.replace('$(document).ready(function() {', '$(document).ready(function() {\n        planRenderCuttingTable();');
    
    fs.writeFileSync(planPath, planHtml, 'utf8');
} else {
    console.log("Could not find document.ready");
}
