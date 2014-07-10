using System;
using System.Collections.Generic;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Api;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Services.Interface
{
    public interface IWebApiDataService 
    {
        IList<SearchSuggestionApiModel> GetSearchSuggestions(string searchText, int expectedRowsCount, ItemTypeEnum? itemType = null);
        IList<GenreApiModel> GetGenresByLevel(Guid? rootGenreId);
    }
}