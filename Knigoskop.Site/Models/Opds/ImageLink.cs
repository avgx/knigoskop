namespace Knigoskop.Site.Models.Opds
{
    public sealed class ImageLink : BaseLink
    {
        private readonly string _type;
        public ImageLink(string contentType, ImageLinkTypeEnum type = ImageLinkTypeEnum.Image)
        {
            _type = contentType;
            Rel = "http://opds-spec.org/image";
            if (type == ImageLinkTypeEnum.Thumbnail)
                Rel += "/thumbnail";
        }
        public override string Type
        {
            get { return _type; }
        }
    }
}