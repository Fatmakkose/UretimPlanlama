const fs = require('fs');

let kesimHtml = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/extract_kesim.txt', 'utf8');

// Replace tab container
kesimHtml = kesimHtml.replace('<div class="tab-pane" id="cutting">', '<div id="tab-kesim" class="tab-content @(activeTab == "kesim" ? "active" : "")">');

// Remove the switchTab button at the end
kesimHtml = kesimHtml.replace(/<div class="text-end mt-3 mb-2">[\s\S]*?<\/button>\s*<\/div>/, '');

// JS replacements
const replacements = [
    ['Model.CuttingProcessJson', 'Model.PlannedCuttingJson'],
    ['cuttingRecords', 'planCuttingRecords'],
    ['saveCuttingData', 'planSaveCuttingData'],
    ['renderCuttingRecords', 'planRenderCuttingRecords'],
    ['calculateAcik', 'planCalculateAcik'],
    ['calculateAsorti', 'planCalculateAsorti'],
    ['addCuttingRecord', 'planAddCuttingRecord'],
    ['deleteCuttingRecord', 'planDeleteCuttingRecord'],
    ['toggleCutForm', 'planToggleCutForm'],
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
    ['txtAcikLength', 'planTxtAcikLength'],
    ['txtAcikMeter', 'planTxtAcikMeter'],
    ['txtAcikDate', 'planTxtAcikDate'],
    ['txtAsortiMultiplier', 'planTxtAsortiMultiplier'],
    ['txtAsortiSize_', 'planTxtAsortiSize_'],
    ['txtAsortiTotal', 'planTxtAsortiTotal'],
    ['txtAsortiLength', 'planTxtAsortiLength'],
    ['txtAsortiMeter', 'planTxtAsortiMeter'],
    ['txtAsortiDate', 'planTxtAsortiDate']
];

for (let [search, replace] of replacements) {
    kesimHtml = kesimHtml.split(search).join(replace);
}

// Ensure the endpoint JSON payload key is updated from CuttingProcessJson to PlannedCuttingJson
kesimHtml = kesimHtml.replace(/CuttingProcessJson: JSON.stringify\(planCuttingRecords\)/, 'PlannedCuttingJson: JSON.stringify(planCuttingRecords)');
kesimHtml = kesimHtml.replace(/CuttingProcessJson:/g, 'PlannedCuttingJson:');

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/extract_kesim_replaced.txt', kesimHtml, 'utf8');
