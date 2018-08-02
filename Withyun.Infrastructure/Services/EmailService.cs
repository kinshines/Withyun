using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Withyun.Infrastructure.Utility;

namespace Withyun.Infrastructure.Services
{
    public class EmailService
    {
        private readonly IHostingEnvironment _env;
        public EmailService(IHostingEnvironment env)
        {
            _env = env;
        }
        public void SendEmail(EmailType emailType, string emailAddress, string userName, string callbackUrl, string code)
        {
            try
            {
                string template = GetTemplate(emailType);
                template = template.Replace("$UserName$", userName);
                template = template.Replace("$CallbackUrl$", callbackUrl);
                template = template.Replace("$Code$", code);
                MailMessage mail = new MailMessage();
                Encoding encoding = Encoding.GetEncoding("utf-8");
                mail.From = new MailAddress("noreply@withyun.com", "Withyun", encoding);
                mail.To.Add(new MailAddress(emailAddress, userName, encoding));
                if (emailType == EmailType.Confirmation)
                {
                    mail.Subject = "Withyun请求确认您的注册邮箱";
                }
                else if (emailType == EmailType.ResetPassword)
                {
                    mail.Subject = "重置密码信息";
                }
                else if (emailType == EmailType.ResetEmail)
                {
                    mail.Subject = "请输入 " + code + " 完成新邮箱验证";
                }
                mail.IsBodyHtml = true;
                mail.Body = template;
                mail.BodyEncoding = encoding;

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.mxhichina.com",
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential("noreply@withyun.com", "password"),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "发送邮件异常");
            }
        }

        private string GetTemplate(EmailType emailType)
        {
            string template = "";
            string path = _env.ContentRootFileProvider.GetFileInfo("/App_Data/EmailTemplate/" + emailType + ".html").PhysicalPath;
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    template = sr.ReadToEnd();
                }
            }
            return template;
        }

    }

    public enum EmailType
    {
        Confirmation,
        ResetPassword,
        ResetEmail
    }
}
