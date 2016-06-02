using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace DynamicObjectMapper
{
    public class DynamicObjectMapperIL<T> : IDynamicObjectMapper<T> where T : new()
    {
        private delegate IDictionary<string, object> MapperDelegate(T objectSource);

        private static readonly ConcurrentDictionary<int, MapperDelegate> MapperCache = new ConcurrentDictionary<int, MapperDelegate>();

        private static readonly AssemblyBuilder AssemblyBuilder;
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly string AssemblyName = $"DynamicObjectMapper-{typeof(T).FullName.Replace(".", "_")}";

        static DynamicObjectMapperIL()
        {
            var assemblyName = new AssemblyName(AssemblyName);
            var appDomain = System.Threading.Thread.GetDomain();

            AssemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(assemblyName.Name, AssemblyName + ".dll");
        }

        public static void DumpAssembly()
        {
            AssemblyBuilder.Save(AssemblyName + ".dll");
        }

        public IDictionary<string, object> Map(T source, params string[] properties)
        {
            int hashCode = properties.Aggregate(0, (current, property) => current ^ property.ToLowerInvariant().GetHashCode());

            var mapper = MapperCache.GetOrAdd(hashCode, code => this.ConstructObjectMapper(code, properties));

            return mapper(source);
        }

        private MapperDelegate ConstructObjectMapper(int hashCode, params string[] properties)
        {
            var methodName = "__" + typeof(T).Name + "_mapper_" + hashCode;
            Type returnType = typeof(Dictionary<string, object>);

            var typeBuilder = ModuleBuilder.DefineType(typeof(T).Name + "_mapper_class_" + hashCode, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed);
            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, new[] { typeof(T) });

            var il = methodBuilder.GetILGenerator();

            // Return value
            il.DeclareLocal(returnType);

            var constructor = returnType.GetConstructor(Type.EmptyTypes);

            // Creates a new dictionary, storing it at location 0
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc_0);

            var dictionaryAddMethod = returnType.GetMethod("Add");

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int complexPropertyCounter = 0;
            foreach (var property in properties)
            {
                if (visited.Contains(property))
                    continue;

                if (property.Contains('.'))
                {
                    int idx = property.IndexOf('.');
                    var propertyName = property.Substring(0, idx);
                    var associatedProperties = properties
                        .Where(p => p.StartsWith(propertyName + ".", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    var subPropertyNames = associatedProperties
                        .Select(p => p.Substring(idx + 1, p.Length - (idx + 1)))
                        .ToArray();

                    this.EmitIlForComplexProperty(il, propertyName, subPropertyNames, dictionaryAddMethod, ++complexPropertyCounter);
                    foreach (var pr in associatedProperties)
                        visited.Add(pr);
                }
                else
                {
                    this.EmitIlForProperty(il, property, dictionaryAddMethod);
                    visited.Add(property);
                }
            }

            // Load the dictionary
            il.Emit(OpCodes.Ldloc_0);

            // Return from the dynamic method
            il.Emit(OpCodes.Ret);

            typeBuilder.CreateType();

            var method = typeBuilder.GetMethods().First(m => m.Name == methodName);
            return (MapperDelegate)method.CreateDelegate(typeof(MapperDelegate));
        }

        private void EmitIlForComplexProperty(ILGenerator il, string property, string[] subProperties, MethodInfo dictionaryAddMethod, int complexPropertyCounter)
        {
            var propertyInfo = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var getMethod = propertyInfo.GetGetMethod();

            var dynamicMapperType = typeof(DynamicObjectMapperIL<>).MakeGenericType(propertyInfo.PropertyType);
            var dynamicMapperCtor = dynamicMapperType.GetConstructor(Type.EmptyTypes);
            var mapMethodInfo = dynamicMapperType.GetMethod(nameof(this.Map));

            il.DeclareLocal(typeof(IDictionary<string, object>));

            il.Emit(OpCodes.Newobj, dynamicMapperCtor);

            if (typeof(T).IsValueType)
                il.Emit(OpCodes.Ldarga_S, 00);
            else
                il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Callvirt, getMethod);

            // create array based on subProperties argument
            il.Emit(OpCodes.Ldc_I4_S, subProperties.Length);
            il.Emit(OpCodes.Newarr, typeof(string));
            il.Emit(OpCodes.Dup);

            for (int i = 0; i < subProperties.Length; i++)
            {
                il.Emit(OpCodes.Ldc_I4_S, i);
                il.Emit(OpCodes.Ldstr, subProperties[i]);
                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Callvirt, mapMethodInfo);
            il.Emit(OpCodes.Stloc_S, complexPropertyCounter);

            il.Emit(OpCodes.Ldloc_0); // original dict
            il.Emit(OpCodes.Ldstr, propertyInfo.Name);
            il.Emit(OpCodes.Ldloc_S, complexPropertyCounter);
            il.Emit(OpCodes.Callvirt, dictionaryAddMethod);
        }

        private void EmitIlForProperty(ILGenerator il, string property, MethodInfo dictionaryAddMethod)
        {
            var propertyInfo = typeof(T).GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(property), $"'{property}' is not a supported property of the '{typeof(T).Name}' type");
            }

            var getMethod = propertyInfo.GetGetMethod();

            // Load the dictionary from location 0
            il.Emit(OpCodes.Ldloc_0);

            // Push the name of the property onto the stack
            il.Emit(OpCodes.Ldstr, propertyInfo.Name);

            // Push the Geocache object onto the stack (the argument to this dynamic method)
            if (typeof(T).IsValueType)
                il.Emit(OpCodes.Ldarga_S, 00);
            else
                il.Emit(OpCodes.Ldarg_0);

            // Execute the getter for the property of the Geocache object
            il.Emit(OpCodes.Callvirt, getMethod);

            // If the property is a class, drill into the properties
            if (propertyInfo.PropertyType.IsClass)
            {
                // Preserve the class hierarchy
            }

            // If the property is a value type, box it
            if (propertyInfo.PropertyType.IsValueType)
            {
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            // Add the value of the property to the dictionary
            il.Emit(OpCodes.Callvirt, dictionaryAddMethod);
        }

        public async Task<object> ReshapeObjectAsync(string properties, T originalObject)
        {
            throw new NotImplementedException();
        }
    }
}
