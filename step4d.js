const fs = require('fs');
let content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

// 1. Remove the Kesim Tab Button
const tabButtonRegex = /<div class="plan-tab @\(activeTab == "kesim" \? "active" : ""\)" onclick="switchTab\('kesim', this\)">[\s\S]*?<\/div>\s*<\/div>/;
// Wait, the `</div>\s*</div>` might accidentally remove the closing div of plan-tabs.
// Let's just find the exact tab button.
const tabBtnStart = content.indexOf('<div class="plan-tab @(activeTab == "kesim"');
if (tabBtnStart !== -1) {
    // Find the next </div> that corresponds to this tab button. It has a few inner divs, so it's safer to just regex up to "BEKLİYOR</div>\n            </div>"
    const tabBtnEndStr = '</div>\n            </div>';
    const tabBtnEndIndex = content.indexOf('BEKLİYOR</div>', tabBtnStart);
    if(tabBtnEndIndex !== -1) {
        const fullEnd = content.indexOf('</div>', content.indexOf('</div>', tabBtnEndIndex) + 6) + 6;
        const btnContent = content.substring(tabBtnStart, fullEnd);
        content = content.replace(btnContent, '');
    }
}

// 2. Find Kesim Content
const kesimStartComment = content.indexOf('<!-- KESİM SÜRECİ TAB -->');
const kesimDivStart = content.indexOf('<div id="tab-kesim"', kesimStartComment);
// The end of kesim content is right before document.addEventListener
const endKesim = content.indexOf("document.addEventListener('DOMContentLoaded'");
// Actually, it's better to just extract everything between <div id="tab-kesim"...> and the script tag.
const scriptStart = content.indexOf('<script>', kesimDivStart);

if (kesimDivStart !== -1 && scriptStart !== -1) {
    let kesimInnerHtml = content.substring(kesimDivStart, scriptStart);
    
    // Remove <div id="tab-kesim"...> and the last </div>
    kesimInnerHtml = kesimInnerHtml.substring(kesimInnerHtml.indexOf('>') + 1);
    const lastDivIndex = kesimInnerHtml.lastIndexOf('</div>');
    if (lastDivIndex !== -1) {
        kesimInnerHtml = kesimInnerHtml.substring(0, lastDivIndex);
    }
    
    // Remove the Kesim content from its original place
    content = content.substring(0, kesimStartComment) + content.substring(scriptStart);

    // Insert kesimInnerHtml inside tab-uretim
    const uretimStart = content.indexOf('<div id="tab-uretim"');
    const formEnd = content.indexOf('</form>', uretimStart) + 7;
    
    // We insert right after </form>
    content = content.substring(0, formEnd) + "\n\n        <div style=\"margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;\">\n" + kesimInnerHtml + "\n        </div>\n" + content.substring(formEnd);
}

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', content, 'utf8');
console.log("Transformation complete.");
