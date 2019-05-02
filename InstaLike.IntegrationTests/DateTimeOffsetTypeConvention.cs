using System;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace InstaLike.IntegrationTests
{
    internal class DateTimeOffsetTypeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(x => x.Type.Name == nameof(DateTimeOffset));
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.CustomType<DateTimeOffsetUserType>();
        }
    }

    internal class GuidTypeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(x => x.Type.Name == nameof(Guid));
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.Insert();
        }
    }
}
