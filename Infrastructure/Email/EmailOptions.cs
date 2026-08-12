using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Infrastructure.Email
{
    public class EmailOptions : IEmailConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public int ExpirationMinutes { get; set; } = 3;
    }
}

//if we use brevo the stmpPasword will expire after one year so we need to change it every year and update the appsettings.json file