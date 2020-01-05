using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Transform;

namespace InstaLike.Web.Data
{
    internal class CastPropertyTransformer<TTarget> : IResultTransformer 
        where TTarget : class
    {
        private readonly AliasToBeanResultTransformer _innerTransformer;
 
        public CastPropertyTransformer()
        {
            _innerTransformer = new AliasToBeanResultTransformer(typeof(TTarget));
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var targetType = typeof(TTarget);

            var aliasesList = new List<string>(aliases);
            var sameTypeProperties = new List<string>(aliases);
            var propertiesToCast = new List<string>();

            for (int i = 0; i < aliasesList.Count; i++)
            {
                var currentAlias = aliasesList[i];
                if (tuple[i].GetType() != targetType.GetProperty(currentAlias).PropertyType)
                {
                    propertiesToCast.Add(aliases[i]);
                    sameTypeProperties[i] = null;
                }
            }

            var result = _innerTransformer.TransformTuple(tuple, sameTypeProperties.ToArray());

            CastRemainingAliases(tuple, propertiesToCast, result, aliasesList);

            return result;
        }

        public IList TransformList(IList collection)
        {
            return _innerTransformer.TransformList(collection);
        }

        /// <summary>
        /// Iterates the Path Client.Address.City.Code 
        /// </summary>
        private static void CastRemainingAliases(object[] tuple, List<string> aliasesToCast, object result, List<string> aliasesList)
        {
            var targetInstance = result as TTarget;

            foreach (var aliasToCast in aliasesToCast)
            {
                var aliasToCastIndex = aliasesList.IndexOf(aliasToCast);
                var aliasValue = tuple[aliasToCastIndex];
                if (aliasValue == null)
                {
                    continue;
                }

                var propertyToCast = targetInstance.GetType()
                    .GetProperty(aliasToCast, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                propertyToCast.SetValue(targetInstance, Cast(propertyToCast.PropertyType, aliasValue));
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