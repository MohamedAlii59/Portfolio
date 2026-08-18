namespace Portfolio.Services.Implementations
{
    using Portfolio.Services.Interfaces;
    using System.Net;
    using System.Net.Mail;
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var frontendUrl = _config["Frontend:BaseUrl"];
            var resetLink = $"{frontendUrl}/admin/reset-password?token={resetToken}";

            using var client = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(_config["Smtp:User"], _config["Smtp:Password"]),
                EnableSsl = true,
            };

            var message = new MailMessage
            {
                From = new MailAddress(_config["Smtp:FromAddress"] ?? "no-reply@example.com", "Portfolio Site"),
                Subject = "Reset your password",
                Body = $"Click the link below to reset your password. This link expires in 1 hour.\n\n{resetLink}",
                IsBodyHtml = false,
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
