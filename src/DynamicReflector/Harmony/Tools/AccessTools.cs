using System.Reflection;

namespace DynamicReflector.Harmony.Tools
{
    /// <summary>A helper class for reflection related functions</summary>
    ///
    internal static class AccessTools
    {
        /// <summary>Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level</summary>
        ///
        // Note: This should a be const, but changing from static (readonly) to const breaks binary compatibility.
        public static readonly BindingFlags all = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        /// <summary>Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level but only within the current type</summary>
        ///
        // Note: This should a be const, but changing from static (readonly) to const breaks binary compatibility.
        public static readonly BindingFlags allDeclared = all | BindingFlags.DeclaredOnly;

        /// <summary>Given a type, returns the first inner type matching a recursive search by name</summary>
        /// <param name="type">The class/type to start searching at</param>
        /// <param name="name">The name of the inner type (case sensitive)</param>
        /// <returns>The inner type or null if type/name is null or if a type with that name cannot be found</returns>
        ///
        public static Type? Inner(Type type, string name)
        {
            if (type is null || name is null)
            {
                return null;
            }
            return FindIncludingBaseTypes(type, t => t.GetNestedType(name, all));
        }

        /// <summary>Applies a function going up the type hierarchy and stops at the first non-<c>null</c> result</summary>
        /// <typeparam name="T">Result type of func()</typeparam>
        /// <param name="type">The class/type to start with</param>
        /// <param name="func">The evaluation function returning T</param>
        /// <returns>The first non-<c>null</c> result, or <c>null</c> if no match</returns>
        /// <remarks>
        /// The type hierarchy of a class or value type (including struct) does NOT include implemented interfaces,
        /// and the type hierarchy of an interface is only itself (regardless of whether that interface implements other interfaces).
        /// The top-most type in the type hierarchy of all non-interface types (including value types) is <see cref="object"/>.
        /// </remarks>
        ///
        public static T? FindIncludingBaseTypes<T>(Type type, Func<Type, T?> func) where T : class
        {
            while (true)
            {
                var result = func(type);
                if (result is object)
                {
                    return result;
                }
                type = type.BaseType;
                if (type is null)
                {
                    return null;
                }
            }
        }

        /// <summary>Gets the names of all fields that are declared in a type</summary>
        /// <param name="type">The declaring class/type</param>
        /// <returns>A list of field names</returns>
        ///
        public static List<string> GetFieldNames(Type type)
        {
            if (type is null)
            {
                return new List<string>();
            }
            return GetDeclaredFields(type).Select(f => f.Name).ToList();
        }

        /// <summary>Gets the names of all fields that are declared in the type of the instance</summary>
        /// <param name="instance">An instance of the type to search in</param>
        /// <returns>A list of field names</returns>
        ///
        public static List<string> GetFieldNames(object instance)
        {
            if (instance is null)
            {
                return new List<string>();
            }
            return GetFieldNames(instance.GetType());
        }

        /// <summary>Gets reflection information for all declared fields</summary>
        /// <param name="type">The class/type where the fields are declared</param>
        /// <returns>A list of fields</returns>
        ///
        public static List<FieldInfo> GetDeclaredFields(Type type)
        {
            if (type is null)
            {
                return new List<FieldInfo>();
            }
            return type.GetFields(allDeclared).ToList();
        }

        public static List<string> GetPropertyNames(Type type)
        {
            if (type is null)
            {
                return new List<string>();
            }
            return GetDeclaredProperties(type).Select(f => f.Name).ToList();
        }

        /// <summary>Gets the names of all properties that are declared in the type of the instance</summary>
        /// <param name="instance">An instance of the type to search in</param>
        /// <returns>A list of property names</returns>
        ///
        public static List<string> GetPropertyNames(object instance)
        {
            if (instance is null)
            {
                return new List<string>();
            }
            return GetPropertyNames(instance.GetType());
        }

        /// <summary>Gets reflection information for all declared properties</summary>
        /// <param name="type">The class/type where the properties are declared</param>
        /// <returns>A list of properties</returns>
        ///
        public static List<PropertyInfo> GetDeclaredProperties(Type type)
        {
            if (type is null)
            {
                return new List<PropertyInfo>();
            }
            return type.GetProperties(allDeclared).ToList();
        }

        /// <summary>Returns an array containing the type of each object in the given array</summary>
        /// <param name="parameters">An array of objects</param>
        /// <returns>An array of types or an empty array if parameters is null (if an object is null, the type for it will be object)</returns>
        ///
        public static Type[] GetTypes(object[] parameters)
        {
            if (parameters is null) return new Type[0];
            return parameters.Select(p => p is null ? typeof(object) : p.GetType()).ToArray();
        }

