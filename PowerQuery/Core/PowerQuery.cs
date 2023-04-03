using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Text;
using PowerQuery.Models;

namespace PowerQuery.Core
{
    public static class PowerQuery
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        private static MethodInfo tostringMethod = typeof(object).GetMethod(nameof(object.ToString));
        private static MethodInfo tollowerMethod = typeof(string).GetMethods()
            .Single(x => x.Name == nameof(string.ToLower) && x.GetParameters().Length == 0);

        /// <summary>
        /// Build a query expression for an parameter.
        /// </summary>
        /// <param name="parameter">Parameter for the expression.</param>
        /// <param name="term">Term to be search.</param>
        /// <param name="propertyInfo">Base property.</param>
        /// <param name="config">Configuration.</param>
        /// <returns></returns>
        private static Expression BuildPropertyExpression(Expression parameter, string term, PropertyInfo propertyInfo, PowerQueryConfig config)
        {
            Expression expression = Expression.Property(parameter, propertyInfo);

            if (propertyInfo.PropertyType.IsPrimitive)
            {
                expression = Expression.Call(expression, tostringMethod);
            }

            if (!config.CaseSensitive)
            {
                expression = Expression.Call(expression, tollowerMethod);
            }

            return Expression.Call(expression, containsMethod, Expression.Constant(term));
        }

        private static Expression BuildExpressionByType(Expression parameterExpression, Type type, string term, PowerQueryConfig config, int level = 1, Type parrentType = null, string parrentName = null)
        {
            //Early exit if max expassion level was reached.
            if (level > config.MaxExpanssionLevel)
            {
                return null;
            }

            var typeExpressions = new List<Expression>();
            foreach (var property in type.GetProperties()
                .Where(x => (parrentType == null || parrentType != x.PropertyType) 
                    && !config.ExcludeByType.Contains(x.PropertyType) 
                    && !config.ExcludeByName.Contains((parrentName ?? string.Empty) + x.Name)))
            {
                //If simple property.
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    typeExpressions.Add(BuildPropertyExpression(parameterExpression, term, property, config));
                }
                else
                {
                    //Works with arrays and IEnumerables.
                    if (config.FilterInList
                        && property.PropertyType.IsGenericType 
                        && (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) || property.PropertyType.IsArray))
                    {
                        var innerType = property.PropertyType.IsGenericType
                            ? property.PropertyType.GetGenericArguments().First()
                            : property.PropertyType;

                        var param = Expression.Parameter(innerType, $"p{level}");
                        var predicate = BuildExpressionByType(
                                param,
                                innerType,
                                term,
                                config,
                                ++level,
                                type,
                                (parrentName ?? string.Empty) + property.Name + "."
                            );

                        if (predicate == null)
                        {
                            continue;
                        }

                        var asQueryblaMethod = typeof(Queryable).GetMethods()
                            .Where(m => m.Name == nameof(Queryable.AsQueryable))
                            .Single(m => m.IsGenericMethod)
                            .MakeGenericMethod(innerType);

                        var anyMethod = typeof(Queryable).GetMethods()
                            .Where(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Length == 2)
                            .First()
                            .MakeGenericMethod(innerType);

                        var temp = Expression.Call(
                                anyMethod,
                                Expression.Call(
                                        null,
                                        asQueryblaMethod,
                                        Expression.Property(parameterExpression, property)
                                    ),
                                Expression.Lambda(predicate, param)
                            );
                        typeExpressions.Add(temp);
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        //work with user classes.
                        var temp = BuildExpressionByType(
                                Expression.Property(parameterExpression, property),
                                property.PropertyType,
                                term,
                                config,
                                ++level,
                                type,
                                (parrentName ?? string.Empty) + property.Name + "."
                            );

                        typeExpressions.Add(temp);
                    }
                }
            }

            return typeExpressions.Where(x => x != null)
                .Aggregate((result, current) => Expression.OrElse(result, current));
        }

        public static IQueryable<T> SearchForTermComplex<T>(this IQueryable<T> source, string term, Action<PowerQueryConfig> action)
        {
            var config = new PowerQueryConfig();
            action.Invoke(config);

            return SearchForTermComplex<T>(source, term, config);

        }

        public static IQueryable<T> SearchForTermComplex<T>(this IQueryable<T> source, string term, PowerQueryConfig config = null)
        {
            if (string.IsNullOrEmpty(term))
            {
                return source;
            }

            if (config == null)
            {
                config = new PowerQueryConfig();
            }

            if (!config.CaseSensitive)
            {
                term = term.ToLower();
            }

            var firstType = typeof(T);
            var firstParam = Expression.Parameter(firstType, "p0");
            var expression = BuildExpressionByType(firstParam, firstType, term, config);

            var lambda = Expression.Lambda<Func<T, bool>>(expression, firstParam);

            return source.Where(lambda);
        }
    }
}
