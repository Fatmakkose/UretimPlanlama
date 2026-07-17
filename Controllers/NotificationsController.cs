using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using UretimPlanlama.Data;

namespace UretimPlanlama.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUnread()
        {
            var notificationsList = _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(15)
                .ToList();

            var orderCodes = notificationsList.Select(n => n.OrderCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var orderIdMap = _context.Orders.Where(o => orderCodes.Contains(o.OrderCode))
                                            .Select(o => new { o.OrderCode, o.Id })
                                            .ToList()
                                            .GroupBy(x => x.OrderCode)
                                            .ToDictionary(g => g.Key, g => g.First().Id);

            var notifications = notificationsList.Select(n => new {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type,
                orderId = !string.IsNullOrEmpty(n.OrderCode) && orderIdMap.ContainsKey(n.OrderCode) ? (int?)orderIdMap[n.OrderCode] : null,
                createdAt = n.CreatedAt.ToString("HH:mm - dd.MM.yyyy"),
                isRead = n.IsRead
            }).ToList();

            var unreadCount = _context.Notifications.Count(n => !n.IsRead);
            
            return Json(new { notifications, unreadCount });
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _context.Notifications.Find(id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var unread = _context.Notifications.Where(n => !n.IsRead).ToList();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
