using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Services
{
    public class OpdsDataService : DataService, IOpdsDataService
    {
        public OpdsDataService(ISearchService searchService)
            : base(searchService)
        {

        }

    }
}