
using System.Net;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using FinalProject.src.Application.Interfaces;
namespace FinalProject.src.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _emailFrom;
        private readonly string _emailPassword;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _emailFrom = configuration["EmailSettings:EmailFrom"];
            _emailPassword = configuration["EmailSettings:EmailPassword"];
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Peron", _emailFrom));
                email.To.Add(new MailboxAddress("", toEmail));
                email.Subject = subject;
                email.Body = new TextPart("plain") { Text = message };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailFrom, _emailPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }

    //public class EmailService : IEmailService
    //{
    //    private readonly IConfiguration _configuration;

    //    public EmailService(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }

    //    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    //    {
    //        try
    //        {
    //            var smtpServer = _configuration["EmailSettings:SmtpServer"];
    //            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
    //            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
    //            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
    //            var fromEmail = _configuration["EmailSettings:FromEmail"];

    //            using (var client = new SmtpClient(smtpServer, smtpPort))
    //            {
    //                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
    //                client.EnableSsl = true;

    //                var mailMessage = new MailMessage
    //                {
    //                    From = new MailAddress(fromEmail),
    //                    Subject = subject,
    //                    Body = body,
    //                    IsBodyHtml = true
    //                };
    //                mailMessage.To.Add(toEmail);

    //                await client.SendMailAsync(mailMessage);
    //                return true;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Email sending failed: {ex.Message}");
    //            return false;
    //        }
    //    }
    //}
}
