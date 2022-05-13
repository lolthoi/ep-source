using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IMailService
    {
        //Task SendMailAsync();
        Task SendMailSMTP(MailRequest mailRequest);
        void SendMailInformEvaluation(int inputYear, int inputQuarter);
    }
    public class MailService : IMailService
    {
        private readonly MailSetting _mailSetting;
        private readonly IAuthenLogicService<MailService> _logicService;
        private readonly IConfiguration _configuration;
        public MailService(IOptions<MailSetting> mailSetting, IAuthenLogicService<MailService> logicService, IConfiguration configuration)
        {
            _mailSetting = mailSetting.Value;
            _logicService = logicService;
            _configuration = configuration;
        }

        #region HUY LE backup
        public void SendMailInformEvaluation(int inputYear, int inputQuarter)
        {
            DateTime now = DateTime.UtcNow;
            var firstDayOfQuarter = GetFirstDayOfQuarter(inputYear, inputQuarter);
            var lastDayOfQuarter = GetLastDayOfQuarter(inputYear, inputQuarter);
            var data = _logicService.Start()
               .ThenAuthorize(Roles.ADMINISTRATOR)
               .ThenValidate(x =>
               {
                   return null;
               })
               .ThenImplement(x =>
               {
                   var project = _logicService.Cache.Projects.GetValues().Where(project => !project.DeletedDate.HasValue)
                               .Where(project => (project.Status == (int)ProjectStatusEnum.OPEN && project.StartDate < lastDayOfQuarter) || (project.Status == (int)ProjectStatusEnum.CLOSED && project.StartDate < lastDayOfQuarter && project.EndDate > firstDayOfQuarter))
                               .ToList();
                   var adminMailBoxs = _logicService.Cache.Users.GetValues()
                                        .Where(x => (x.RoleId == (int)Roles.ADMINISTRATOR) && !x.DeletedDate.HasValue)
                                        .Select(t => t.Email).ToList();

                   MailContent mailContent = new()
                   {
                       ToEmail = "",
                       Subject = "IMPORTANT: Employee Performance Evaluation",
                       Body = "<p>Dear {0} project member,</p>" +
                       "<p><i>Gửi thành viên dự án {0}, </i></p>" +
                       "<p>&nbsp;</p>" +
                       "<p>On behalf of the Management Board, I would like to notify you about the {1} quarter employee evaluation of {2}.</p>" +
                       "<p><i>Thay mặt Ban điều hành, tôi xin thông báo với các bạn về việc đánh giá nhân viên quý {3} năm {2}.</i></p>" +
                       "<p>&nbsp;</p></br>" +
                       "<p>The Management Board requires all of you to complete your self-evaluation in the link below.</p>" +
                       "<p><i>Ban điều hành yêu cầu tất cả các bạn hoàn thành phiếu tự đánh giá bản thân ở link dưới đây.</i></p>" +
                       _configuration.GetSection("ClientUrl").Value + "/evaluation-personal" +
                        "<p>&nbsp;</p></br>" +
                        "<p>For project managers, following the link below to evaluate your team members.</p>" +
                        "<p><i>Đối với các trưởng dự án, vui lòng truy cập link dưới đây để đánh giá thành viên nhóm mình.</i></p>" +
                        _configuration.GetSection("ClientUrl").Value + "/evaluation-leader",
                       Attachments = null,
                   };

                   project.ForEach(t =>
                   {
                       //Init
                       var allUser = _logicService.Cache.Projects.GetUsers(t.Id).Where(x => !x.DeletedDate.HasValue);
                       var leader = allUser.Where(x => x.ProjectRoleId == (int)ProjectRoles.PM);
                       var normal = allUser.Except(leader).Select(x => x.Email);
                       //

                       List<string> mailsToCC = new List<string>();
                       mailsToCC.AddRange(adminMailBoxs);
                       if (leader.FirstOrDefault() != null)
                       {
                           mailsToCC.AddRange(leader.Select(x => x.Email));
                       }

                       var content = string.Format(mailContent.Body, t.Name, ConvertOrderQuarter(inputQuarter), inputYear, inputQuarter);

                       MailRequest mailRequest = new MailRequest()
                       {
                           Attachments = null,
                           Body = content,
                           Subject = mailContent.Subject,
                           ToCCEmails = mailsToCC.ToList(),
                           ToEmails = normal.ToList()
                       };

                       SendMailSMTP(mailRequest);
                   });
                   return true;
               });
        }

        public async Task SendMailSMTP(MailRequest mailRequest)
        {
            //Valid

            if (mailRequest.ToEmails.Count == 0)
            {
                return;
            }

            //

            if (_mailSetting.DisableCertificate)
                NEVER_EAT_POISON_Disable_CertificateValidation();

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSetting.Mail);

            if (mailRequest.ToCCEmails != null)
            {
                if (mailRequest.ToCCEmails.Count > 0)
                {
                    var lstCCEmailToSend = mailRequest.ToCCEmails.Select(t => MailboxAddress.Parse(t));
                    email.Cc.AddRange(lstCCEmailToSend);
                }
            }

            var lstEmailsToSend = mailRequest.ToEmails.Select(t => MailboxAddress.Parse(t));
            email.To.AddRange(lstEmailsToSend);

            string agency = Regex.Replace(_mailSetting.SenderName, @"[^a-zA-Z\s]+", "");
            email.Subject = mailRequest.Subject;//string.IsNullOrEmpty(agency) ? mailRequest.Subject : string.Format("{0}: {1}", agency, mailRequest.Subject);
            var builder = new BodyBuilder();
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSetting.Host, _mailSetting.Port, ConvertToSecureSocketOptions(_mailSetting.DisableSecureSocket));

            if (_mailSetting.DisableXOATH2)
            {
                smtp.AuthenticationMechanisms.Remove("XOAUTH2");
            }

            if (!_mailSetting.UseDefaultCredentials)
            {
                smtp.Authenticate(_mailSetting.Mail, _mailSetting.Password);
            }

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };
        }
        private SecureSocketOptions ConvertToSecureSocketOptions(string option)
        {
            switch (option)
            {
                case "None":
                    return SecureSocketOptions.None;

                case "Auto":
                    return SecureSocketOptions.Auto;

                case "SslOnConnect":
                    return SecureSocketOptions.SslOnConnect;

                case "StartTls":
                    return SecureSocketOptions.StartTls;

                case "StartTlsWhenAvailable":
                    return SecureSocketOptions.StartTlsWhenAvailable;

                default:
                    return SecureSocketOptions.Auto;
            }
        }
        #endregion

        #region private count 
        public string ConvertOrderQuarter(int number)
        {
            if (number == 1)
                return "first";
            if (number == 2)
                return "second";
            if (number == 3)
                return "third";
            else
                return "fourth";
        }
        private DateTime AddQuarters(DateTime originalDate, int quarters)
        {
            return originalDate.AddMonths(quarters * 3);
        }
        private DateTime GetFirstDayOfQuarter(int year, int quarter)
        {
            return AddQuarters(new DateTime(year, 1, 1), quarter - 1);
        }
        private DateTime GetLastDayOfQuarter(int year, int quarter)
        {
            return AddQuarters(new DateTime(year, 1, 1), quarter).AddDays(-1);
        }
        #endregion

    }

    public class MailRequest
    {
        public List<string> ToEmails { get; set; }
        public List<string> ToCCEmails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}