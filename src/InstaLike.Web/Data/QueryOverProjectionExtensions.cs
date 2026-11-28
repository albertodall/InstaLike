using System;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data
{
    internal static class QueryOverProjectionExtensions
    {
        public static IQueryOver<TRoot, TSubType> SelectInto<TRoot, TSubType, TResult>(
            this IQueryOver<TRoot, TSubType> query,
            Expression<Func<TResult>> projection)
        {
            if (projection.Body is not MemberInitExpression initializer)
            {
                throw new ArgumentException("The projection must be an object initializer, such as () => new Model { Property = alias.Property }.", nameof(projection));
            }

            var columns = Projections.ProjectionList();
            foreach (var binding in initializer.Bindings)
            {
                if (binding is not MemberAssignment assignment)
                {
                    throw new ArgumentException($"The projection of {binding.Member.Name} must be a plain assignment.", nameof(projection));
                }

                var column = System.Linq.Expressions.Expression.Lambda<Func<object>>(System.Linq.Expressions.Expression.Convert(WithoutConversions(assignment.Expression), typeof(object)));
                columns.Add(Projections.Property(column), assignment.Member.Name);
            }

            return query
                .Select(columns)
                .TransformUsing(Transformers.AliasToBean<TResult>());
        }

        private static System.Linq.Expressions.Expression WithoutConversions(System.Linq.Expressions.Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }
    }
}
