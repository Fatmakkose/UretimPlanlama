const fs = require('fs');

let content = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', 'utf8');

const kesimStart = content.indexOf('<!-- KESİM SÜRECİ TAB -->');
const scriptStart = content.indexOf('<script>', kesimStart);

if (kesimStart !== -1 && scriptStart !== -1) {
    let kesimInnerHtml = content.substring(kesimStart, scriptStart);
    
    // Remove it from current location
    content = content.substring(0, kesimStart) + content.substring(scriptStart);
    
    // Replace the outer tab div with a simple div
    kesimInnerHtml = kesimInnerHtml.replace(
        /<div id="tab-kesim" class="tab-content.*?>/g, 
        '<div style="margin-top: 30px; border-top: 1px solid #e2e8f0; padding-top: 20px;">'
    );
    
    // Find the end of production form
    const uretimStart = content.indexOf('<div id="tab-uretim"');
    const formEnd = content.indexOf('</form>', uretimStart) + 7;
    
    // Insert it
    content = content.substring(0, formEnd) + "\n\n        " + kesimInnerHtml + "\n" + content.substring(formEnd);
    
    fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Planning/Plan.cshtml', content, 'utf8');
    console.log("Success");
} else {
    console.log("Kesim start or script start not found");
}
