using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;

namespace InstaLike.Web.Data
{
    internal class AssociationsMappingConvention : IHasManyConvention, IReferenceConvention
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

    /// <summary>
    /// Set Guid fields not nullable, for filestream references.
    /// </summary>
    internal class NotNullGuidTypeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(x => x.Property.PropertyType == typeof(Guid));
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.Not.Nullable();
        }
    }
}