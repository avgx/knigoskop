namespace Knigoskop.Site.Models.Opds
{
    public sealed class CatalogProfileLink : BaseLink
    {
        private readonly DocumentTypeEnum _type;
        private readonly KindEnum _kind;

        public CatalogProfileLink(DocumentTypeEnum type = DocumentTypeEnum.Feed, KindEnum kind = KindEnum.None)
        {
            _type = type;
            _kind = kind;
        }

        public override string Type
        {
            get
            {
                string type = string.Empty;
                if (_type == DocumentTypeEnum.Entry)
                    type = string.Format(";type={0}", _type.ToString().ToLower());
                string kind = string.Empty;
                if (_kind != KindEnum.None)
                    kind = string.Format(";kind={0}", _kind.ToString().ToLower());
                return string.Format("application/atom+xml{0};profile=opds-catalog{1}", type, kind);
            }
        }
    }
}