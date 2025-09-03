namespace E_CommerceSystem.Services
{
    using System.Net;
    using System.Net.Mail;

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var s = _cfg.GetSection("Smtp");
            using var client = new SmtpClient(s["Host"], int.Parse(s["Port"]!))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(s["User"], s["Password"])
            };
            using var msg = new MailMessage
            {
                From = new MailAddress(s["From"]!, s["FromName"]),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(to);
            await client.SendMailAsync(msg);
        }
    }

}
