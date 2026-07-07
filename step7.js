const fs = require('fs');

let content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const kesimStartComment = '<!-- KESİM SÜRECİ TAB -->';
const kesimStartIndex = content.indexOf(kesimStartComment);

const isProdCompletedMatch = content.match(/<div[^>]*?>[\s\S]*?<input[^>]*?name="IsProductionCompleted"[\s\S]*?<\/div>/);

if (!isProdCompletedMatch || kesimStartIndex === -1) {
    console.log("Could not find required blocks.");
    process.exit(1);
}

const targetIndex = isProdCompletedMatch.index;

const myFormEndIndex = content.indexOf('</form>', targetIndex);

const elseBlockIndex = content.indexOf('}\r\n    else', myFormEndIndex);
const closingBraceIndex = content.lastIndexOf('}', elseBlockIndex - 1);
const kesimEndIndex = content.lastIndexOf('</div>', closingBraceIndex) + 6;

let kesimHtml = content.substring(kesimStartIndex, kesimEndIndex);

// Remove KesimHtml from current location
content = content.substring(0, kesimStartIndex) + content.substring(kesimEndIndex);

// Modify kesimHtml
kesimHtml = kesimHtml.replace('Kesim Süreci (Günlük Takip)', 'Kesim Planlaması');
kesimHtml = kesimHtml.replace(/<th>Tarih<\/th>/g, '<th>Planlanan Tarih</th>');
kesimHtml = kesimHtml.replace(/<th style="(.*?)">Tarih<\/th>/g, '<th style="$1">Planlanan Tarih</th>');

// Find the target insertion point again because the string has changed (actually it's before kesimStart so index didn't change)
// Wait, targetIndex is BEFORE kesimStart?
// kesimStartIndex is around line 1000, targetIndex is around 983.
// Since targetIndex < kesimStartIndex, removing kesim doesn't change targetIndex.

content = content.substring(0, targetIndex) + kesimHtml + '\n<br/>\n' + content.substring(targetIndex);

fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', content, 'utf8');
console.log("Done");
