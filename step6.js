const fs = require('fs');

let content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const kesimStartComment = '<!-- KESİM SÜRECİ TAB -->';
const kesimStartIndex = content.indexOf(kesimStartComment);
const kesimEndIndex = content.indexOf('</div>\n\n        \n<script>', kesimStartIndex) + 6; // Include the closing </div>

if (kesimStartIndex === -1 || kesimEndIndex === 5) {
    console.log("Could not find Kesim block");
    process.exit(1);
}

let kesimHtml = content.substring(kesimStartIndex, kesimEndIndex);

// Remove from old location
content = content.substring(0, kesimStartIndex) + content.substring(kesimEndIndex);

// Strip the first line of KesimHtml that has the activeTab logic
// <div id="tab-kesim" class="tab-content @(activeTab == "kesim" ? "active" : "")">
// We just replace the outer div
const outerDivStart = kesimHtml.indexOf('<div id="tab-kesim"');
const outerDivEnd = kesimHtml.indexOf('>', outerDivStart) + 1;
kesimHtml = kesimHtml.substring(0, outerDivStart) + '<div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">' + kesimHtml.substring(outerDivEnd);

// Find where to insert (end of tab-uretim form)
const formEndIndex = content.indexOf('</form>', content.indexOf('<div id="tab-uretim"'));
if (formEndIndex === -1) {
    console.log("Could not find form end");
    process.exit(1);
}

const insertIndex = formEndIndex + 7; // after </form>

content = content.substring(0, insertIndex) + '\n' + kesimHtml + '\n' + content.substring(insertIndex);

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', content, 'utf8');
console.log("Moved successfully.");
