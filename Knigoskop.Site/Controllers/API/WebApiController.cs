using System;
using System.Web.Http;
using Knigoskop.Site.Models.Api;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Controllers.Api
{
    public class WebApiController : ApiController
    {
        private readonly IWebApiDataService _dataService;
        public WebApiController(IWebApiDataService apiDataService)
        {
            _dataService = apiDataService;
        }

        [HttpGet]
        [HttpPost]
        public SearchSuggestionsApiResponse GetSearchSuggestions(string searchText, int expectedRowCount = 50)
        {
            return new SearchSuggestionsApiResponse
                {
                    Items = _dataService.GetSearchSuggestions(searchText, expectedRowCount)
                };
        }

        [HttpGet]
        [HttpPost]
        public SearchSuggestionsApiResponse SearchBooks(string searchText, int expectedRowCount = 50)
        {
            return new SearchSuggestionsApiResponse
            {
                Items = _dataService.GetSearchSuggestions(searchText, expectedRowCount, ItemTypeEnum.Book)
            };
        }

        [HttpGet]
        [HttpPost]
        public SearchSuggestionsApiResponse SearchAuthors(string searchText, int expectedRowCount = 50)
        {
            return new SearchSuggestionsApiResponse
            {
                Items = _dataService.GetSearchSuggestions(searchText, expectedRowCount, ItemTypeEnum.Author)
            };
        }

        [HttpGet]
        [HttpPost]
        public SearchSuggestionsApiResponse SearchSeries(string searchText, int expectedRowCount = 50)
        {
            return new SearchSuggestionsApiResponse
            {
                Items = _dataService.GetSearchSuggestions(searchText, expectedRowCount, ItemTypeEnum.Serie)
            };
        }

        [HttpGet]
        [HttpPost]
        public GenresApiResponse GetGenres(Guid? rootGenreId)
        {
            return new GenresApiResponse()
            {
                Items = _dataService.GetGenresByLevel(rootGenreId)
            };
        }

    }
}