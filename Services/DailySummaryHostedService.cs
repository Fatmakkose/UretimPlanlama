using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UretimPlanlama.Data;

namespace UretimPlanlama.Services
{
    public class DailySummaryHostedService : BackgroundService
    {
        private readonly ILogger<DailySummaryHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DailySummaryHostedService(ILogger<DailySummaryHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Her gün 18:00'ı hesapla
                var now = DateTime.Now;
                var next18 = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);

                if (now > next18)
                {
                    // Eğer şu an 18:00'i geçtiyse, yarın 18:00'e kur
                    next18 = next18.AddDays(1);
                }

                var delay = next18 - now;
                _logger.LogInformation($"Günlük özet servisi beklemede. Sonraki çalışma zamanı: {next18}");

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await SendDailySummaryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Günlük özet gönderilirken bir hata oluştu.");
                    }
                }
            }
        }

        private async Task SendDailySummaryAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var today = DateTime.Today;

            // Bugün eklenen bildirimler (tamamlanan aşamalar vb.)
            var todaysNotifications = await dbContext.Notifications
                .Where(n => n.CreatedAt >= today)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (!todaysNotifications.Any())
            {
                _logger.LogInformation("Bugün için herhangi bir hareket bulunamadığından özet mail atılmadı.");
                return;
            }

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append(@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #0f766e;'>Günlük Üretim Özeti</h2>
                    <p>Bugün (" + today.ToString("dd.MM.yyyy") + @") sistemde gerçekleşen süreç bildirimleri aşağıdadır:</p>
                    <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                        <thead>
                            <tr style='background-color: #f1f5f9; text-align: left;'>
                                <th style='padding: 10px; border: 1px solid #cbd5e1;'>Saat</th>
                                <th style='padding: 10px; border: 1px solid #cbd5e1;'>Sipariş No</th>
                                <th style='padding: 10px; border: 1px solid #cbd5e1;'>Kategori</th>
                                <th style='padding: 10px; border: 1px solid #cbd5e1;'>Başlık</th>
                                <th style='padding: 10px; border: 1px solid #cbd5e1;'>Detay</th>
                            </tr>
                        </thead>
                        <tbody>");

            foreach (var notif in todaysNotifications)
            {
                htmlBuilder.Append($@"
                            <tr>
                                <td style='padding: 10px; border: 1px solid #cbd5e1;'>{notif.CreatedAt:HH:mm}</td>
                                <td style='padding: 10px; border: 1px solid #cbd5e1;'><strong>{notif.OrderCode}</strong></td>
                                <td style='padding: 10px; border: 1px solid #cbd5e1;'>{notif.Type}</td>
                                <td style='padding: 10px; border: 1px solid #cbd5e1;'>{notif.Title}</td>
                                <td style='padding: 10px; border: 1px solid #cbd5e1;'>{notif.Message}</td>
                            </tr>");
            }

            htmlBuilder.Append(@"
                        </tbody>
                    </table>
                    <p style='margin-top: 30px; font-size: 0.9em; color: #64748b;'>Bu e-posta Üretim Planlama sistemi tarafından otomatik olarak gönderilmiştir.</p>
                </div>");

            // Sistemdeki tüm geçerli kullanıcılara mail at
            var users = await dbContext.Users.Where(u => !string.IsNullOrEmpty(u.Email) && u.ReceiveEmailNotifications).Select(u => u.Email).ToListAsync();
            
            if (users.Any())
            {
                string targetEmails = string.Join(",", users);
                await emailService.SendEmailAsync(targetEmails, $"Günlük Üretim Özeti - {today:dd.MM.yyyy}", htmlBuilder.ToString());
                _logger.LogInformation($"Günlük özet {users.Count} kullanıcıya başarıyla gönderildi.");
            }
        }
    }
}
