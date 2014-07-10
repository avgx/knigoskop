using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ActionMailer.Net.Mvc;

namespace Knigoskop.Site.Common.Helpers
{
    public static class EmailResultExtender
    {
        public static async Task SendAsync(this EmailResult e)
        {
            await Task.Factory.StartNew(e.Deliver);
        }
        public static void Send(this EmailResult e)
        {
            e.Deliver();
        }
    }
}