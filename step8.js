const fs = require('fs');

let content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

// The Kesim block starts with: <div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">
// Wait, step6.js did this:
// kesimHtml = kesimHtml.substring(0, outerDivStart) + '<div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">' + kesimHtml.substring(outerDivEnd);
// So it starts with that.
// Let's find it.
const kesimStart = content.indexOf('<div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">');

if (kesimStart === -1) {
    console.log("Could not find kesimStart");
    process.exit(1);
}

// Find the end of kesimHtml. It's inserted right after `</form>` of tab-uretim.
// Actually, step6.js did: content = content.substring(0, insertIndex) + '\n' + kesimHtml + '\n' + content.substring(insertIndex);
// where insertIndex = formEndIndex + 7;
// So kesimHtml is between `</form>\n` and `\n        </div>\n    }\n    else`.
// Let's find the `</div>\n    }\n    else`
const endSearchStr = '        </div>\r\n    }\r\n    else';
let endMarker = content.indexOf(endSearchStr, kesimStart);
if (endMarker === -1) {
    // try LF only
    endMarker = content.indexOf('        </div>\n    }\n    else', kesimStart);
}
if (endMarker === -1) {
    // try just looking for `    else`
    endMarker = content.indexOf('    else\r\n    {', kesimStart);
    if (endMarker !== -1) {
        endMarker = content.lastIndexOf('</div>', endMarker);
    }
}

if (endMarker === -1) {
    console.log("Could not find endMarker");
    process.exit(1);
}

// The Kesim HTML goes from kesimStart to endMarker
let kesimHtml = content.substring(kesimStart, endMarker).trim();

// Remove kesimHtml from content
content = content.substring(0, kesimStart) + content.substring(endMarker);

// Modify kesimHtml
kesimHtml = kesimHtml.replace('Kesim Süreci (Günlük Takip)', 'Kesim Planlaması');
kesimHtml = kesimHtml.replace(/<th>Tarih<\/th>/g, '<th>Planlanan Tarih</th>');
kesimHtml = kesimHtml.replace(/<th style="(.*?)">Tarih<\/th>/g, '<th style="$1">Planlanan Tarih</th>');

// We want to insert it BEFORE:
// <div style="display: flex; justify-content: space-between; margin-top: 20px; align-items: center;">
// <label style="display: flex; align-items: center; gap: 8px; font-weight: 600; color: #0f172a;">
// <input type="checkbox" name="IsProductionCompleted"
const isProdCompletedStr = '<input type="checkbox" name="IsProductionCompleted"';
const isProdCompletedIndex = content.indexOf(isProdCompletedStr);

if (isProdCompletedIndex === -1) {
    console.log("Could not find IsProductionCompleted");
    process.exit(1);
}

// Find the parent div of IsProductionCompleted
const parentDivIndex = content.lastIndexOf('<div style="display: flex; justify-content: space-between; margin-top: 20px; align-items: center;">', isProdCompletedIndex);

if (parentDivIndex === -1) {
    console.log("Could not find parentDivIndex");
    process.exit(1);
}

content = content.substring(0, parentDivIndex) + kesimHtml + '\n<br/>\n' + content.substring(parentDivIndex);

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', content, 'utf8');
console.log("Done");
