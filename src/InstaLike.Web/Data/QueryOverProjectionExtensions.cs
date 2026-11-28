using System;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data
{
    internal static class QueryOverProjectionExtensions
    {
        /// <summary>
        /// Projects the query into a model through a constructor call or an
        /// object initializer over aliases such as <c>() => new Model(alias.Property)</c>
        /// or <c>() => new Model { Property = alias.Property }</c>.
        /// </summary>
        public static IQueryOver<TRoot, TSubType> SelectInto<TRoot, TSubType, TResult>(
            this IQueryOver<TRoot, TSubType> query,
            Expression<Func<TResult>> projection)
        {
            return SelectColumnsInto<TRoot, TSubType, TResult>(
                query,
                projection,
                column => Projections.Property(
                    System.Linq.Expressions.Expression.Lambda<Func<object>>(column)));
        }

        /// <summary>
        /// Projects the query into a model through a constructor call or an
        /// object initializer over the query root, such as <c>m => new Model(m.Property)</c>
        /// or <c>m => new Model { Property = m.Property }</c>.
        /// </summary>
        public static IQueryOver<TRoot, TSubType> SelectInto<TRoot, TSubType, TResult>(
            this IQueryOver<TRoot, TSubType> query,
            Expression<Func<TSubType, TResult>> projection)
        {
            return SelectColumnsInto<TRoot, TSubType, TResult>(
                query,
                projection,
                column => Projections.Property(
                    System.Linq.Expressions.Expression.Lambda<Func<TSubType, object>>(column, projection.Parameters)));
        }

        private static IQueryOver<TRoot, TSubType> SelectColumnsInto<TRoot, TSubType, TResult>(
            IQueryOver<TRoot, TSubType> query,
            LambdaExpression projection,
            Func<System.Linq.Expressions.Expression, IProjection> toColumn)
        {
            if (projection.Body is NewExpression ctorCall && ctorCall.Constructor is not null)
            {
                var arguments = Projections.ProjectionList();
                foreach (var argument in ctorCall.Arguments)
                {
                    arguments.Add(toColumn(AsColumn(argument)));
                }

                return query
                    .Select(arguments)
                    .TransformUsing(Transformers.AliasToBeanConstructor(ctorCall.Constructor));
            }

            if (projection.Body is MemberInitExpression initializer &&
                initializer.NewExpression.Arguments.Count == 0)
            {
                var properties = Projections.ProjectionList();
                foreach (var binding in initializer.Bindings)
                {
                    if (binding is not MemberAssignment assignment)
                    {
                        throw new ArgumentException($"The projection of {binding.Member.Name} must be a plain assignment.", nameof(projection));
                    }

                    properties.Add(toColumn(AsColumn(assignment.Expression)), assignment.Member.Name);
                }

                return query
                    .Select(properties)
                    .TransformUsing(Transformers.AliasToBean<TResult>());
            }

            throw new ArgumentException("The projection must be either a constructor call or an object initializer, such as new Model(entity.Property) or new Model { Property = entity.Property }.", nameof(projection));
        }

        /// <summary>
        /// Rewrites a projected value as the object-typed property access that
        /// <see cref="Projections.Property(Expression{Func{object}})"/> expects.
        /// </summary>
        private static UnaryExpression AsColumn(System.Linq.Expressions.Expression value)
        {
            return System.Linq.Expressions.Expression.Convert(WithoutConversions(value), typeof(object));
        }

        /// <summary>
        /// Strips the conversions the compiler wraps around a value, such as boxing
        /// or nullable lifting, so that NHibernate sees the property access itself.
        /// </summary>
        private static System.Linq.Expressions.Expression WithoutConversions(System.Linq.Expressions.Expression expression)
        {
            while (expression is UnaryExpression conversion &&
                   conversion.NodeType == ExpressionType.Convert)
            {
                expression = conversion.Operand;
            }

            return expression;
        }
    }
}
