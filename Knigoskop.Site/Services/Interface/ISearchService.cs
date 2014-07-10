using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Services.Interface
{
    public interface ISearchService
    {
        IEnumerable<LuceneResultModel> GetSearchResults(string searchQuery, ItemTypeEnum? itemType = null);
        void CreateIndex();
    }
}
