using System;
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
            criteria.Expect(x => 
            {
                if (x.Type.Name == nameof(DateTimeOffset))
                {
                    return true;
                }
                return false;
            });
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.CustomType<DateTimeOffsetUserType>();
        }
    }
}
