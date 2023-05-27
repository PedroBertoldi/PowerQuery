using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomGrouping
{
    public static class CustomGrouping
    {
        private static ConcurrentDictionary<string, Type> _classDictionary;
        private static AssemblyBuilder customAssembly;
        private static ModuleBuilder customModule;

        static CustomGrouping()
        {
            _classDictionary = new ConcurrentDictionary<string, Type>();
            customAssembly = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName($"Dynamic{nameof(CustomGrouping)}Assembly{Guid.NewGuid()}"), AssemblyBuilderAccess.Run);

            customModule = customAssembly
                .DefineDynamicModule($"Dynamic{nameof(CustomGrouping)}Module{Guid.NewGuid()}");
        }

        public static IQueryable<IGrouping<object, T>> GroupByClass<T>(this IQueryable<T> query, params string[] except)
        {
            return query.GroupBy(BuildFetcher<T>(except));
        }

        public static IEnumerable<IGrouping<object, T>> GroupByClass<T>(this IEnumerable<T> enumerable, params string[] except)
        {
            return enumerable.GroupBy(BuildFetcher<T>(except).Compile());
        }

        private static Expression<Func<T, object>> BuildFetcher<T>(string[] except)
        {
            var sourceType = typeof(T);
            var dynType = GetOrCreateType(sourceType, except);
            var sourceItem = Expression.Parameter(sourceType, "item");
            var bindings = dynType.GetProperties()
                .Select(p =>
                    Expression.Bind(p, Expression.PropertyOrField(sourceItem, p.Name)))
                .OfType<MemberBinding>()
                .ToArray();

            var fetcher = Expression.Lambda<Func<T, object>>(
                Expression.MemberInit(Expression.New(dynType.GetConstructor(Type.EmptyTypes)), bindings),
                sourceItem);

            return fetcher;
        }

        private static Type? GetOrCreateType(Type type, string[] strings)
        {
            var key = BuildObjKey(type, strings);
            return _classDictionary.GetOrAdd(key, (x) =>
            {
                var typeBuilder = customModule
                    .DefineType($"Dynamic{nameof(CustomGrouping)}Type{Guid.NewGuid().ToString().Replace("-", string.Empty)}", TypeAttributes.Public);

                type.GetFields()
                    .Where(f => !strings.Contains(f.Name))
                    .ToList()
                    .ForEach(f =>
                    {
                        typeBuilder.DefineField(f.Name, f.FieldType, FieldAttributes.Public);
                    });

                type.GetProperties()
                    .Where(p => !strings.Contains(p.Name))
                    .ToList()
                    .ForEach(p =>
                    {
                        var field = typeBuilder.DefineField("m_" + p.Name, p.PropertyType, FieldAttributes.Private);
                        var prop = typeBuilder.DefineProperty(p.Name, PropertyAttributes.HasDefault, p.PropertyType, null);
                        var getter = typeBuilder.DefineMethod("get_" + p.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, p.PropertyType, Type.EmptyTypes);
                        var getterIl = getter.GetILGenerator();
                        getterIl.Emit(OpCodes.Ldarg_0);
                        getterIl.Emit(OpCodes.Ldfld, field);
                        getterIl.Emit(OpCodes.Ret);

                        var setter = typeBuilder.DefineMethod("set_" + p.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { p.PropertyType });
                        var setterIl = setter.GetILGenerator();
                        setterIl.Emit(OpCodes.Ldarg_0);
                        setterIl.Emit(OpCodes.Ldarg_1);
                        setterIl.Emit(OpCodes.Stfld, field);
                        setterIl.Emit(OpCodes.Ret);

                        prop.SetSetMethod(setter);
                        prop.SetGetMethod(getter);
                        //typeBuilder.DefineField(p.Name, p.PropertyType, FieldAttributes.Public);
                    });

                return typeBuilder.CreateType(); ;
            });
        }

        private static string BuildObjKey(Type type, string[] strings)
        {
            return $"{type.FullName}|{string.Join(";", strings)}";
        }
    }
}
