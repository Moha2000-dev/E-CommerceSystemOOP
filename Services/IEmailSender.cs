namespace E_CommerceSystem.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

}
