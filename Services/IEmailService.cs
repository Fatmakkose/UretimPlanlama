using System.Threading.Tasks;

namespace UretimPlanlama.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}
