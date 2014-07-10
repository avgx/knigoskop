using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Knigoskop.Site.Common.Helpers;
using Knigoskop.Site.Controllers;
using Knigoskop.Site.Localization;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Mail;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Services
{
    public class MailService : BasicDataService, IMailService
    {
        private MailController GetSender()
        {
            var result = new MailController();
            return result;
        }

        public void ComplainEmail(Guid itemId)
        {
            return;
            var model = new ComplainEmailModel
            {

            };
            var result = GetSender().ComposeEmail("Complain", model);
            result.Mail.To.Add(MailController.MailSettings.AdminEmail);
            result.Mail.Subject = Text.ComplainEmailSubject;
            result.SendAsync();
        }

        public void NewComment(Guid itemId)
        {
            return;
            var model = new NewCommentEmailModel()
            {

            };
            var result = GetSender().ComposeEmail("NewComment", model);
            //result.Mail.To.Add(MailSender.MailSettings.AdminEmail);
            //result.Mail.Subject = Text.ComplainEmailSubject;
            //result.SendAsync();
        }

        public void NewIncome(Guid itemId)
        {
            return;
            var model = new NewIncomeEmailModel()
            {

            };
            var result = GetSender().ComposeEmail("NewIncome", model);
            result.Mail.To.Add(MailController.MailSettings.AdminEmail);
            result.Mail.Subject = Text.NewIncomeEmailSubject;
            result.SendAsync();
        }

        public void IncomeApproved(Guid itemId)
        {
            return;
            var model = new IncomeApprovedEmailModel()
            {

            };
            //var mail = GetSender().ComposeEmail(model);
            //mail.SendAsync();
        }

        public void DeliverBookToEmail(string toEmail, string name, ConversionResultModel source)
        {
            var result = GetSender();
            var message = new MailMessage
            {
                From = new MailAddress(MailController.MailSettings.NoReplyEmail),
                Subject = string.Format(Text.BookSent, name)
            };
            message.To.Add(new MailAddress(toEmail));
            message.Attachments.Add(new Attachment(source.FullFileName)
            {
                Name = source.DownloadFileName
            });
            result.MailSender.Send(message);
        }
    }
}