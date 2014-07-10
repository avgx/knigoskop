using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Knigoskop.DataModel;
using Knigoskop.Site.Models;
using Knigoskop.Site.Models.Shared;
using Microsoft.Data.Edm;
using OAuth2.Models;

namespace Knigoskop.Site.Services.Interface
{
    public enum DisplayTypeEnum
    {
        Books,
        Authors,
        Both
    }

    public interface IDataService
    {
        IdentityInfoModel AuthorizeUser(UserInfo userInfo, Guid? userId = null);
        byte[] GetUserAvatar(Guid userId);
        long UpdateUserAvatar(Guid userId, byte[] avatar);
        void UpdateUserProfile(IdentityInfoModel identity);
        IdentityInfoModel GetUserProfile(Guid userId);
        byte[] GetAuthorPortraitImage(Guid authorId);
        byte[] GetBookCoverImage(Guid bookId);
        IList<GenreModel> GetGenres(Guid? selectedGenreId);
        BookModel GetBook(Guid bookId, Guid? userId);
        AuthorModel GetAuthor(Guid authorId, Guid? userId);
        SerieModel GetSerie(Guid serieId, Guid? userId);
        ReviewModel GetReview(Guid reviewId, Guid? userId);
        Guid AddComment(Guid foreignId, Guid userId, Guid? parentCommentId, string commentText, ItemTypeEnum commentType);
        void DeleteComment(Guid commentId);
        RatingModel SetRating(Guid foreignId, Guid userId, short rating, ItemTypeEnum itemType);
        bool IsItemSubscribed(Guid foreignId, Guid userId, ItemTypeEnum itemType);
        void Subscribe(Guid foreignId, Guid userId, ItemTypeEnum itemType);
        void UnSubscribe(Guid foreignId, Guid userId, ItemTypeEnum itemType);
        CatalogueModel GetCatalogueModel(CatalogueViewTypeEnum viewType, string searchQuery, int lastRowIndex);
        CatalogueModel GetCatalogueModel(CatalogueViewTypeEnum viewType, Guid? genreId, int lastRowIndex = 0);
        WholeAmountModel GetWholeAmounts();
        IEnumerable<BookItemModel> GetTopBooks(int lastRowIndex = 0, int count = 10);
        IEnumerable<ReviewItemModel> GetTopReviews(int lastRowIndex = 0, int count = 10);
        void SetPageView(Guid sessionId, ItemTypeEnum itemType, string userAgent, string address, Guid foreignId, Guid? userId);
        UserStatsModel SetUserStatToBook(Guid bookId, Guid userId, UserStatStateEnum state);
        MyBooksModel GetMyBooks(Guid userId, UserStatStateEnum state, int lastRowIndex = 0, Guid? genreId = null);
        UserIncomesModel GetUserIncomes(Guid userId, ItemTypeEnum itemType);
        void SendComplain(Guid commentId, Guid userId, string reasonText);
        string GetIncomeItem(ItemTypeEnum itemType, Guid foreignId, Guid? userId = null);
        UserIncomesModel GetUnprocessedIncomes(ItemTypeEnum itemType);
        int GetUnprocessedIncomesCount(ItemTypeEnum? itemType = null);
        Guid ApplyIncomeItem(ItemTypeEnum itemType, Guid userId, string name, string xmlBody, Guid? foreignId);
        void UpdateIncomeItem(Guid incomeId, string name, string xmlBody);
        void DeleteIncomeItem(Guid incomeId);
        ProceedItemModel ProcessInсomeItem(Guid incomeId, Guid processedByUserId);
        Guid AddOpdLink(string name, string uri, string searchUri, Guid userId);
        void UpdateOpdLink(OpdsLinkModel link);
        void DeleteOpdsLink(Guid id);
        IEnumerable<OpdsLinkModel> GetOpdsLinks(Guid? userId, bool getAll = true);
        OpdsSectionModel GetOpdsSection(Guid bookId, Guid? userId, bool getAll = true);
        DownloadInfoModel GetDownloadInfo(Guid bookId, Guid? userId);
        bool IsUserInRole(Guid userId, string roleName);
        BookSourceModel GetBookSource(Guid bookId, Guid? userId, bool isSuperUser);
        
        IEnumerable<DeviceModel> GetDevices(Guid userId);
        Guid AddDevice(string name, string email, Guid userId);
        void UpdateDevice(DeviceModel device);
        void DeleteDevice(Guid id);

        Guid AddCitation(Guid bookId, Guid userId, string text);
        void DeleteCitation(Guid citationId);
        IEnumerable<CitationModel> GetUserCitations(Guid userId);
        IEnumerable<CitationModel> GetBookCitations(Guid bookId);
        void AddDownload(Guid userId, Guid bookId, DownloadTypeEnum type);
        Guid UploadBookContent(Guid bookId, string sourceFileName, byte[] fb2Source);
    }
}