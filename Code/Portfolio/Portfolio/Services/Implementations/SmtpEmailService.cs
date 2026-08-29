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

            var senderEmail = _config["Smtp:SenderEmail"]
                ?? throw new InvalidOperationException("Smtp:SenderEmail not configured");
            var senderPassword = _config["Smtp:SenderPassword"]
                ?? throw new InvalidOperationException("Smtp:SenderPassword not configured");
            var senderName = _config["Smtp:SenderName"] ?? "Portfolio Site";
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true");

            using var client = new SmtpClient(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl,
            };

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Reset your password",
                Body = $"Click the link below to reset your password. This link expires in 1 hour.\n\n{resetLink}",
                IsBodyHtml = false,
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}