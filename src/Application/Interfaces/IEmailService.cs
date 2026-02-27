namespace FinalProject.src.Application.Interfaces
{
    public interface IEmailService
    {
        //Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
    }

}
