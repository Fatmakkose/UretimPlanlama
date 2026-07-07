const fs = require('fs');
const text = fs.readFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml', 'utf8');
const occurrences = [...text.matchAll(/<div id="editOrderModal"/g)].map(m => m.index);
console.log('Occurrences of editOrderModal: ' + occurrences.length);
if (occurrences.length > 1) {
    const startIdx = occurrences[1];
    const endStr = '<!-- Edit Model Detail Modal -->';
    const endIdx = text.indexOf(endStr, startIdx);
    if (endIdx > -1) {
        const replacement = `                    </div>
                </div>

                <div style="margin-top: 24px; padding-top: 20px; border-top: 1px solid var(--border-color); display: flex; justify-content: flex-end; gap: 12px;">
                    <button type="button" id="btnEditCancelModal" class="btn-secondary" style="padding: 10px 20px;">İptal</button>
                    <button type="submit" class="btn-planner" style="padding: 10px 24px;"><i class="fa-solid fa-save"></i> Siparişi Güncelle</button>
                </div>
            </form>

        `;
        const newText = text.substring(0, startIdx) + replacement + text.substring(endIdx);
        fs.writeFileSync('c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml', newText, 'utf8');
        console.log('Fixed file via Node.js!');
    }
}
