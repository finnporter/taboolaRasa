using TaboolaRasa.Configurations;
using TaboolaRasa.Data;
using TaboolaRasa.Data.Models;
using TaboolaRasa.Models.Enums;
using Microsoft.Extensions.Configuration;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaboolaRasa.Email
{
    public interface IEmailService
    {
        Task SendEmail(CommunicationReason communicationReason, ExpandoObject model, string emailAddress, string recipientName, string relatedId = null);
        Task Send(CommunicationReason communicationReason, ExpandoObject model, int recipientUserId, bool sendEmail, string relatedId = null);
    }

    public class EmailService : IEmailService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        public EmailService(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        private async Task SendMail(string toEmailAddress, string toName, string fromEmailAddress, string fromName,
            string subject, string body, string replyTo, string category)
        {
            try
            {
                var email = new MailMessage
                {
                    From = new MailAddress(fromEmailAddress, fromName),
                    ReplyTo = new MailAddress(replyTo ?? fromEmailAddress, fromName),
                    Body = body,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = true,
                    Subject = subject,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                };
                if (!string.IsNullOrEmpty(SharedConfig.Instance.EmailOverrideAddress))
                {
                    toEmailAddress = SharedConfig.Instance.EmailOverrideAddress;
                }
                email.To.Add(new MailAddress(toEmailAddress, toName));
                using (var _smtpClient = new SmtpClient()
                {
                    Host = _configuration.GetValue<string>("SMTP:host"),
                    Port = _configuration.GetValue<int>("SMTP:port"),
                })
                {
                    switch (SharedConfig.Instance.BuildConfiguration)
                    {
                        case BuildConfiguration.Snapshot:
                            email.Subject = "[SNAPSHOT-IGNORE] " + email.Subject;
                            break;
                        case BuildConfiguration.Staging:
                            email.Subject = "[STAGING-IGNORE] " + email.Subject;
                            break;
                        case BuildConfiguration.Debug:
                            email.Subject = "[DEBUG-IGNORE] " + email.Subject;
                            break;
                    }
                    _smtpClient.Send(email);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<string> BuildEmailBodyTemplate(string baseTemplatePath, string communicationType)
        {
            var sb = new StringBuilder();
            var baseTemplate = System.IO.File.ReadAllText(baseTemplatePath + "Email-Base.cshtml");
            sb.Append(baseTemplate);
            var body = System.IO.File.ReadAllText(baseTemplatePath + communicationType + "-Email-Body.cshtml");            
            body = body.Replace("<h1>",
                "<h1 style=\"Margin-top: 0;font-weight: 400;letter-spacing: -0.02em;font-family: sans-serif;color: #3b3e42;font-size: 40px;line-height: 48px;Margin-bottom: 20px\">");
            body = body.Replace("<p>",
                "<p style=\"Margin-top: 0;text-rendering: optimizeLegibility;font-family: sans-serif;color: #60666d;Margin-bottom: 24px;font-size: 15px;line-height: 24px\">");
            body = body.Replace("<table>",
                "<table style=\"border-collapse: collapse;border-spacing: 0;Margin-left: auto;Margin-right: auto\">");
            body = body.Replace("<tr>",
                "<tr style=\"text-rendering: optimizeLegibility;font-family: sans-serif;color: #60666d;font-size: 15px;line-height: 24px\"");            
            sb.Replace("<div id=\"body\"></div>", body);            
            return sb.ToString();
        }

        private async Task<string> BuildEmailSubjectTemplate(string baseTemplatePath, string communicationType)
        {
            var sb = new StringBuilder();
            sb.Append(System.IO.File.ReadAllText(baseTemplatePath + communicationType + "-Email-Subject.cshtml"));
            return sb.ToString();
        }

        public async Task SendEmail(CommunicationReason communicationReason, ExpandoObject model, string emailAddress, string recipientName, string relatedId = null)
        {
            try
            {
                var baseTemplatePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Templates\\";

                var bodyTemplate = await BuildEmailBodyTemplate(baseTemplatePath, communicationReason.ToString());
                var subjectTemplate = await BuildEmailSubjectTemplate(baseTemplatePath, communicationReason.ToString());

                IDictionary<string, object> myUnderlyingObject = model;
                var body = await ParseTemplateWithModel(bodyTemplate, "Body-" + communicationReason.ToString(), myUnderlyingObject);
                var subject = await ParseTemplateWithModel(subjectTemplate, "Subject-" + communicationReason.ToString(), myUnderlyingObject);

                string fromEmailAddress = SharedConfig.Instance.FromEmailAddress;
                string fromName = SharedConfig.Instance.FromEmailName;
                string replyToEmailAddress = fromEmailAddress;
                await SendMail(emailAddress, recipientName, fromEmailAddress, fromName, subject, body, replyToEmailAddress, communicationReason.ToString());
                _db.EmailLogs.Add(new EmailLog()
                {
                    EmailAddress = emailAddress,
                    Reason = communicationReason,
                    RelatedId = relatedId,
                    SentOn = DateTime.UtcNow,
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Something went wrong.
            }
        }

        public async Task Send(CommunicationReason communicationReason, ExpandoObject model, int recipientUserId, bool sendEmail, string relatedId = null)
        {
            try
            {
                var recipientUser = _db.Users.Find(recipientUserId);
                if (recipientUser != null)
                {
                    if (sendEmail)
                    {
                        await SendEmail(communicationReason, model, recipientUser.Email, recipientUser.FirstName, relatedId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Something went wrong.
            }
        }

        private async Task<string> ParseTemplateWithModel(string template, string key, object model)
        {
            var data = Engine.Razor.RunCompile(template, key, null, model, null);
            return data;
        }
    }
}
