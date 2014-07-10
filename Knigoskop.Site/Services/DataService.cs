using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using Knigoskop.DataModel;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Knigoskop.Site.Services.Interface;
using OAuth2.Models;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Knigoskop.Site.Services
{
    public class DataService : BasicDataService, IDataService
    {
        private const int ConstPageRowsCount = 32;

        public DataService(ISearchService searchService)
        {
            SearchService = searchService;
        }

        protected ISearchService SearchService { get; private set; }

        public byte[] GetUserAvatar(Guid userId)
        {
            using (Entities context = GetContext())
            {
                return context.Users.Where(u => u.UserId == userId).Select(u => u.Avatar).FirstOrDefault();
            }
        }

        public long UpdateUserAvatar(Guid userId, byte[] avatar)
        {
            using (Entities context = GetContext())
            {
                User user = context.Users.First(u => u.UserId == userId);
                user.Avatar = avatar;
                user.AvatarUpdated = DateTime.UtcNow;
                context.SaveChanges();
                return user.AvatarUpdated.Ticks;
            }
        }

        public IdentityInfoModel GetUserProfile(Guid userId)
        {
            using (Entities context = GetContext())
            {
                var result = context.Users
                              .Where(u => u.UserId == userId)
                              .Select(u => new IdentityInfoModel
                                  {
                                      Id = u.UserId,
                                      FirstName = u.FirstName,
                                      LastName = u.LastName,
                                      Email = u.Email,
                                      AvatarUpdated = u.AvatarUpdated,
                                      HasImage = u.Avatar != null,
                                      Created = u.Created,
                                      CommentsCount = u.Comments.Count(),
                                      ReviewsCount = u.Reviews.Count(),
                                      BooksCount = u.Books.Count(),
                                      RatingsCount = u.Ratings.Count(),
                                      LinkedProviders = u.UserAuthProfiles.Select(o => o.ProviderName),
                                      IsSubscriber = u.IsSubscriber
                                  })
                              .ToList()
                              .First();
                return result;

            }
        }


        public byte[] GetAuthorPortraitImage(Guid authorId)
        {
            using (Entities context = GetContext())
            {
                return context.Authors.Where(a => a.AuthorId == authorId).Select(a => a.Photo).FirstOrDefault();
            }
        }

        public byte[] GetBookCoverImage(Guid bookId)
        {
            using (Entities context = GetContext())
            {
                return
                    context.Books.Where(bm => bm.BookId == bookId)
                           .Select(bm => bm.Cover)
                           .FirstOrDefault();
            }
        }

        public void UpdateUserProfile(IdentityInfoModel identity)
        {
            using (Entities context = GetContext())
            {
                User user = context.Users.First(u => u.UserId == identity.Id);
                user.FirstName = identity.FirstName;
                user.LastName = identity.LastName;
                user.Email = identity.Email;
                user.IsSubscriber = identity.IsSubscriber;
                context.SaveChanges();
            }
        }

        private byte[] DownloadRemoteAvatar(string photoUri)
        {
            byte[] avatar = null;
            if (!string.IsNullOrEmpty(photoUri))
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        avatar = webClient.DownloadData(photoUri);
                    }
                    catch
                    {
                    }
                }
            }
            return avatar;
        }

        private UserAuthProfile UserInfoToOAuthProfile(UserInfo userInfo)
        {
            return new UserAuthProfile
            {
                UserAuthProfileId = Guid.NewGuid(),
                Id = userInfo.Id,
                ProviderName = userInfo.ProviderName,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Email = userInfo.Email,
                PhotoUrl = !string.IsNullOrEmpty(userInfo.AvatarUri.Large) ? userInfo.AvatarUri.Large : userInfo.PhotoUri,
                IsAttached = DateTime.UtcNow

            };
        }
        public IdentityInfoModel AuthorizeUser(UserInfo userInfo, Guid? userId = null)
        {
            using (Entities context = GetContext())
            {
                Guid id = context.Users
                                 .Where(
                                     u =>
                                     u.UserAuthProfiles.Any(
                                         o => o.Id.Equals(userInfo.Id) && o.ProviderName.Equals(userInfo.ProviderName)))
                                 .Select(u => u.UserId)
                                 .FirstOrDefault();

                if (id == Guid.Empty)
                {
                    User user = null;
                    if (userId != null)
                        user = context.Users.Include("UserAuthProfiles").FirstOrDefault(o => o.UserId == userId);
                    else if (!string.IsNullOrEmpty(userInfo.Email))
                        user = context.Users.Include("UserAuthProfiles").FirstOrDefault(o => o.Email == userInfo.Email);

                    var avatarUri = !string.IsNullOrEmpty(userInfo.AvatarUri.Large) ? userInfo.AvatarUri.Large : userInfo.PhotoUri;

                    if (user == null)
                    {
                        id = Guid.NewGuid();
                        user = new User
                            {
                                UserId = id,
                                FirstName = userInfo.FirstName,
                                LastName = userInfo.LastName,
                                Avatar = DownloadRemoteAvatar(avatarUri),
                                AvatarUpdated = DateTime.UtcNow,
                                Email = userInfo.Email,
                                IsLocked = false,
                                IsSubscriber = true,
                                Created = DateTime.UtcNow,
                                LastAccessed = DateTime.UtcNow
                            };
                        context.Users.Add(user);
                    }
                    else
                        id = user.UserId;

                    if (userId != null && user.UserAuthProfiles.Any(au => au.ProviderName == userInfo.ProviderName))
                    {
                        user.UserAuthProfiles.Remove(
                            user.UserAuthProfiles.First(au => au.ProviderName == userInfo.ProviderName));
                    }
                    else
                        user.UserAuthProfiles.Add(UserInfoToOAuthProfile(userInfo));
                    context.SaveChanges();
                }
                return GetUserProfile(id);
            }
        }


        public IList<GenreModel> GetGenres(Guid? selectedGenreId)
        {
            using (Entities context = GetContext())
            {
                var dynRoots = context.Genres.Where(
                    g => g.ParentId == null)
                                      .Select(g => new
                                          {
                                              g.GenreId,
                                              g.Name,
                                              g.ParentId,
                                              Children = g.Child.Select(c => new
                                                  {
                                                      c.GenreId,
                                                      c.Name,
                                                      c.ParentId
                                                  }).OrderBy(c => c.Name)
                                          })
                                      .OrderBy(c => c.Name).ToList();

                IEnumerable<GenreModel> roots =
                    dynRoots.Where(
                        g => g.ParentId == null)
                            .Select(g => new GenreModel
                                {
                                    Id = g.GenreId,
                                    Name = g.Name,
                                    Children = g.Children.Select(c => new GenreModel
                                        {
                                            Id = c.GenreId,
                                            Name = c.Name,
                                            IsSelected = c.GenreId == selectedGenreId,
                                        }).OrderBy(c => c.Name),
                                    IsSelected = g.Children.Any(c => c.GenreId == selectedGenreId)
                                })
                            .OrderBy(c => c.Name);
                return roots.ToList();
            }
        }


        public BookModel GetBook(Guid bookId, Guid? userId)
        {
            using (Entities context = GetContext())
            {
                var dayOfWeekOffset = (int)DateTime.UtcNow.DayOfWeek;
                BookModel result = context.Books.Where(be => be.BookId == bookId)
                    .Select(b => new BookModel
                    {
                        Id = b.BookId,
                        HasImage = b.Cover != null,
                        Name = b.Name,
                        Published = b.Published,
                        PagesCount = b.PagesCount,
                        Publisher = b.Publisher,
                        Genres =
                            b.Genres.Select(
                                g => new GenreModel
                                {
                                    Id = g.GenreId,
                                    Name = g.Name
                                }),
                        Description = b.Description,
                        Rating = new RatingModel
                        {
                            Value = (double?)b.Ratings.Average(r => r.Value) ?? 0,
                            Count = b.Ratings.Count()
                        },
                        ISBN = b.ISBN,
                        Authors =
                            b.Authors.Select(a => new AuthorItemModel
                            {
                                Id = a.AuthorId,
                                Name = a.Name
                            }),
                        Translators = b.TranslatorsInBooks.Select(t => new AuthorItemModel
                            {
                                Id = t.Author.AuthorId,
                                Name = t.Author.Name
                            }),
                        Series =
                            b.BookInSeries.Select(bs => bs.Series)
                             .Select(sr => new SerieModel
                                 {
                                     Id = sr.SerieId,
                                     Name = sr.Name
                                 }),
                        Comments =
                            b.Comments.Select(cb => new CommentModel
                                {
                                    Id = cb.CommentId,
                                    Created = cb.Created,
                                    ParentId = cb.ParentId,
                                    Text = cb.Text,
                                    Type = ItemTypeEnum.Book,
                                    UserId = cb.UserId,
                                    UserName = cb.User.FirstName + " " + cb.User.LastName,
                                    UserImageUpdated = cb.User.AvatarUpdated,
                                    IsPublished = cb.IsPublished,
                                    HasChildren = cb.Children.Any()
                                }).OrderBy(cb => cb.Created),
                        IsUserSubscribed = b.Subscriptions.Any(s => s.UserId == userId),
                        Reviews = b.Reviews.Select(r => new ReviewItemModel
                            {
                                Id = r.ReviewId,
                                CreatedBy = new IdentityInfoModel
                                    {
                                        Id = r.UserId,
                                        FirstName = r.User.FirstName,
                                        LastName = r.User.LastName
                                    },
                                Name = r.Title,
                                Description = r.Text,
                                Created = r.Created,
                                ReviewRating = r.Book.Ratings.Where(rr => rr.UserId == r.UserId).Select(rr => rr.Value).FirstOrDefault(),
                                Rating = new RatingModel
                                    {
                                        Value =
                                            (double?)r.Ratings.Average(rr => rr.Value) ?? 0,
                                        Count = r.Ratings.Count
                                    }
                            }),
                        Citations = b.Citations.Select(c => new CitationModel
                        {
                            Id = c.CitationId,
                            BookId = c.BookId,
                            Created = c.Created,
                            UserId = c.UserId,
                            Text = c.Text
                        }),
                        ViewStats = new ViewStatsModel
                            {
                                Today =
                                    b.ViewStats.Count(
                                        s =>
                                        DbFunctions.DiffDays(s.Created, DateTime.UtcNow) ==
                                        0),
                                Week =
                                    b.ViewStats.Count(
                                        s =>
                                        DbFunctions.DiffDays(s.Created, DateTime.UtcNow) <=
                                        dayOfWeekOffset),
                                Month =
                                    b.ViewStats.Count(
                                        s =>
                                        DbFunctions.DiffMonths(s.Created, DateTime.UtcNow) ==
                                        0),
                                Year =
                                    b.ViewStats.Count(
                                        s =>
                                        DbFunctions.DiffYears(s.Created, DateTime.UtcNow) ==
                                        0),
                                Total = b.ViewStats.Count()
                            },
                        UserStats = new UserStatsModel
                            {
                                WantsToRead =
                                    b.UserStats.Count(
                                        u => u.State == UserStatStateEnum.WantsToRead),
                                Read =
                                    b.UserStats.Count(u => u.State == UserStatStateEnum.Read),
                                IsReading =
                                    b.UserStats.Count(
                                        u => u.State == UserStatStateEnum.IsReading),
                                State =
                                    b.UserStats.Where(us => us.UserId == userId)
                                     .Select(us => us.State)
                                     .FirstOrDefault()
                            }
                    })
                                          .FirstOrDefault();
                if (result != null)
                {
                    result.RelatedBooks = context.BookRelations.Where(b => b.BookId == bookId && b.Score >= 0.1F && b.IsRelatedByGenre)
                                                 .Select(rr => new RelatedBookModel
                                                     {
                                                         Id = rr.BookRef.BookId,
                                                         HasImage = rr.BookRef.Cover != null,
                                                         Name = rr.BookRef.Name,
                                                         Authors =
                                                             rr.BookRef.Authors.Select(a => new AuthorItemModel
                                                                 {
                                                                     Id = a.AuthorId,
                                                                     Name = a.Name
                                                                 }),
                                                         Score = rr.Score,
                                                         Rating = new RatingModel
                                                             {
                                                                 Value =
                                                                     rr.BookRef.Ratings.Average(r => (double?)r.Value) ??
                                                                     0
                                                             }
                                                     })
                                                 .OrderByDescending(rr => rr.Score).ToList();
                }
                return result;
            }
        }

        public DownloadInfoModel GetDownloadInfo(Guid bookId, Guid? userId)
        {
            using (Entities context = GetContext())
            {
                var info = context.Books
                    .Where(b => b.BookId == bookId)
                    .Select(b => new DownloadInfoModel
                            {
                                BookId = bookId,
                                HasContent = b.BookContents.Count > 0,
                                UserAccess = b.UserDownloadAccess,
                            })
                     .First();
                info.Devices = context.Devices.Where(d => d.UserId == userId).Select(d => new DeviceModel
                {
                    Id = d.DeviceId,
                    Name = d.Name,
                    Email = d.Email,
                    UserId = d.UserId
                }).ToList();
                info.PrimaryEmail = context.Users.Where(u => u.UserId == userId).Select(u => u.Email).FirstOrDefault();
                return info;
            }
        }

        public AuthorModel GetAuthor(Guid authorId, Guid? userId)
        {
            using (Entities context = GetContext())
            {
                var dayOfWeekOffset = (int)DateTime.UtcNow.DayOfWeek;
                AuthorModel result = context.Authors
                                            .Where(ar => ar.AuthorId == authorId)
                                            .Select(a => new AuthorModel
                                                {
                                                    Id = a.AuthorId,
                                                    Name = a.Name,
                                                    Rating = new RatingModel
                                                        {
                                                            Value = (double?)a.Ratings.Average(r => r.Value) ?? 0,
                                                            Count = a.Ratings.Count()
                                                        },
                                                    Description = a.Biography,
                                                    BornDate = a.BirthDate,
                                                    DeathDate = a.DeathDate,
                                                    HasImage = a.Photo != null,
                                                    OfficialUrl = a.OfficialUrl,
                                                    SourceUrl = a.SourceUrl,
                                                    Comments =
                                                        a.Comments.Select(cb => new CommentModel
                                                            {
                                                                Id = cb.CommentId,
                                                                Created = cb.Created,
                                                                ParentId = cb.ParentId,
                                                                Text = cb.Text,
                                                                Type = ItemTypeEnum.Author,
                                                                UserId = cb.UserId,
                                                                UserName = cb.User.FirstName + " " + cb.User.LastName,
                                                                UserImageUpdated = cb.User.AvatarUpdated,
                                                                IsPublished = cb.IsPublished,
                                                                HasChildren = cb.Children.Any()
                                                            }).OrderBy(cb => cb.Created),
                                                    IsUserSubscribed = a.Subscriptions.Any(s => s.UserId == userId),
                                                    ViewStats = new ViewStatsModel
                                                        {
                                                            Today =
                                                                a.ViewStats.Count(
                                                                    s =>
                                                                    DbFunctions.DiffDays(s.Created, DateTime.UtcNow) ==
                                                                    0),
                                                            Week =
                                                                a.ViewStats.Count(
                                                                    s =>
                                                                    DbFunctions.DiffDays(s.Created, DateTime.UtcNow) <=
                                                                    dayOfWeekOffset),
                                                            Month =
                                                                a.ViewStats.Count(
                                                                    s =>
                                                                    DbFunctions.DiffMonths(s.Created,
                                                                                               DateTime.UtcNow) == 0),
                                                            Year =
                                                                a.ViewStats.Count(
                                                                    s =>
                                                                    DbFunctions.DiffYears(s.Created, DateTime.UtcNow) ==
                                                                    0),
                                                            Total = a.ViewStats.Count()
                                                        }
                                                })
                                            .FirstOrDefault();
                if (result != null)
                {
                    result.Books =
                        context.Authors.Where(a => a.AuthorId == authorId)
                               .SelectMany(b => b.Books)
                               .Select(rr => new BookItemModel
                                   {
                                       Id = rr.BookId,
                                       HasImage = rr.Cover != null,
                                       Rating =
                                           new RatingModel { Value = (double?)rr.Ratings.Average(r => r.Value) ?? 0 },
                                       Name = rr.Name,
                                       Authors =
                                           rr.Authors.Select(aa => new AuthorItemModel
                                               {
                                                   Id = aa.AuthorId,
                                                   Name = aa.Name
                                               }).OrderBy(ss => ss.Name != result.Name)
                                   })
                               .OrderBy(rr => rr.Authors.Count() != 1)
                               .ThenBy(rr => rr.Name).ToList();

                    result.Series =
                        context.Series.Where(
                            sr => sr.BookInSeries.Any(bis => bis.Book.Authors.Any(a => a.AuthorId == result.Id)))
                               .Distinct()
                               .Select(ss => new SerieModel
                                   {
                                       Id = ss.SerieId,
                                       Name = ss.Name
                                   })
                               .OrderBy(sr => sr.Name)
                               .ToList();
                }

                return result;
            }
        }

        public SerieModel GetSerie(Guid serieId, Guid? userId)
        {
            using (Entities context = GetContext())
            {
                var dayOfWeekOffset = (int)DateTime.UtcNow.DayOfWeek;
                SerieModel result = context.Series.Where(s => s.SerieId == serieId)
                                           .Select(s => new SerieModel
                                               {
                                                   Id = s.SerieId,
                                                   Name = s.Name,
                                                   Description = s.Description,
                                                   Rating = new RatingModel
                                                       {
                                                           Value = (double?)s.Ratings.Average(r => r.Value) ?? 0,
                                                           Count = s.Ratings.Count()
                                                       },
                                                   IsUserSubscribed = s.Subscriptions.Any(ss => ss.UserId == userId),
                                                   Comments = s.Comments.Select(cb => new CommentModel
                                                       {
                                                           Id = cb.CommentId,
                                                           Created = cb.Created,
                                                           ParentId = cb.ParentId,
                                                           Text = cb.Text,
                                                           Type = ItemTypeEnum.Serie,
                                                           UserId = cb.UserId,
                                                           UserName = cb.User.FirstName + " " + cb.User.LastName,
                                                           UserImageUpdated = cb.User.AvatarUpdated,
                                                           IsPublished = cb.IsPublished,
                                                           HasChildren = cb.Children.Any()
                                                       }).OrderBy(cb => cb.Created),
                                                   ViewStats = new ViewStatsModel
                                                       {
                                                           Today =
                                                               s.ViewStats.Count(
                                                                   v =>
                                                                   DbFunctions.DiffDays(v.Created, DateTime.UtcNow) ==
                                                                   0),
                                                           Week =
                                                               s.ViewStats.Count(
                                                                   v =>
                                                                   DbFunctions.DiffDays(v.Created, DateTime.UtcNow) <=
                                                                   dayOfWeekOffset),
                                                           Month =
                                                               s.ViewStats.Count(
                                                                   v =>
                                                                   DbFunctions.DiffMonths(v.Created, DateTime.UtcNow) ==
                                                                   0),
                                                           Year =
                                                               s.ViewStats.Count(
                                                                   v =>
                                                                   DbFunctions.DiffYears(v.Created, DateTime.UtcNow) ==
                                                                   0),
                                                           Total = s.ViewStats.Count()
                                                       }
                                               }).FirstOrDefault();

                if (result != null)
                {
                    result.Books =
                        context.BookInSeries.Where(a => a.SerieId == serieId)
                               .Select(rr => new SerieBookModel
                                   {
                                       Id = rr.Book.BookId,
                                       HasImage = rr.Book.Cover != null,
                                       Rating =
                                           new RatingModel
                                               {
                                                   Value = (double?)rr.Book.Ratings.Average(r => r.Value) ?? 0
                                               },
                                       Name = rr.Book.Name,
                                       Position = rr.Position,
                                       Authors = rr.Book.Authors.Select(aa => new AuthorItemModel
                                           {
                                               Id = aa.AuthorId,
                                               Name = aa.Name
                                           }).OrderBy(ss => ss.Name != result.Name)
                                   })
                               .OrderBy(rr => rr.Position)
                               .ThenBy(rr => rr.Name)
                               .ToList();
                }
                return result;
            }
        }

        public ReviewModel GetReview(Guid reviewId, Guid? userId)
        {
            using (Entities context = GetContext())
            {
                var dayOfWeekOffset = (int)DateTime.UtcNow.DayOfWeek;
                ReviewModel result = context.Reviews.Where(s => s.ReviewId == reviewId)
                                            .Select(s => new ReviewModel
                                                {
                                                    Id = s.ReviewId,
                                                    Name = s.Title,
                                                    Description = s.Text,
                                                    Created = s.Created,
                                                    CreatedBy = new IdentityInfoModel
                                                        {
                                                            Id = s.UserId,
                                                            FirstName = s.User.FirstName,
                                                            LastName = s.User.LastName
                                                        },
                                                    Book = new BookModel
                                                        {
                                                            Id = s.Book.BookId,
                                                            Name = s.Book.Name,
                                                            Authors =
                                                                s.Book.Authors.Select(
                                                                    a =>
                                                                    new AuthorItemModel { Id = a.AuthorId, Name = a.Name }),
                                                            HasImage = s.Book.Cover != null,
                                                            Published = s.Book.Published
                                                        },
                                                    ReviewRating =
                                                        s.Book.Ratings.Where(r => r.UserId == s.UserId)
                                                         .Select(r => r.Value)
                                                         .FirstOrDefault(),
                                                    Rating = new RatingModel
                                                        {
                                                            Value = (double?)s.Ratings.Average(r => r.Value) ?? 0,
                                                            Count = s.Ratings.Count()
                                                        },
                                                    IsUserSubscribed = s.Subscriptions.Any(ss => ss.UserId == userId),
                                                    Comments = s.Comments.Select(cb => new CommentModel
                                                        {
                                                            Id = cb.CommentId,
                                                            Created = cb.Created,
                                                            ParentId = cb.ParentId,
                                                            Text = cb.Text,
                                                            Type = ItemTypeEnum.Serie,
                                                            UserId = cb.UserId,
                                                            UserName = cb.User.FirstName + " " + cb.User.LastName,
                                                            UserImageUpdated = cb.User.AvatarUpdated,
                                                            IsPublished = cb.IsPublished,
                                                            HasChildren = cb.Children.Any()
                                                        }).OrderBy(cb => cb.Created),
                                                    ViewStats = new ViewStatsModel
                                                        {
                                                            Today =
                                                                s.ViewStats.Count(
                                                                    v =>
                                                                    DbFunctions.DiffDays(v.Created, DateTime.UtcNow) ==
                                                                    0),
                                                            Week =
                                                                s.ViewStats.Count(
                                                                    v =>
                                                                    DbFunctions.DiffDays(v.Created, DateTime.UtcNow) <=
                                                                    dayOfWeekOffset),
                                                            Month =
                                                                s.ViewStats.Count(
                                                                    v =>
                                                                    DbFunctions.DiffMonths(v.Created,
                                                                                               DateTime.UtcNow) ==
                                                                    0),
                                                            Year =
                                                                s.ViewStats.Count(
                                                                    v =>
                                                                    DbFunctions.DiffYears(v.Created, DateTime.UtcNow) ==
                                                                    0),
                                                            Total = s.ViewStats.Count()
                                                        }
                                                }).FirstOrDefault();
                return result;
            }
        }

        public Guid AddComment(Guid foreignId, Guid userId, Guid? parentCommentId, string commentText,
                               ItemTypeEnum commentType)
        {
            using (Entities context = GetContext())
            {
                var newComment = new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        UserId = userId,
                        Created = DateTime.UtcNow,
                        IsPublished = true,
                        Text = commentText,
                        ParentId = parentCommentId,
                    };
                switch (commentType)
                {
                    case ItemTypeEnum.Serie:
                        newComment.SerieId = foreignId;
                        break;
                    case ItemTypeEnum.Book:
                        newComment.BookId = foreignId;
                        break;
                    case ItemTypeEnum.Author:
                        newComment.AuthorId = foreignId;
                        break;
                    case ItemTypeEnum.Review:
                        newComment.ReviewId = foreignId;
                        break;
                }
                context.Comments.Add(newComment);
                context.SaveChanges();
                return newComment.CommentId;
            }
        }


        public void DeleteComment(Guid commentId)
        {
            using (Entities context = GetContext())
            {
                Comment comment = context.Comments.Include("Children").FirstOrDefault(c => c.CommentId == commentId);
                if (comment != null)
                {
                    if (comment.Children.Any())
                    {
                        comment.IsPublished = false;
                    }
                    else
                    {
                        context.Comments.Remove(comment);
                    }
                    context.SaveChanges();
                }
            }
        }


        public void SendComplain(Guid commentId, Guid userId, string reasonText)
        {
            using (Entities context = GetContext())
            {
                var complain = new Complain
                    {
                        ComplainId = Guid.NewGuid(),
                        Created = DateTime.UtcNow,
                        Reason = reasonText,
                        CommentId = commentId,
                        UserId = userId
                    };
                context.Complains.Add(complain);
                context.SaveChanges();
            }
        }

        public RatingModel SetRating(Guid foreignId, Guid userId, short ratingValue, ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                if (ratingValue > 0)
                {
                    Rating rating = null;
                    switch (itemType)
                    {
                        case ItemTypeEnum.Serie:
                            rating = context.Ratings.FirstOrDefault(ar => ar.SerieId == foreignId && ar.UserId == userId);
                            break;
                        case ItemTypeEnum.Book:
                            rating = context.Ratings.FirstOrDefault(ar => ar.BookId == foreignId && ar.UserId == userId);
                            break;
                        case ItemTypeEnum.Author:
                            rating =
                                context.Ratings.FirstOrDefault(ar => ar.AuthorId == foreignId && ar.UserId == userId);
                            break;
                        case ItemTypeEnum.Review:
                            rating =
                                context.Ratings.FirstOrDefault(ar => ar.ReviewId == foreignId && ar.UserId == userId);
                            break;
                    }

                    if (rating == null)
                    {
                        rating = new Rating
                            {
                                RatingId = Guid.NewGuid(),
                                UserId = userId,
                                Value = ratingValue,
                                Created = DateTime.UtcNow
                            };
                        switch (itemType)
                        {
                            case ItemTypeEnum.Serie:
                                rating.SerieId = foreignId;
                                break;
                            case ItemTypeEnum.Book:
                                rating.BookId = foreignId;
                                break;
                            case ItemTypeEnum.Author:
                                rating.AuthorId = foreignId;
                                break;
                            case ItemTypeEnum.Review:
                                rating.ReviewId = foreignId;
                                break;
                        }
                        context.Ratings.Add(rating);
                    }
                    else
                    {
                        rating.Value = ratingValue;
                    }
                }
                context.SaveChanges();

                IQueryable<Rating> query = context.Ratings;
                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        query = query.Where(a => a.SerieId == foreignId);
                        break;
                    case ItemTypeEnum.Book:
                        query = query.Where(a => a.BookId == foreignId);
                        break;
                    case ItemTypeEnum.Author:
                        query = query.Where(a => a.AuthorId == foreignId);
                        break;
                    case ItemTypeEnum.Review:
                        query = query.Where(a => a.ReviewId == foreignId);
                        break;
                }


                return new RatingModel
                    {
                        Value = query.Average(a => (short?)a.Value) ?? 0,
                        Count = query.Count()
                    };
            }
        }

        public bool IsItemSubscribed(Guid foreignId, Guid userId, ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                IQueryable<Subscription> query = context.Subscriptions;
                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        query = query.Where(a => a.SerieId == foreignId);
                        break;
                    case ItemTypeEnum.Book:
                        query = query.Where(a => a.BookId == foreignId);
                        break;
                    case ItemTypeEnum.Author:
                        query = query.Where(a => a.AuthorId == foreignId);
                        break;
                    case ItemTypeEnum.Review:
                        query = query.Where(a => a.ReviewId == foreignId);
                        break;
                }
                return query.Any(q => q.UserId == userId);
            }
        }

        public void Subscribe(Guid foreignId, Guid userId, ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                var item = new Subscription
                    {
                        SubscriptionId = Guid.NewGuid(),
                        Created = DateTime.UtcNow,
                        UserId = userId
                    };

                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        item.SerieId = foreignId;
                        break;
                    case ItemTypeEnum.Book:
                        item.BookId = foreignId;
                        break;
                    case ItemTypeEnum.Author:
                        item.AuthorId = foreignId;
                        break;
                    case ItemTypeEnum.Review:
                        item.ReviewId = foreignId;
                        break;
                }
                context.Subscriptions.Add(item);
                context.SaveChanges();
            }
        }

        public void UnSubscribe(Guid foreignId, Guid userId, ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                IQueryable<Subscription> query = context.Subscriptions;
                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        query = query.Where(a => a.SerieId == foreignId);
                        break;
                    case ItemTypeEnum.Book:
                        query = query.Where(a => a.BookId == foreignId);
                        break;
                    case ItemTypeEnum.Author:
                        query = query.Where(a => a.AuthorId == foreignId);
                        break;
                    case ItemTypeEnum.Review:
                        query = query.Where(a => a.ReviewId == foreignId);
                        break;
                }
                Subscription item = query.FirstOrDefault(s => s.UserId == userId);
                context.Subscriptions.Remove(item);
                context.SaveChanges();
            }
        }


        public CatalogueModel GetCatalogueModel(CatalogueViewTypeEnum viewType, string searchQuery, int lastRowIndex)
        {
            if (searchQuery.ToLower() == "/reindex")
                SearchService.CreateIndex();

            var result = new CatalogueModel
                {
                    ViewType = viewType,
                    ResultType = ResultTypeEnum.Search
                };

            IEnumerable<LuceneResultModel> searchResults = SearchService.GetSearchResults(searchQuery);
            using (Entities context = GetContext())
            {
                IQueryable<Book> books = context.Books;
                IQueryable<Author> authors = context.Authors.Where(a => a.Books.Any());

                IList<Guid> bookList = searchResults.Where(sr => sr.Type == ItemTypeEnum.Book)
                                                    .Skip(lastRowIndex)
                                                    .Take(ConstPageRowsCount).Select(sr => sr.Id).ToList();

                books = books.Where(br => bookList.Contains(br.BookId));

                IList<Guid> authorList = searchResults.Where(sr => sr.Type == ItemTypeEnum.Author)
                                                      .Skip(lastRowIndex)
                                                      .Take(ConstPageRowsCount).Select(sr => sr.Id).ToList();

                authors = authors.Where(ar => authorList.Contains(ar.AuthorId));

                if (viewType == CatalogueViewTypeEnum.Books)
                {
                    result.FrontViewTotalCount = searchResults.Count(sr => sr.Type == ItemTypeEnum.Book);
                    result.BackViewTotalCount = searchResults.Count(sr => sr.Type == ItemTypeEnum.Author);
                    result.Items = GetSearchSuggestionBooksQuery(books.OrderBy(b => b.Name)).ToList();
                }
                else
                {
                    result.FrontViewTotalCount = searchResults.Count(sr => sr.Type == ItemTypeEnum.Author);
                    result.BackViewTotalCount = searchResults.Count(sr => sr.Type == ItemTypeEnum.Book);
                    result.Items = GetSearchSuggestionAuthorsQuery(authors.OrderBy(a => a.Name)).ToList();
                }
            }
            result.LastRowCount = lastRowIndex + result.Items.Count();
            return result;
        }

        public CatalogueModel GetCatalogueModel(CatalogueViewTypeEnum viewType, Guid? genreId, int lastRowIndex)
        {
            var result = new CatalogueModel
                {
                    ViewType = viewType,
                };

            using (Entities context = GetContext())
            {
                IQueryable<Book> books = context.Books;
                IQueryable<Author> authors = context.Authors.Where(a => a.Books.Any());
                result.ResultType = ResultTypeEnum.All;

                if (genreId != null && genreId != Guid.Empty)
                {
                    books = books.Where(br => br.Genres.Any(g => g.GenreId == genreId));
                    authors = authors.Where(ar => ar.Books.Any(b => b.Genres.Any(g => g.GenreId == genreId)));
                    result.ResultType = ResultTypeEnum.Genre;
                    result.SelectedGenre = context.Genres.Where(g => g.GenreId == genreId)
                                                  .Select(g => g.Name)
                                                  .FirstOrDefault();
                }

                if (viewType == CatalogueViewTypeEnum.Books)
                {
                    result.FrontViewTotalCount = books.Count();
                    result.BackViewTotalCount = authors.Count();
                    result.Items =
                        GetSearchSuggestionBooksQuery(
                            books.OrderByDescending(b => b.Created).ThenBy(b => b.Name).Skip(lastRowIndex).Take(ConstPageRowsCount)).ToList();
                }
                else
                {
                    result.FrontViewTotalCount = authors.Count();
                    result.BackViewTotalCount = books.Count();
                    result.Items =
                        GetSearchSuggestionAuthorsQuery(
                            authors.OrderBy(b => b.Name).Skip(lastRowIndex).Take(ConstPageRowsCount)).ToList();
                }
                result.LastRowCount = lastRowIndex + result.Items.Count();
            }
            return result;
        }

        public IEnumerable<ReviewItemModel> GetTopReviews(int lastRowIndex = 0, int count = 6)
        {
            using (Entities context = GetContext())
            {
                IQueryable<Review> query =
                    context.Reviews.OrderByDescending(b => b.Created).Skip(lastRowIndex).Take(count);
                List<ReviewItemModel> reviews = query.Select(br => new ReviewItemModel
                    {
                        Id = br.ReviewId,
                        Name = br.Title,
                        CreatedBy = new IdentityInfoModel
                            {
                                Id = br.User.UserId,
                                FirstName = br.User.FirstName,
                                LastName = br.User.LastName
                            },
                        ReviewRating = br.Book.Ratings.Where(rr => rr.UserId == br.UserId).Select(rr => rr.Value).FirstOrDefault(),
                        Rating = new RatingModel { Value = (double?)br.Ratings.Average(r => r.Value) ?? 0 },
                        HasImage = false,
                        Description = br.Text,
                        Created = br.Created,
                        Book = new BookItemModel
                            {
                                Id = br.Book.BookId,
                                Name = br.Book.Name,
                                Authors =
                                    br.Book.Authors.Select(a => new AuthorItemModel { Id = a.AuthorId, Name = a.Name }),
                            }
                    }).ToList();
                return reviews;
            }
        }

        public IEnumerable<BookItemModel> GetTopBooks(int lastRowIndex = 0, int count = 10)
        {
            using (Entities context = GetContext())
            {
                IQueryable<Book> query = context.Books.OrderByDescending(b => b.Created).Skip(lastRowIndex).Take(count);
                return GetSearchSuggestionBooksQuery(query).ToList();
            }
        }

        public WholeAmountModel GetWholeAmounts()
        {
            using (Entities context = GetContext())
            {
                return new WholeAmountModel
                    {
                        AuthorsCount = context.Authors.Count(a => a.Books.Any()),
                        BooksCount = context.Books.Count()
                    };
            }
        }

        public void SetPageView(Guid sessionId, ItemTypeEnum itemType, string userAgent, string address, Guid foreignId,
                                Guid? userId)
        {
            using (Entities context = GetContext())
            {
                IQueryable<ViewStat> query = context.ViewStats.Where(v => v.SessionId == sessionId);
                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        query = query.Where(q => q.SerieId == foreignId);

                        break;
                    case ItemTypeEnum.Book:
                        query = query.Where(q => q.BookId == foreignId);
                        break;
                    case ItemTypeEnum.Author:
                        query = query.Where(q => q.AuthorId == foreignId);
                        break;
                    case ItemTypeEnum.Review:
                        query = query.Where(q => q.ReviewId == foreignId);
                        break;
                }

                if (!query.Any())
                {
                    var viewStat = new ViewStat
                        {
                            ViewStatId = Guid.NewGuid(),
                            Created = DateTime.UtcNow,
                            Address = address,
                            UserAgent = userAgent,
                            UserId = userId,
                            SessionId = sessionId
                        };

                    switch (itemType)
                    {
                        case ItemTypeEnum.Serie:
                            viewStat.SerieId = foreignId;
                            break;
                        case ItemTypeEnum.Book:
                            viewStat.BookId = foreignId;
                            break;
                        case ItemTypeEnum.Author:
                            viewStat.AuthorId = foreignId;
                            break;
                        case ItemTypeEnum.Review:
                            viewStat.ReviewId = foreignId;
                            break;
                    }
                    try
                    {
                        context.ViewStats.Add(viewStat);
                        context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        // WRONG ID RECEIVED.
                        if (!(e is DbUpdateException))
                            throw;
                    }
                }
            }
        }

        public UserStatsModel SetUserStatToBook(Guid bookId, Guid userId, UserStatStateEnum state)
        {
            using (Entities context = GetContext())
            {
                UserStat stat = context.UserStats.FirstOrDefault(u => u.BookId == bookId && u.UserId == userId);
                if (state == UserStatStateEnum.None)
                {
                    if (stat != null)
                        context.UserStats.Remove(stat);
                }
                else
                {
                    if (stat == null)
                    {
                        stat = new UserStat
                            {
                                UserStatId = Guid.NewGuid(),
                                BookId = bookId,
                                UserId = userId,
                            };
                        context.UserStats.Add(stat);
                    }
                    stat.Created = DateTime.UtcNow;
                    stat.State = state;
                }
                context.SaveChanges();
                return context.Books.Where(u => u.BookId == bookId).Select(u => new UserStatsModel
                    {
                        IsReading = u.UserStats.Count(s => s.State == UserStatStateEnum.IsReading),
                        Read = u.UserStats.Count(s => s.State == UserStatStateEnum.Read),
                        WantsToRead = u.UserStats.Count(s => s.State == UserStatStateEnum.WantsToRead),
                        State = u.UserStats.Where(us => us.UserId == userId).Select(us => us.State).FirstOrDefault()
                    }).FirstOrDefault();
            }
        }

        public int GetUnprocessedIncomesCount(ItemTypeEnum? itemType = null)
        {
            using (Entities context = GetContext())
            {
                var query = context.Incomes.Where(i => !i.IsPublished);
                if (itemType != null)
                    query = query.Where(q => q.ItemType == (int)itemType);
                return query.Count();
            }
        }

        public Guid ApplyIncomeItem(ItemTypeEnum itemType, Guid userId, string name, string xmlBody, Guid? foreignId)
        {
            using (Entities context = GetContext())
            {
                Income income = null;
                if (foreignId != null)
                    income = context.Incomes.FirstOrDefault(i => !i.IsPublished && (i.UserId == userId && (i.BookId == foreignId || i.AuthorId == foreignId || i.SerieId == foreignId)));

                if (income == null)
                {
                    income = new Income
                    {
                        IncomeId = Guid.NewGuid(),
                        UserId = userId,
                        IsPublished = false,
                        ItemType = (int)itemType
                    };
                    if (foreignId != null)
                    {
                        switch (itemType)
                        {
                            case ItemTypeEnum.Book:
                                income.BookId = foreignId;
                                break;
                            case ItemTypeEnum.Author:
                                income.AuthorId = foreignId;
                                break;
                            case ItemTypeEnum.Serie:
                                income.SerieId = foreignId;
                                break;
                        }
                    }
                    context.Incomes.Add(income);
                }

                income.Name = name;
                income.Created = DateTime.UtcNow;
                income.Body = xmlBody;

                context.SaveChanges();

                return income.IncomeId;
            }
        }


        public void UpdateIncomeItem(Guid incomeId, string name, string xmlBody)
        {
            using (Entities context = GetContext())
            {
                var income = context.Incomes.First(i => i.IncomeId == incomeId);
                income.Name = name;
                income.Body = xmlBody;
                context.SaveChanges();
            }
        }


        public void DeleteIncomeItem(Guid incomeId)
        {
            using (Entities context = GetContext())
            {
                Income income = context.Incomes.First(i => i.IncomeId == incomeId);
                context.Incomes.Remove(income);
                context.SaveChanges();
            }
        }


        private IQueryable<UserStat> AddGenreQuery(IQueryable<UserStat> query, Guid? genreId = null)
        {
            if (genreId != null)
                return query.Where(q => q.Book.Genres.Any(g => g.GenreId == genreId));
            return query;
        }

        public MyBooksModel GetMyBooks(Guid userId, UserStatStateEnum state, int lastRowIndex = 0, Guid? genreId = null)
        {
            using (Entities context = GetContext())
            {
                var result = new MyBooksModel
                    {
                        Stats = new UserStatsModel
                            {
                                //WantsToRead = AddGenreQuery(context.UserStats.Where(us => us.UserId == userId && us.State == UserStatStateEnum.WantsToRead), genreId).Count(),
                                //IsReading = AddGenreQuery(context.UserStats.Where(us => us.UserId == userId && us.State == UserStatStateEnum.IsReading), genreId).Count(),
                                //Read = AddGenreQuery(context.UserStats.Where(us => us.UserId == userId && us.State == UserStatStateEnum.Read), genreId).Count(),
                                WantsToRead = context.UserStats.Count(us => us.UserId == userId && us.State == UserStatStateEnum.WantsToRead),
                                IsReading = context.UserStats.Count(us => us.UserId == userId && us.State == UserStatStateEnum.IsReading),
                                Read = context.UserStats.Count(us => us.UserId == userId && us.State == UserStatStateEnum.Read),

                                State = state,
                            },
                        TotalCount = AddGenreQuery(context.UserStats.Where(us => us.UserId == userId && us.State == state), genreId).Count(),
                        Books =
                            AddGenreQuery(context.UserStats.Where(us => us.UserId == userId && us.State == state), genreId).OrderByDescending(s => s.Created).Skip(lastRowIndex).Take(ConstPageRowsCount)
                                   .Select(b => new BookItemModel
                                       {
                                           Id = b.BookId,
                                           HasImage = b.Book.Cover != null,
                                           Name = b.Book.Name,
                                           Rating =
                                               new RatingModel
                                                   {
                                                       Value = (double?)b.Book.Ratings.Average(r => r.Value) ?? 0
                                                   },
                                           Authors = b.Book.Authors.Select(aa => new AuthorItemModel
                                               {
                                                   Id = aa.AuthorId,
                                                   Name = aa.Name
                                               }).OrderBy(ss => ss.Name)
                                       }).ToList(),
                    };


                var genres = GetGenres(genreId);

                var bookGenres = context.UserStats.Where(us => us.UserId == userId && us.State == state)
                    .SelectMany(s => s.Book.Genres.Select(g => new { g.GenreId, g.ParentId })).Distinct().ToList();

                result.Genres = genres.Where(item => bookGenres.Any(g => g.ParentId == item.Id));
                foreach (var item in result.Genres)
                {
                    item.Children = item.Children.Where(c => bookGenres.Any(b => b.GenreId == c.Id));
                }

                result.LastRowCount = lastRowIndex + result.Books.Count();
                return result;
            }
        }

        public UserStatsModel GetUserStats(Guid userId, UserStatStateEnum state = UserStatStateEnum.None)
        {
            using (Entities context = GetContext())
            {
                return new UserStatsModel
                {
                    WantsToRead =
                        context.UserStats.Count(
                            us => us.UserId == userId && us.State == UserStatStateEnum.WantsToRead),
                    IsReading =
                        context.UserStats.Count(us => us.UserId == userId && us.State == UserStatStateEnum.IsReading),
                    Read = context.UserStats.Count(us => us.UserId == userId && us.State == UserStatStateEnum.Read),
                    State = state
                };
            }
        }


        public ProceedItemModel ProcessInñomeItem(Guid incomeId, Guid processedByUserId)
        {
            ProceedItemModel result = null;
            using (Entities context = GetContext())
            {
                var incomeItem = context.Incomes.FirstOrDefault(i => i.IncomeId == incomeId);
                if (incomeItem != null)
                {
                    switch (incomeItem.ItemType)
                    {
                        case (int)ItemTypeEnum.Review:
                            {
                                result = AddReview(context, incomeItem);
                                break;
                            }
                        case (int)ItemTypeEnum.Serie:
                            {
                                result = ModifySerie(context, incomeItem);
                                break;
                            }
                        case (int)ItemTypeEnum.Author:
                            {
                                result = ModifyAuthor(context, incomeItem);
                                break;
                            }
                        case (int)ItemTypeEnum.Book:
                            {
                                result = ProcessBook(context, incomeItem);
                                break;
                            }
                    }
                    incomeItem.IsPublished = true;
                    incomeItem.ProcessedByUserId = processedByUserId;
                    context.SaveChanges();
                }
            }
            return result;
        }

        private ProceedItemModel ProcessBook(Entities context, Income incomeItem)
        {
            IncomeModel incomeBook = DeserializeModel<IncomeModel>(incomeItem.Body);
            Book book = context.Books.FirstOrDefault(i => i.BookId == incomeBook.IncomeBook.Id);
            if (book == null)
            {
                book = new Book();
                book.BookId = Guid.NewGuid();
                book.Created = DateTime.Now;
                context.Books.Add(book);
            }
            if (book.UserId == null)
            {
                book.UserId = incomeItem.UserId;
            }
            book.Name = incomeBook.IncomeBook.Name;
            book.Published = incomeBook.IncomeBook.Published;
            book.PagesCount = incomeBook.IncomeBook.PagesCount;
            book.Publisher = incomeBook.IncomeBook.Publisher;
            book.Description = incomeBook.IncomeBook.Annotation;
            book.ISBN = incomeBook.IncomeBook.ISBN;
            if (incomeBook.IncomeBook.Cover != null)
            {
                book.Cover = incomeBook.IncomeBook.Cover;
                book.CoverUpdated = DateTime.Now;
            }
            ProcessAuthors(context, book, incomeBook.IncomeAuthors);
            ProcessSeries(context, book, incomeBook.IncomeSeries);
            ProcessGenres(context, book, incomeBook.IncomeGenres);
            ProcessSimilarBooks(context, book, incomeItem.UserId, incomeBook.IncomeSimilarBooks);
            var result = new ProceedItemModel
            {
                Id = book.BookId,
                ItemType = ItemTypeEnum.Book,
                UserId = incomeItem.UserId
            };
            return result;
        }

        private void ProcessSimilarBooks(Entities context, Book book, Guid userId, List<IncomeSimilarBookModel> similarBooks)
        {
            book.BookRelations.Clear();
            foreach (IncomeSimilarBookModel incomeSimilar in similarBooks)
            {
                BookRelation bookRelation = new BookRelation
                {
                    BookId = book.BookId,
                    RelatedBookId = (Guid)incomeSimilar.Id,
                    IsRelatedByGenre = true,
                    Score = .9,
                    UserId = userId
                };
                book.BookRelations.Add(bookRelation);
            }
        }

        private void ProcessGenres(Entities context, Book book, List<IncomeGenreModel> genres)
        {
            book.Genres.Clear();
            foreach (IncomeGenreModel incomeGenre in genres)
            {
                Genre genre = context.Genres.FirstOrDefault(i => i.GenreId == incomeGenre.Id);
                if (genre != null)
                {
                    book.Genres.Add(genre);
                }
            }
        }

        private void ProcessSeries(Entities context, Book book, List<IncomeSerieModel> series)
        {
            book.BookInSeries.Clear();
            foreach (IncomeSerieModel incomeSerie in series)
            {
                Serie serie = null;
                if (incomeSerie.Id != null)
                {
                    serie = context.Series.FirstOrDefault(i => i.SerieId == incomeSerie.Id);
                }
                else
                {
                    serie = new Serie
                    {
                        SerieId = Guid.NewGuid(),
                        Name = incomeSerie.Name,
                        Description = incomeSerie.Description
                    };
                    context.Series.Add(serie);
                }
                BookInSerie bookInSerie = new BookInSerie
                {
                    BookId = book.BookId,
                    SerieId = serie.SerieId
                };
                book.BookInSeries.Add(bookInSerie);
            }
        }

        private void ProcessAuthors(Entities context, Book book, List<IncomeAuthorModel> authors)
        {
            book.Authors.Clear();
            foreach (IncomeAuthorModel incomeAuthor in authors)
            {
                Author author = null;
                if (incomeAuthor.Id != null)
                {
                    author = context.Authors.FirstOrDefault(i => i.AuthorId == incomeAuthor.Id);
                }
                else
                {
                    author = new Author
                    {
                        AuthorId = Guid.NewGuid(),
                        Name = incomeAuthor.Name,
                        Biography = incomeAuthor.Biography,
                        BirthDate = incomeAuthor.BirthDate,
                        DeathDate = incomeAuthor.DeathDate,
                        PhotoUpdated = DateTime.Now
                    };
                    if (incomeAuthor.Photo != null)
                    {
                        author.Photo = incomeAuthor.Photo;
                        author.PhotoUpdated = DateTime.Now;
                    }
                    context.Authors.Add(author);
                }
                book.Authors.Add(author);
            }
        }

        private ProceedItemModel ModifyAuthor(Entities context, Income incomeItem)
        {
            ProceedItemModel result = null;
            IncomeAuthorModel incomeAuthor = DeserializeModel<IncomeAuthorModel>(incomeItem.Body);
            var author = context.Authors.FirstOrDefault(i => i.AuthorId == incomeAuthor.Id);
            if (author != null)
            {
                author.Name = incomeAuthor.Name;
                if (!incomeAuthor.HasImage || incomeAuthor.Photo != null)
                {
                    author.Photo = incomeAuthor.Photo;
                    author.PhotoUpdated = DateTime.Now;
                }
                author.Biography = incomeAuthor.Biography;
                author.BirthDate = incomeAuthor.BirthDate;
                author.DeathDate = incomeAuthor.DeathDate;
                result = new ProceedItemModel
                {
                    Id = author.AuthorId,
                    ItemType = ItemTypeEnum.Author,
                    UserId = incomeItem.UserId
                };
            }
            return result;
        }

        private ProceedItemModel ModifySerie(Entities context, Income incomeItem)
        {
            ProceedItemModel result = null;
            IncomeSerieModel incomeSerie = DeserializeModel<IncomeSerieModel>(incomeItem.Body);
            var serie = context.Series.FirstOrDefault(i => i.SerieId == incomeSerie.Id);
            if (serie != null)
            {
                serie.Name = incomeSerie.Name;
                serie.Description = incomeSerie.Description;
                result = new ProceedItemModel
                {
                    Id = serie.SerieId,
                    ItemType = ItemTypeEnum.Serie,
                    UserId = incomeItem.UserId
                };
            }
            return result;
        }

        private ProceedItemModel AddReview(Entities context, Income incomeItem)
        {
            IncomeReviewModel incomeReview = DeserializeModel<IncomeReviewModel>(incomeItem.Body);

            Review review = new Review
            {
                ReviewId = Guid.NewGuid(),
                UserId = incomeItem.UserId,
                Title = incomeReview.Name,
                BookId = incomeReview.BookId,
                Text = incomeReview.Description,
                Created = DateTime.Now,
                IsPublished = true,
            };
            SetRating(review.BookId, incomeItem.UserId, (short)incomeReview.Rating, ItemTypeEnum.Book);

            context.Reviews.Add(review);
            return new ProceedItemModel
            {
                Id = review.ReviewId,
                ItemType = ItemTypeEnum.Review,
                UserId = incomeItem.UserId,
                RefId = review.BookId,
                Rating = (short)incomeReview.Rating
            };
        }

        private T DeserializeModel<T>(string modelXml)
        {
            StringBuilder builder = new StringBuilder();
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(modelXml))
            {
                return (T)s.Deserialize(reader);
            }
        }

        public Guid AddOpdLink(string name, string uri, string searchUri, Guid userId)
        {
            using (Entities context = GetContext())
            {
                OpdsLink link = new OpdsLink
                    {
                        OpdsLinkId = Guid.NewGuid(),
                        Name = name,
                        Uri = uri,
                        SearchUri = searchUri,
                        UserId = userId
                    };
                context.OpdsLinks.Add(link);
                context.SaveChanges();
                return link.OpdsLinkId;
            }
        }

        public void UpdateOpdLink(OpdsLinkModel link)
        {
            using (Entities context = GetContext())
            {
                OpdsLink record = context.OpdsLinks.First(o => o.OpdsLinkId == link.Id);
                record.Name = link.Name;
                record.Uri = link.Uri;
                record.SearchUri = link.SearchUri;
                context.SaveChanges();
            }
        }

        public void DeleteOpdsLink(Guid id)
        {
            using (Entities context = GetContext())
            {
                OpdsLink record = context.OpdsLinks.First(o => o.OpdsLinkId == id);
                context.OpdsLinks.Remove(record);
                context.SaveChanges();
            }
        }

        public OpdsSectionModel GetOpdsSection(Guid bookId, Guid? userId, bool getAll = true)
        {
            using (Entities context = GetContext())
            {
                return new OpdsSectionModel
                {
                    BookName = context.Books.Where(b => b.BookId == bookId).Select(b => b.Name).First(),
                    Items = GetOpdsLinks(userId, getAll)
                };
            }
        }

        public IEnumerable<OpdsLinkModel> GetOpdsLinks(Guid? userId, bool getAll = true)
        {
            using (Entities context = GetContext())
            {
                IQueryable<OpdsLink> query = context.OpdsLinks.Where(o => o.UserId == userId);
                if (getAll && userId != null)
                    query = query
                        .Union(context.OpdsLinks.Where(o => o.UserId == null));

                query = query.OrderBy(o => o.UserId != null)
                    .ThenBy(o => o.Name);

                return query.Select(q => new OpdsLinkModel
                {
                    Id = q.OpdsLinkId,
                    Name = q.Name,
                    Uri = q.Uri,
                    SearchUri = q.SearchUri,
                    UserId = q.UserId
                }).ToList();
            }
        }


        public string GetIncomeItem(ItemTypeEnum itemType, Guid foreignId, Guid? userId = null)
        {
            using (Entities context = GetContext())
            {
                IQueryable<Income> query = context.Incomes.Where(i => !i.IsPublished);
                if (userId != null)
                    query = query.Where(i => i.UserId == userId);
                switch (itemType)
                {
                    case ItemTypeEnum.Serie:
                        query = query.Where(ar => ar.SerieId == foreignId || ar.IncomeId == foreignId);
                        break;
                    case ItemTypeEnum.Book:
                        query = query.Where(ar => ar.BookId == foreignId || ar.IncomeId == foreignId);
                        break;
                    case ItemTypeEnum.Author:
                        query = query.Where(ar => ar.AuthorId == foreignId || ar.IncomeId == foreignId);
                        break;
                    case ItemTypeEnum.Review:
                        query = query.Where(ar => ar.IncomeId == foreignId);
                        break;
                }
                return query.Select(i => i.Body).FirstOrDefault();
            }
        }


        public bool IsUserInRole(Guid userId, string roleName)
        {
            using (Entities context = GetContext())
            {
                return context.Users.Any(u => u.UserId == userId && u.UserRoles.Any(r => r.RoleName.ToLower() == roleName.ToLower()));
            }
        }

        public BookSourceModel GetBookSource(Guid bookId, Guid? userId, bool isSuperUser)
        {
            using (Entities context = GetContext())
            {
                return context.BookContents
                    .Where(b => b.BookId == bookId && (b.Book.UserDownloadAccess == UserDownloadAccessEnum.Free || isSuperUser))
                    .Select(b => new BookSourceModel
                            {
                                Name = b.Book.Name,
                                Body = b.Source,
                                FileName = b.SourceFileName
                            }).FirstOrDefault();
            }
        }

        public IEnumerable<DeviceModel> GetDevices(Guid userId)
        {
            using (Entities context = GetContext())
            {
                return context.Devices.Where(d => d.UserId == userId).Select(d => new DeviceModel
                {
                    Id = d.DeviceId,
                    Name = d.Name,
                    Email = d.Email,
                    UserId = d.UserId
                }).ToList();
            }
        }

        public UserIncomesModel GetUserIncomes(Guid userId, ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                var result = new UserIncomesModel
                {
                    ItemType = itemType,
                    BooksCount = context.Incomes.Count(i => !i.IsPublished && i.UserId == userId && i.ItemType == (int)ItemTypeEnum.Book),
                    AuthorsCount = context.Incomes.Count(i => !i.IsPublished && i.UserId == userId && i.ItemType == (int)ItemTypeEnum.Author),
                    SeriesCount = context.Incomes.Count(i => !i.IsPublished && i.UserId == userId && i.ItemType == (int)ItemTypeEnum.Serie),
                    ReviewsCount = context.Incomes.Count(i => !i.IsPublished && i.UserId == userId && i.ItemType == (int)ItemTypeEnum.Review)
                };

                result.Items = context.Incomes
                    .Where(i => !i.IsPublished && i.UserId == userId && i.ItemType == (int)itemType)
                    .OrderByDescending(i => i.Created)
                    .Select(i => new IncomeItemModel
                    {
                        Id = i.IncomeId,
                        State = i.BookId == null && i.AuthorId == null && i.SerieId == null ? IncomeStateEnum.New : IncomeStateEnum.Update,
                        ItemType = itemType,
                        Created = i.Created,
                        Name = i.Name,
                        UserId = userId,
                        UserName = i.User.FirstName + " " + i.User.LastName
                    }).ToList();

                return result;
            }
        }


        public UserIncomesModel GetUnprocessedIncomes(ItemTypeEnum itemType)
        {
            using (Entities context = GetContext())
            {
                var result = new UserIncomesModel
                {
                    ItemType = itemType,
                    BooksCount = context.Incomes.Count(
                            i => !i.IsPublished && i.ItemType == (int)ItemTypeEnum.Book),
                    AuthorsCount =
                        context.Incomes.Count(i => !i.IsPublished && i.ItemType == (int)ItemTypeEnum.Author),
                    SeriesCount =
                        context.Incomes.Count(i => !i.IsPublished && i.ItemType == (int)ItemTypeEnum.Serie),
                    ReviewsCount =
                        context.Incomes.Count(i => !i.IsPublished && i.ItemType == (int)ItemTypeEnum.Review)
                };
                result.ItemType = itemType;
                result.Items = context.Incomes.Where(i => i.ItemType == (int)itemType && !i.IsPublished).OrderByDescending(i => i.Created)
                    .Select(q => new IncomeItemModel
                {
                    Id = q.IncomeId,
                    Created = q.Created,
                    Name = q.Name,
                    ItemType = (ItemTypeEnum)q.ItemType,
                    UserId = q.UserId,
                    UserName = q.User.FirstName + " " + q.User.LastName,
                    IsPublished = q.IsPublished,
                    State = (q.BookId == null && q.AuthorId == null && q.SerieId == null) ? IncomeStateEnum.New : IncomeStateEnum.Update
                }).ToList();

                return result;
            }
        }


        public Guid AddDevice(string name, string email, Guid userId)
        {
            using (Entities context = GetContext())
            {
                var item = new Device
                {
                    DeviceId = Guid.NewGuid(),
                    Name = name,
                    Email = email,
                    UserId = userId
                };
                context.Devices.Add(item);
                context.SaveChanges();
                return item.DeviceId;
            }
        }

        public void UpdateDevice(DeviceModel device)
        {
            using (Entities context = GetContext())
            {
                var item = context.Devices.First(d => d.DeviceId == device.Id);
                item.Name = device.Name;
                item.Email = device.Email;
                context.SaveChanges();
            }

        }

        public void DeleteDevice(Guid id)
        {
            using (Entities context = GetContext())
            {
                var item = context.Devices.First(d => d.DeviceId == id);
                context.Devices.Remove(item);
                context.SaveChanges();
            }
        }


        public Guid AddCitation(Guid bookId, Guid userId, string text)
        {
            using (Entities context = GetContext())
            {
                Citation item = new Citation
                {
                    CitationId = Guid.NewGuid(),
                    BookId = bookId,
                    UserId = userId,
                    Text = text,
                    Created = DateTime.UtcNow
                };
                context.Citations.Add(item);
                context.SaveChanges();
                return item.CitationId;
            }
        }

        public void DeleteCitation(Guid citationId)
        {
            using (Entities context = GetContext())
            {
                var item = context.Citations.First(c => c.CitationId == citationId);
                context.Citations.Remove(item);
                context.SaveChanges();
            }
        }

        public IEnumerable<CitationModel> GetUserCitations(Guid userId)
        {
            using (Entities context = GetContext())
            {
                return context.Citations
                    .Where(c => c.UserId == userId)
                    .Select(c => new CitationModel
                    {
                        Id = c.CitationId,
                        BookId = c.BookId,
                        UserId = c.UserId,
                        Created = c.Created,
                        Text = c.Text
                    }).ToList();
            }
        }

        public IEnumerable<CitationModel> GetBookCitations(Guid bookId)
        {
            using (Entities context = GetContext())
            {
                return context.Citations
                    .Where(c => c.BookId == bookId)
                    .Select(c => new CitationModel
                    {
                        Id = c.CitationId,
                        BookId = c.BookId,
                        UserId = c.UserId,
                        Created = c.Created,
                        Text = c.Text
                    }).ToList();
            }
        }


        public void AddDownload(Guid userId, Guid bookId, DownloadTypeEnum type)
        {
            using (Entities context = GetContext())
            {
                var item = new Download
                {
                    DownloadId = Guid.NewGuid(),
                    BookId = bookId,
                    UserId = userId,
                    DownloadType = (int)type,
                    Created = DateTime.UtcNow
                };
                context.Downloads.Add(item);
                context.SaveChanges();
            }
        }


        public Guid UploadBookContent(Guid bookId, string sourceFileName, byte[] fb2Source)
        {
            using (Entities context = GetContext())
            {
                var bookContent = context.BookContents.FirstOrDefault(b => b.BookId == bookId);
                if (bookContent == null)
                {
                    bookContent = new BookContent
                    {
                        BookContentId =  Guid.NewGuid(),
                        BookId = bookId
                    };
                    context.BookContents.Add(bookContent);
                }

                bookContent.Source = fb2Source;
                bookContent.SourceFileName = sourceFileName;
                bookContent.IsOwnContent = true;
                bookContent.ContentType = ".txt";
                bookContent.Size = fb2Source.Length;
                context.SaveChanges();
                return bookContent.BookContentId;
            }
        }
    }
}