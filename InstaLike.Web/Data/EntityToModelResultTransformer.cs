using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Transform;

namespace InstaLike.Web.Data
{
    /// <summary>
    /// Custom transformer that maps an entity result to a model type.
    /// It iterates the source properties and compares them to the target properties, keeping track
    /// of target properties that are of a different type from the corresponding source properties.
    /// For properties with the same types, it uses a standard <see cref="AliasToBeanResultTransformer"/>,
    /// while trying to map the remaining properties using a type cast.
    /// It's useful for value objects, when you want to project them partially.
    /// </summary>
    /// <typeparam name="TTarget">Target model type.</typeparam>
    internal class EntityToModelResultTransformer<TTarget> : IResultTransformer 
        where TTarget : class
    {
        private readonly AliasToBeanResultTransformer _innerTransformer;
 
        public EntityToModelResultTransformer()
        {
            _innerTransformer = new AliasToBeanResultTransformer(typeof(TTarget));
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var targetType = typeof(TTarget);

            var aliasesList = new List<string>(aliases);
            var sameTypeProperties = new List<string>(aliases);
            var differentTypeProperties = new List<string>();

            for (var i = 0; i < aliasesList.Count; i++)
            {
                var currentAlias = aliasesList[i];
                if (tuple[i].GetType() != targetType.GetProperty(currentAlias)?.PropertyType)
                {
                    differentTypeProperties.Add(aliases[i]);
                    // Aliases cannot change their position inside the "aliases" array, to not fool NHibernate
                    sameTypeProperties[i] = null;
                }
            }

            var result = _innerTransformer.TransformTuple(tuple, sameTypeProperties.ToArray());

            MapPropertiesWithDifferentTypes(tuple, differentTypeProperties, result, aliasesList);

            return result;
        }

        public IList TransformList(IList collection)
        {
            return _innerTransformer.TransformList(collection);
        }

        /// <summary>
        /// Iterates though properties that have a target type different from the source type,
        /// and tries to cast the source type's value to the target property's type.
        /// </summary>
        private static void MapPropertiesWithDifferentTypes(object[] tuple, List<string> aliasesToCast, object result, IList<string> aliasesList)
        {
            if (result is TTarget targetInstance)
            {
                foreach (var aliasToCast in aliasesToCast)
                {
                    var aliasToCastIndex = aliasesList.IndexOf(aliasToCast);
                    var aliasValue = tuple[aliasToCastIndex];
                    if (aliasValue != null)
                    {
                        var propertyToCast = targetInstance.GetType()
                            .GetProperty(aliasToCast, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                        propertyToCast?.SetValue(targetInstance, Cast(propertyToCast.PropertyType, aliasValue));
                    }
                }
            }
        }

        private static object Cast(Type targetType, object data)
        {
            var dataParameter = Expression.Parameter(typeof(object), "data");
            var expressionBody = Expression.Block(
                Expression.Convert(
                    Expression.Convert(dataParameter, data.GetType()), targetType))
                ;

            var compiledExpression = Expression.Lambda(expressionBody, dataParameter).Compile();
            return compiledExpression.DynamicInvoke(data);
        }
    }
}