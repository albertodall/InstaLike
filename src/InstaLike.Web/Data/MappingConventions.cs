using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;

namespace InstaLike.Web.Data
{
    internal sealed class AssociationsMappingConvention : IHasManyConvention, IReferenceConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.LazyLoad();
            instance.AsBag();
            instance.Cascade.AllDeleteOrphan();
            instance.Inverse();
        }

        public void Apply(IManyToOneInstance instance)
        {
            instance.LazyLoad(Laziness.Proxy);
            instance.Cascade.None();
            instance.Not.Nullable();
            instance.ForeignKey($"{instance.EntityType.Name}_{instance.Property.Name}");
        }
    }
}
