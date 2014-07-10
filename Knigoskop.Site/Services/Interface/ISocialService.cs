using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knigoskop.DataModel;

namespace Knigoskop.Site.Services.Interface
{
    public interface ISocialService
    {
        void RateBook(Guid userId, string bookUrl, short ratingValue, string reviewUrl = null, string actionId = null);
        void SetBookStat(Guid userId, string bookUrl, UserStatStateEnum state, string actionId = null);
    }
}
