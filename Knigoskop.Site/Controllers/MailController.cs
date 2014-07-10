using System.Configuration;
using System.Web.UI.WebControls;
using ActionMailer.Net.Mvc;
using Knigoskop.Site.Code.Configuration;

namespace Knigoskop.Site.Controllers
{
    public class MailController : MailerBase
    {
        public static readonly MailSettings MailSettings = (MailSettings)ConfigurationManager.GetSection("mailConfiguration");
        public MailController()
        {
            From = MailSettings.NoReplyEmail;
        }

        //
        // GET: /Mail/

        public EmailResult ComposeEmail(string viewName, object model)
        {
            return Email(viewName, model);
        }
	}
}