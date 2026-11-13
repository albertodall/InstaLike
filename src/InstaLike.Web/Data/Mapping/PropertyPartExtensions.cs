using FluentNHibernate.Mapping;

namespace InstaLike.Web.Data.Mapping
{
    internal static class PropertyPartExtensions
    {
        public static PropertyPart AnsiString(this PropertyPart property, int length) =>
            property.CustomType("AnsiString").Length(length);
    }
}
