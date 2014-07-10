using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Knigoskop.Site.Models;

namespace Knigoskop.Site.Services.Interface
{
    public interface IMailService
    {
        void ComplainEmail(Guid itemId);
        void NewComment(Guid itemId);
        void NewIncome(Guid itemId);
        void IncomeApproved(Guid itemId);

        void DeliverBookToEmail(string toEmail, string name, ConversionResultModel source);
    }
}
