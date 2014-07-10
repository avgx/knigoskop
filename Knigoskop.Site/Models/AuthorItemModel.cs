using Knigoskop.Site.Models.Shared;
namespace Knigoskop.Site.Models
{
    public class AuthorItemModel : BaseItemModel
    {
        public int? BornYear { get; set; }
        public int? DeathYear { get; set; }
        public int BooksCount { get; set; }
        public override ItemTypeEnum ItemType
        {
            get { return ItemTypeEnum.Author; }
        }
    }
}