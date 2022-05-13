using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Common
{
    public static class MailUtils
    {
        public static async Task SendMailAsync(MailSetting mailSetting, List<MimeMessage> models)
        {
            try
            {
                if (models.Count == 0) return;

                Init(mailSetting);
                using (var _smtp = new SmtpClient())
                {
                    try
                    {
                        var builder = new BodyBuilder();
                        _smtp.Connect(mailSetting.Host, mailSetting.Port, ConvertToSecureSocketOptions(mailSetting.DisableSecureSocket));

                        if (mailSetting.DisableXOATH2)
                            _smtp.AuthenticationMechanisms.Remove("XOAUTH2");

                        if (!mailSetting.UseDefaultCredentials)
                            _smtp.Authenticate(mailSetting.Mail, mailSetting.Password);

                        models.ForEach(x =>
                        {
                            x.Sender = x.Sender == null ? x.Sender = MailboxAddress.Parse(mailSetting.SenderName) : x.Sender;
                            _smtp.Send(x);
                        });

                        _smtp.Disconnect(true);
                        await Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<MailboxAddress> ConvertToMailBox(this List<string> receivers)
        {
            if (receivers == null || receivers.Count == 0)
                return new List<MailboxAddress>();

            var data = receivers.Select(x => MailboxAddress.Parse(x)).ToList();
            return data;
        }

        private static void Init(MailSetting _mailSetting)
        {
            if (_mailSetting.DisableCertificate)
            {
                DISABLE_CERTIFICATE_VALIDATION();
            }
        }
        private static void DISABLE_CERTIFICATE_VALIDATION()
        {
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
        private static SecureSocketOptions ConvertToSecureSocketOptions(string option)
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
    }
}
