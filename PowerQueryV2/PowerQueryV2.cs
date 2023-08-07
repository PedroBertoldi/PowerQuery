using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PowerQueryV2
{
    public static class PowerQueryV2
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        private static MethodInfo tostringMethod = typeof(object).GetMethod(nameof(object.ToString));
        private static MethodInfo tollowerMethod = typeof(string).GetMethods()
            .Single(x => x.Name == nameof(string.ToLower) && x.GetParameters().Length == 0);
        private static MethodInfo asQueryblaMethod = typeof(Queryable).GetMethods()
            .Where(m => m.Name == nameof(Queryable.AsQueryable))
            .Single(m => m.IsGenericMethod);
        private static MethodInfo anyMethod = typeof(Queryable).GetMethods()
            .Where(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Length == 2)
            .First();

        private static Type[] treatAsPrimitives = new Type[]
        {
            typeof(string), typeof(DateTime), typeof(DateTimeOffset)
        };

        public static IQueryable<T> QueryV2<T>(this IQueryable<T> query, string term, PQConfig config = null)
            where T : class
        {
            if (string.IsNullOrEmpty(term))
            {
                return query;
            }

            if (config is null)
            {
                config = new PQConfig();
            }

            var type = typeof(T);
            var param = Expression.Parameter(type, "p0");
            var exp = BuildQueryForType(type, param, term, config);
            if (exp.CanReduce)
            {
                exp = exp.Reduce();
            }

            var lambda = Expression.Lambda<Func<T, bool>>(exp, param);

            return query.Where(lambda);
        }

        private static Expression BuildQueryForType(Type type, ParameterExpression parameter, string term, PQConfig config)
        {
            var temp = new PQTemp
            {
                CurrentLevel = 0,
                Path = string.Empty,
                TermArray = config.SeparateString
                    ? term.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim().ToLower().AsParam()).ToArray()
                    : new Expression[] { term.Trim().ToLower().AsParam() }
            };

            return BuildQueryForObject(type, parameter, config, temp);
        }

        private static Expression BuildQueryForObject(Type type, Expression inputParam, PQConfig config, PQTemp temp)
        {
            var properties = type.GetProperties()
                .Where(p => !config.Exclude.Contains(temp.Path + p.Name))
                .ToList();

            var expList = new List<Expression>();

            foreach (var property in properties)
            {
                TypeCheckAndExecute(property.PropertyType,
                    isPrimitive: () =>
                    {
                        var exp = temp.TermArray
                            .Select(x => BuildForPrimitive(Expression.Property(inputParam, property), property.PropertyType, x))
                            .Aggregate(Expression.AndAlso);

                        expList.Add(exp);
                    },
                    isEnumerable: () =>
                    {
                        var exp = BuildForIEnumerable(Expression.Property(inputParam, property), property.PropertyType, config, temp);
                        expList.Add(exp);
                    });
            }

            return expList.Aggregate(Expression.OrElse);
        }

        private static Expression BuildForIEnumerable(Expression input, Type type, PQConfig config, PQTemp temp)
        {
            var innerType = type.GetGenericArguments().First();
            Expression exp = input;

            if (type != typeof(IQueryable))
            {
                exp = Expression.Call(null, asQueryblaMethod.MakeGenericMethod(innerType), exp);
            }

            var param = Expression.Parameter(innerType);
            Expression innerExp = default;
            TypeCheckAndExecute(innerType,
                isPrimitive: () =>
                {
                    innerExp = temp.TermArray.Select(x => BuildForPrimitive(param, innerType, x))
                        .Aggregate(Expression.AndAlso);
                },
                isObject: () =>
                {
                    innerExp = BuildQueryForObject(innerType, param, config, temp);
                });

            var lambda = Expression.Lambda(innerExp, param);
            exp = Expression.Call(null, anyMethod.MakeGenericMethod(innerType), exp, lambda);

            return exp;
        }

        /// <summary>
        /// Build an comparission expression for a primitive type.  <br />
        /// x => x.[propertyInfo] != null  <br />
        ///     ? x.[propertyInfo] == [Term]  <br />
        ///     : false
        /// </summary>
        /// <param name="input">Input parameter.</param>
        /// <param name="propertyInfo">Property to be compared.</param>
        /// <param name="term">Term to be compared.</param>
        /// <returns>Expression.</returns>
        private static Expression BuildForPrimitive(Expression input, Type type, Expression term)
        {
            //var prop = Expression.Property(input, propertyInfo);
            Expression exp = input;
            if (type != typeof(string))
            {
                exp = Expression.Call(exp, tostringMethod);
            }

            exp = Expression.Call(exp, tollowerMethod);
            exp = Expression.Call(exp, containsMethod, term);

            if (Nullable.GetUnderlyingType(type) != null || treatAsPrimitives.Contains(type))
            {
                var condition = Expression.Equal(input, Expression.Constant(null));
                exp = Expression.Condition(condition, Expression.Constant(false), exp);
            }

            if (exp.CanReduce)
            {
                exp = exp.Reduce();
            }

            return exp;
        }

        /// <summary>
        /// Convert a string in a parameter, this is utilized to prevent sql injection.
        /// </summary>
        /// <param name="term">string to be parse as object.</param>
        /// <returns>Paramaret expression as Expression.</returns>
        private static Expression AsParam(this string term)
        {
            return Expression.Property(Expression.Constant(new { value = term }), "value");
        }

        private static void TypeCheckAndExecute(Type type, Action isPrimitive = null, Action isEnumerable = null, Action isObject = null)
        {
            if (isPrimitive != null && (type.IsPrimitive || treatAsPrimitives.Contains(type)))
            {
                isPrimitive();
            }
            else if (isEnumerable != null && (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type)))
            {
                isEnumerable();
            }
            else if (isObject != null)
            {
                isObject();
            }
        }
    }
}
