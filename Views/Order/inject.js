const fs = require('fs');
const file = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Views/Order/Index.cshtml';
let text = fs.readFileSync(file, 'utf8');

const targetStr = '$("#editOrderId").val(order.id);';
const replaceStr = targetStr + `
                                var $submitBtn = $("#editOrderForm button[type='submit']");
                                if($submitBtn.length === 0) { $submitBtn = $(".btn-planner").first(); }
                                $submitBtn.html('<i class="fa-solid fa-save"></i> Siparişi Güncelle')
                                         .addClass('btn-planner')
                                         .css({'background-color': '', 'border-color': ''});
`;

if(text.includes(targetStr)) {
    text = text.replace(targetStr, replaceStr);
    fs.writeFileSync(file, text, 'utf8');
    console.log('Replaced successfully');
} else {
    console.log('Target string not found');
}
