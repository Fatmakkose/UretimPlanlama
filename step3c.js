const fs = require('fs');

let planPath = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml';
let planHtml = fs.readFileSync(planPath, 'utf8');

let kesimHtml = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/extract_kesim_replaced.txt', 'utf8');

// Insert kesimHtml before <script>
const scriptIndex = planHtml.indexOf('<script>');
if (scriptIndex !== -1) {
    let part1 = planHtml.substring(0, scriptIndex);
    let part2 = planHtml.substring(scriptIndex);
    planHtml = part1 + kesimHtml + '\n' + part2;
    fs.writeFileSync(planPath, planHtml, 'utf8');
} else {
    console.log("Could not find <script> tag.");
}
