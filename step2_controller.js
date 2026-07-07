const fs = require('fs');
const path = 'c:/Users/Fatma/Downloads/UretimPlanlama-master/UretimPlanlama-master/Controllers/PlanningController.cs';
let content = fs.readFileSync(path, 'utf8');

const newMethod = `
        [HttpPost]
        public IActionResult UpdatePlannedCutting([FromBody] PlannedCuttingRequest request)
        {
            if (!User.HasPermission("Write")) return Json(new { success = false, message = "Yetkisiz" });

            var order = _context.Orders.Find(request.Id);
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });

            order.PlannedCuttingJson = request.PlannedCuttingJson;
            _context.SaveChanges();

            return Json(new { success = true });
        }
`;

const newModel = `
    public class PlannedCuttingRequest
    {
        public int Id { get; set; }
        public string PlannedCuttingJson { get; set; } = string.Empty;
    }
}
`;

// Insert newMethod before the last closing brace of the controller class
// Insert newModel before the namespace closing brace

// The namespace ends with `}`. The controller class ends with `}` before that.
// We can just replace the last two `}` with the newMethod, newModel and `}`.

const searchStr = `    }
}`;

const replaceStr = `    ${newMethod}
    }
${newModel}`;

if (content.includes(searchStr)) {
    content = content.replace(searchStr, replaceStr);
    fs.writeFileSync(path, content, 'utf8');
} else {
    // try finding just the end
    let lastIndex = content.lastIndexOf('}');
    let secondLastIndex = content.lastIndexOf('}', lastIndex - 1);
    
    let part1 = content.substring(0, secondLastIndex);
    let part2 = content.substring(secondLastIndex);
    
    let combined = part1 + newMethod + part2.replace('}', '} ' + newModel);
    fs.writeFileSync(path, combined, 'utf8');
}
