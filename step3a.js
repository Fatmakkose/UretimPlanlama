const fs = require('fs');

const pathPlan = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml';
let planContent = fs.readFileSync(pathPlan, 'utf8');

const searchRegex = /(<div class="plan-tab \@\(activeTab == "uretim" \? "active" : ""\)" onclick="switchTab\('uretim', this\)">[\s\S]*?<\/div>\s*<\/div>)/;

const match = planContent.match(searchRegex);
if (match) {
    let replacedText = match[1].replace(/<\/div>\s*$/, `    <div class="plan-tab @(activeTab == "kesim" ? "active" : "")" onclick="switchTab('kesim', this)">
                <i class="fa-solid fa-scissors"></i>
                <div class="plan-tab-title">4. KESİM PLANLAMASI</div>
                @if (!string.IsNullOrEmpty(Model.PlannedCuttingJson)) {
                    <div class="plan-tab-status status-complete">%100 TAMAM</div>
                } else {
                    <div class="plan-tab-status status-notstarted">BEKLİYOR</div>
                }
            </div>
        </div>`);
    planContent = planContent.replace(searchRegex, replacedText);
    fs.writeFileSync(pathPlan, planContent, 'utf8');
} else {
    console.log("Could not find the end of the tabs.");
}
