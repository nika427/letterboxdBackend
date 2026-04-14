using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService
{
    public async Task SendVerificationEmailAsync(string email, string code)
    {
        var message = new MailMessage();
        message.From = new MailAddress("n.kalantarovi@gmail.com");
        message.To.Add(email);
        message.Subject = "Email Verification";
        message.Body = $"Your verification code is: {code}";

        var smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential("n.kalantarovi@gmail.com", "iybi jeyy vkue kbnx");
        smtp.EnableSsl = true;

        await smtp.SendMailAsync(message);
    }
}