const fs = require('fs');

const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/ProcessTracking/Track.cshtml';
let c = fs.readFileSync(path, 'utf8');

// 1. Add purData and plannedCutQty parsing to the top block
const topBlockOld = `    var prodData = new System.Collections.Generic.Dictionary<string, string>();
    if (!string.IsNullOrEmpty(Model.ProductionJson))
    {
        try {
            prodData = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(Model.ProductionJson);
        } catch {}
    }
    string GetProdVal(string key) => prodData != null && prodData.ContainsKey(key) ? prodData[key] : "";`;

const topBlockNew = `    var prodData = new System.Collections.Generic.Dictionary<string, string>();
    if (!string.IsNullOrEmpty(Model.ProductionJson))
    {
        try {
            prodData = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(Model.ProductionJson);
        } catch {}
    }
    string GetProdVal(string key) => prodData != null && prodData.ContainsKey(key) ? prodData[key] : "";

    var purData = new System.Collections.Generic.Dictionary<string, string>();
    if (!string.IsNullOrEmpty(Model.PurchasingDetails))
    {
        try {
            purData = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(Model.PurchasingDetails);
        } catch {}
    }
    string GetPurVal(string key) => purData != null && purData.ContainsKey(key) ? purData[key] : "";

    string colorKey = string.IsNullOrEmpty(Model.Color) ? "GENEL" : Model.Color;
    string plannedCutQtyStr = GetPurVal($"pur_color_{colorKey}_beden_cikacak");
    if (string.IsNullOrEmpty(plannedCutQtyStr)) {
        plannedCutQtyStr = GetPurVal("pur_color_GENEL_beden_cikacak");
    }
    int plannedCutQty = Model.Quantity;
    if (!string.IsNullOrEmpty(plannedCutQtyStr) && int.TryParse(plannedCutQtyStr.Replace(".", ""), out int parsedVal) && parsedVal > 0)
    {
        plannedCutQty = parsedVal;
    }`;

c = c.replace(topBlockOld, topBlockNew);

// 2. Replace Model.Quantity with plannedCutQty for the Goal
const goalOld1 = `<i class="fa-solid fa-bullseye" style="margin-right: 8px;"></i>Genel Toplam Hedef: @Model.Quantity Adet`;
const goalNew1 = `<i class="fa-solid fa-bullseye" style="margin-right: 8px;"></i>Genel Toplam Hedef: @plannedCutQty Adet`;

const goalOld2 = `Kalan İhtiyaç: <strong id="lblRemainingQty">@Model.Quantity</strong> Adet`;
const goalNew2 = `Kalan İhtiyaç: <strong id="lblRemainingQty">@plannedCutQty</strong> Adet`;

c = c.replace(goalOld1, goalNew1);
c = c.replace(goalOld2, goalNew2);

// JS logic update
// Find where the remaining qty is calculated in JS: 
// var targetQty = @Model.Quantity;
const jsGoalOld = `var targetQty = @Model.Quantity;`;
const jsGoalNew = `var targetQty = @plannedCutQty;`;
c = c.replace(jsGoalOld, jsGoalNew);

// 3. Fix input widths for size boxes
// min-width: 50px -> min-width: 70px
c = c.replace(/min-width:\s*50px/g, 'min-width: 75px; padding: 4px;');
c = c.replace(/min-width:\s*60px/g, 'min-width: 80px; padding: 4px;');

fs.writeFileSync(path, c, 'utf8');
