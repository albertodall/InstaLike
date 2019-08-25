using System;
using System.Linq;
using System.Reflection;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Mapping;
using FluentNHibernate.Mapping.Providers;

namespace InstaLike.Web.Extensions
{
    internal static class FluentNHibernateExtensions
    {
        public static FluentMappingsContainer AddFromAssembly(this FluentMappingsContainer container, Assembly assembly, Predicate<Type> whereCondition)
        {
            if (whereCondition == null)
            {
                return container.AddFromAssembly(assembly);
            }

            var mappingTypes = 
                assembly.GetTypes()
                    .Where(t => (IsMappingOf<IMappingProvider>(t) ||
                                IsMappingOf<IIndeterminateSubclassMappingProvider>(t) ||
                                IsMappingOf<IExternalComponentMappingProvider>(t) ||
                                IsMappingOf<IFilterDefinition>(t)) && whereCondition(t));

            foreach (var mapping in mappingTypes)
            {
                container.Add(mapping);
            }

            return container;
        }

        /// <summary>
        /// From FluentNHibernate source (PersistenModel.cs:151)
        /// </summary>
        private static bool IsMappingOf<T>(Type type)
        {
            return !type.IsGenericType && typeof(T).IsAssignableFrom(type);
        }
    }
}
