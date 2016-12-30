using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace Contact2015.Helpers
{
    public class MailHelper
    {


        private static string _fromEmailAddress = WebConfigurationManager.AppSettings["DEFAULTEMAIL"];

        internal static void SendMail(string toEmailAddress, string mailSubject, string mailMessage)
        {
            SendMail(_fromEmailAddress, toEmailAddress, mailSubject, mailMessage);
        }


        internal static void SendMail(string fromAddress, string toAddress, string mailSubject, string mailMessage)
        {
            string retVal = string.Empty;
            MailMessage mail = new MailMessage(fromAddress, toAddress, mailSubject, mailMessage);
            mail.IsBodyHtml = true;

            string smtpServer = WebConfigurationManager.AppSettings["SMTPSERVER"];
            string smtpUsername = WebConfigurationManager.AppSettings["SMTPUSERNAME"];
            string smtpPassword = WebConfigurationManager.AppSettings["SMTPPASSWORD"];
            string smtpPortValue = WebConfigurationManager.AppSettings["SMTPPORT"];
            int smtpPort = int.Parse(smtpPortValue);

            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

            smtp.Send(mail);
        }

        internal static void SendMail(IList<string> emailsList, string toTeacherEmail, string mailSubject, string emailMessage, int fromEmailIndicator = 0)
        {
            if (fromEmailIndicator == 1)
                _fromEmailAddress = toTeacherEmail;

            MailMessage mail = new MailMessage(_fromEmailAddress, toTeacherEmail);
            string bccEmailList = string.Empty;

            foreach (var bccAddress in emailsList)
            {
                mail.Bcc.Add(new MailAddress(bccAddress));
            }
            mail.IsBodyHtml = true;

            string smtpServer = WebConfigurationManager.AppSettings["SMTPSERVER"];
            string smtpUsername = WebConfigurationManager.AppSettings["SMTPUSERNAME"];
            string smtpPassword = WebConfigurationManager.AppSettings["SMTPPASSWORD"];
            string smtpPortValue = WebConfigurationManager.AppSettings["SMTPPORT"];
            int smtpPort = int.Parse(smtpPortValue);

            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

            smtp.Send(mail);
        }


        internal static void SendMail(IDictionary<string, string> emailData)
        {
            string smtpServer = WebConfigurationManager.AppSettings["SMTPSERVER"];
            string smtpUsername = WebConfigurationManager.AppSettings["SMTPUSERNAME"];
            string smtpPassword = WebConfigurationManager.AppSettings["SMTPPASSWORD"];
            string smtpPortValue = WebConfigurationManager.AppSettings["SMTPPORT"];
            int smtpPort = int.Parse(smtpPortValue);

            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

            using (var message = new System.Net.Mail.MailMessage())
            {
                message.IsBodyHtml = true;
                message.To.Add(emailData["To"]);
                message.Subject = emailData["Subject"];
                message.From = new System.Net.Mail.MailAddress(emailData["From"], emailData["FromDescription"]);
                message.Body = emailData["Body"];

                smtp.Send(message);
            }
        }



    }
}