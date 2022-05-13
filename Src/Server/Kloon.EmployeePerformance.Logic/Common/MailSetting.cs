using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Kloon.EmployeePerformance.Logic.Common
{
    public class MailSetting
    {
        public string SenderName { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string DisableSecureSocket { get; set; }
        public bool DisableXOATH2 { get; set; }
        public bool DisableCertificate { get; set; }
        public bool UseDefaultCredentials { get; set; }
    }
    public class MailContent
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
