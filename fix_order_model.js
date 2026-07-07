const fs = require('fs');

const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Models/Order.cs';
let content = fs.readFileSync(path, 'utf8');

const targetStr = `        public int Quantity { get; set; }`;
const newProp = `        public int Quantity { get; set; }

        [NotMapped]
        public int CalculatedQuantity
        {
            get
            {
                int qty = Quantity;
                if (!string.IsNullOrEmpty(PurchasingMaterialsJson))
                {
                    try
                    {
                        var purData = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(PurchasingMaterialsJson);
                        if (purData != null)
                        {
                            string c = string.IsNullOrEmpty(Color) ? "GENEL" : Color;
                            
                            // 1. İstenen Kumaş -> Çıkacak Adet
                            string target = purData.ContainsKey($"pur_color_{c}_beden_cikacak") ? purData[$"pur_color_{c}_beden_cikacak"] : null;
                            if (string.IsNullOrEmpty(target) && purData.ContainsKey("pur_color_GENEL_beden_cikacak"))
                                target = purData["pur_color_GENEL_beden_cikacak"];
                            
                            if (!string.IsNullOrEmpty(target) && int.TryParse(target.Replace(".", ""), out int val) && val > 0)
                                return val;

                            // 2. İhtiyaç Olan Kumaş -> Sipariş Adeti
                            target = purData.ContainsKey($"pur_color_{c}_siparis_adeti") ? purData[$"pur_color_{c}_siparis_adeti"] : null;
                            if (string.IsNullOrEmpty(target) && purData.ContainsKey("pur_color_GENEL_siparis_adeti"))
                                target = purData["pur_color_GENEL_siparis_adeti"];
                            
                            if (!string.IsNullOrEmpty(target) && int.TryParse(target.Replace(".", ""), out int val2) && val2 > 0)
                                return val2;
                        }
                    }
                    catch { }
                }
                return qty;
            }
        }`;

content = content.replace(targetStr, newProp);

fs.writeFileSync(path, content, 'utf8');
