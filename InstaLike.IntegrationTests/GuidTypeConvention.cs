using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace InstaLike.IntegrationTests
{
    internal class GuidTypeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(x => x.Property.PropertyType == typeof(Guid));
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.Not.ReadOnly();
            instance.Default(Guid.Empty.ToString().Replace("-", string.Empty));
        }
    }
}