        /// <summary>Gets the names of all method that are declared in a type</summary>
        /// <param name="type">The declaring class/type</param>
        /// <returns>A list of method names</returns>
        ///
        public static List<string> GetMethodNames(Type type)
        {
            if (type is null)
            {
                return new List<string>();
            }
            return GetDeclaredMethods(type).Select(m => m.Name).ToList();
        }

        /// <summary>Gets reflection information for all declared methods</summary>
        /// <param name="type">The class/type where the methods are declared</param>
        /// <returns>A list of methods</returns>
        ///
        public static List<MethodInfo> GetDeclaredMethods(Type type)
        {
            if (type is null)
            {
                return new List<MethodInfo>();
            }
            return type.GetMethods(allDeclared).ToList();
        }

        /// <summary>Gets a type by name. Prefers a full name with namespace but falls back to the first type matching the name otherwise</summary>
        /// <param name="name">The name</param>
        /// <returns>A type or null if not found</returns>
        ///
        public static Type TypeByName(string name)
        {
            var type = Type.GetType(name, false);
            if (type is null)
            {
                type = AllTypes().FirstOrDefault(t => t.FullName == name);
            }
            if (type is null)
            {
                type = AllTypes().FirstOrDefault(t => t.Name == name);
            }
            return type;
        }

        /// <summary>Enumerates all successfully loaded types in the current app domain, excluding visual studio assemblies</summary>
        /// <returns>An enumeration of all <see cref="Type"/> in all assemblies, excluding visual studio assemblies</returns>
        public static IEnumerable<Type> AllTypes()
        {
            return AllAssemblies().SelectMany(a => GetTypesFromAssembly(a));
        }

        /// <summary>Enumerates all assemblies in the current app domain, excluding visual studio assemblies</summary>
        /// <returns>An enumeration of <see cref="Assembly"/></returns>
        public static IEnumerable<Assembly> AllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);
        }

        /// <summary>Gets all successfully loaded types from a given assembly</summary>
        /// <param name="assembly">The assembly</param>
        /// <returns>An array of types</returns>
        /// <remarks>
        /// This calls and returns <see cref="Assembly.GetTypes"/>, while catching any thrown <see cref="ReflectionTypeLoadException"/>.
        /// If such an exception is thrown, returns the successfully loaded types (<see cref="ReflectionTypeLoadException.Types"/>,
        /// filtered for non-null values).
        /// </remarks>
        ///
        public static Type[] GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type is object).ToArray();
            }
        }

        /// <summary>Gets the return type of a method or constructor</summary>
        /// <param name="methodOrConstructor">The method/constructor</param>
        /// <returns>The return type</returns>
        ///
        public static Type? GetReturnedType(MethodBase methodOrConstructor)
        {
            if (methodOrConstructor is null)
            {
                return null;
            }
            var constructor = methodOrConstructor as ConstructorInfo;
            if (constructor is object) return typeof(void);
            return ((MethodInfo)methodOrConstructor).ReturnType;
        }

        /// <summary>Gets the type of any class member of</summary>
        /// <param name="member">A member</param>
        /// <returns>The class/type of this member</returns>
        ///
        public static Type? GetUnderlyingType(this MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                _ => throw new ArgumentException("Member must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
            };
        }

        /// <summary>Calculates a combined hash code for an enumeration of objects</summary>
        /// <param name="objects">The objects</param>
        /// <returns>The hash code</returns>
        ///
        public static int CombinedHashCode(IEnumerable<object> objects)
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;
            var i = 0;
            foreach (var obj in objects)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ obj.GetHashCode();
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ obj.GetHashCode();
                ++i;
            }
            return hash1 + (hash2 * 1566083941);
        }

        /// <summary>Gets the reflection information for a method by searching the type and all its super types</summary>
        /// <param name="type">The class/type where the method is declared</param>
        /// <param name="name">The name of the method (case sensitive)</param>
        /// <param name="parameters">Optional parameters to target a specific overload of the method</param>
        /// <param name="generics">Optional list of types that define the generic version of the method</param>
        /// <returns>A method or null when type/name is null or when the method cannot be found</returns>
        ///
        public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
        {
            if (type is null)
            {
                return null;
            }
            if (name is null)
            {
                return null;
            }
            MethodInfo result;
            var modifiers = new ParameterModifier[] { };
            if (parameters is null)
            {
                try
                {
                    result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all));
                }
                catch (AmbiguousMatchException ex)
                {
                    result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, new Type[0], modifiers));
                    if (result is null)
                    {
                        throw new AmbiguousMatchException($"Ambiguous match in Harmony patch for {type}:{name}", ex);
                    }
                }
            }
            else
            {
                result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, parameters, modifiers));
            }

            if (result is null)
            {
                return null;
            }

            if (generics is object) result = result.MakeGenericMethod(generics);
            return result;
        }
    }
}