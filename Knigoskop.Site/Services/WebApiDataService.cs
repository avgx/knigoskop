using System;
using System.Collections.Generic;
using System.Linq;
using Knigoskop.DataModel;
using Knigoskop.Site.Common.Helpers;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Api;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;

namespace Knigoskop.Site.Services
{
    public class WebApiDataService : DataService, IWebApiDataService
    {
        public WebApiDataService(ISearchService searchService)
            : base(searchService)
        {

        }

        public IList<SearchSuggestionApiModel> GetSearchSuggestions(string searchText, int expectedRowsCount, ItemTypeEnum? itemType = null)
        {
            using (Entities context = GetContext())
            {
                List<LuceneResultModel> results = SearchService.GetSearchResults(searchText, itemType)
                    .OrderByDescending(t => t.Score)
                    .Take(expectedRowsCount).ToList();
                
                List<Guid> mix;
                
                if (itemType == ItemTypeEnum.Serie)
                {
                    mix = results.Where(r => r.Type == ItemTypeEnum.Serie).Select(r => r.Id).ToList();

                    IQueryable<Serie> serieQuery = context.Series.Where(ar => mix.Contains(ar.SerieId));

                    List<SerieItemModel> series = GetSearchSuggestionSeriesQuery(serieQuery).ToList();
                    List<SearchSuggestionApiModel> seriesResult = series.Select(a => new SearchSuggestionApiModel
                    {
                        Id = a.Id.ToString(),
                        Type = ItemTypeEnum.Author,
                        Title = a.Name,
                        Rating = a.Rating.Value,
                        HasImage = a.HasImage,
                        Score = results.Where(r => r.Id == a.Id).Select(r => r.Score).FirstOrDefault()
                    }).ToList();
                    return seriesResult.OrderByDescending(a => a.Score).ToList();
                }


                mix = results.Where(r => r.Type == ItemTypeEnum.Book).Select(r => r.Id).ToList();

                IQueryable<Book> booksQuery =
                    context.Books.Where(br => mix.Contains(br.BookId));

                List<BookItemModel> books = GetSearchSuggestionBooksQuery(booksQuery).ToList();
                List<SearchSuggestionApiModel> booksResult = books.Select(b => new SearchSuggestionApiModel
                    {
                        Id = b.Id.ToString(),
                        Type = ItemTypeEnum.Book,
                        Title = b.Name,
                        Authors = b.Authors.Select(a => a.Name),
                        Rating = b.Rating.Value,
                        HasImage = b.HasImage,
                        Score = results.Where(r => r.Id == b.Id).Select(r => r.Score).FirstOrDefault()
                    }).ToList();


                mix = results.Where(r => r.Type == ItemTypeEnum.Author).Select(r => r.Id).ToList();

                IQueryable<Author> authorsQuery = context.Authors.Where(ar => mix.Contains(ar.AuthorId));

                List<AuthorItemModel> authors = GetSearchSuggestionAuthorsQuery(authorsQuery).ToList();
                List<SearchSuggestionApiModel> authorsResult = authors.Select(a => new SearchSuggestionApiModel
                    {
                        Id = a.Id.ToString(),
                        Type = ItemTypeEnum.Author,
                        Title = a.Name,
                        BornYear = a.BornYear,
                        DeathYear = a.DeathYear,
                        Rating = a.Rating.Value,
                        HasImage = a.HasImage,
                        Score = results.Where(r => r.Id == a.Id).Select(r => r.Score).FirstOrDefault()
                    }).ToList();

                IEnumerable<SearchSuggestionApiModel> result = authorsResult
                                        .Union(booksResult);

                result = result.OrderByDescending(a => a.Score);

                return result.ToList();                
            }
        }

        public IList<GenreApiModel> GetGenresByLevel(Guid? rootGenreId)
        {
            using (Entities context = GetContext())
            {
                GenreLevelEnum level = rootGenreId == null ? GenreLevelEnum.Root : GenreLevelEnum.Children;
                return context.Genres
                              .Where(q => q.ParentId == rootGenreId)
                              .OrderBy(q => q.Name)
                              .Select(q => new GenreApiModel
                                  {
                                      GenreId = q.GenreId.ToString(),
                                      Name = q.Name,
                                      Level =  level
                                  }).ToList();
            }
        }
    }
}