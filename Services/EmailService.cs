using SendGrid;
using SendGrid.Helpers.Mail;

namespace LmsApi.Services
{
    public class EmailService (IConfiguration config)
    {
        private readonly IConfiguration _config = config;

        public async Task SendEmail(string toEmail, string subject, string message)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(_config["SendGrid:FromEmail"], _config["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = $"<p>{message}</p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to send email: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email", ex);
            }

        }
    }
}
