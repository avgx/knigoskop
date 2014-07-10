using System;
using System.Collections.Generic;
using System.Linq;
using Knigoskop.DataModel;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;

namespace Knigoskop.Site.Services
{
    public class BasicDataService : BasicService
    {
        protected Entities GetContext()
        {
            var result = new Entities();
            return result;
        }


        public IQueryable<SerieItemModel> GetSearchSuggestionSeriesQuery(IQueryable<Serie> query)
        {
            IQueryable<SerieItemModel> series = query.Select(br => new SerieItemModel
            {
                Id = br.SerieId,
                Name = br.Name,
                Authors = br.BookInSeries.SelectMany(bi=>bi.Book.Authors).Select(a => new AuthorItemModel() { Id = a.AuthorId, Name = a.Name }),
                Rating = new RatingModel { Value = (double?)br.Ratings.Average(r => r.Value) ?? 0 },
                HasImage = false
            });
            return series;
        }


        public IQueryable<BookItemModel> GetSearchSuggestionBooksQuery(IQueryable<Book> query)
        {
            IQueryable<BookItemModel> books = query.Select(br => new BookItemModel
                {
                    Id = br.BookId,
                    Name = br.Name,
                    Authors = br.Authors.Select(a => new AuthorItemModel() { Id = a.AuthorId, Name = a.Name }),
                    Rating = new RatingModel { Value = (double?)br.Ratings.Average(r => r.Value) ?? 0 },
                    HasImage = br.Cover != null
                });
            return books;
        }


        public IQueryable<AuthorItemModel> GetSearchSuggestionAuthorsQuery(IQueryable<Author> query)
        {
            IQueryable<AuthorItemModel> authors = query.Select(ar => new AuthorItemModel
                {
                    Id = ar.AuthorId,
                    Name = ar.Name,
                    BornYear = ar.BirthDate != null ? ar.BirthDate.Value.Year : (int?)null,
                    DeathYear = ar.DeathDate != null ? ar.DeathDate.Value.Year : (int?)null,
                    Rating = new RatingModel { Value = (double?)ar.Ratings.Average(r => r.Value) ?? 0 },
                    HasImage = ar.Photo != null,
                    BooksCount = ar.Books.Count()
                });
            return authors;
        }
    }
}