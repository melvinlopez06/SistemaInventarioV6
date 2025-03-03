using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace SistemaInventarioV6.Utilidades
{
    public class EmailSender : IEmailSender
    {
        public string MailTrapSecret { get; set; }

        public EmailSender(IConfiguration _config)
        {
            MailTrapSecret = _config.GetValue<string>("MailTrap:SecretKey");
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new RestClient("https://sandbox.api.mailtrap.io/api/send/3032329");
            var request = new RestRequest();
            request.AddHeader("Authorization", MailTrapSecret);
            request.AddHeader("Content-Type", "application/json");

            request.AddParameter("application/json", "{\"from\":{\"email\":\"hello@example.com\",\"name\":\"Mailtrap Test\"},\"to\":[{\"email\":\"" + email + "\"}],\"subject\":\"" + subject + "\",\"text\":\"" + htmlMessage + "\",\"html\":\"" + htmlMessage + "\",\"category\":\"Integration Test\"}", ParameterType.RequestBody);
            return client.PostAsync(request);
        }
    }
}
