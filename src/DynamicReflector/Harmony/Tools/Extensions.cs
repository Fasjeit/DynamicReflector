using System.Reflection;
using System.Text;

namespace DynamicReflector.Harmony.Tools
{
    internal static class Extensions
    {
        /// <summary>A full description of a type</summary>
        /// <param name="type">The type</param>
        /// <returns>A human readable description</returns>
        ///
        public static string FullDescription(this Type type)
        {
            if (type is null)
                return "null";

            var ns = type.Namespace;
            if (string.IsNullOrEmpty(ns) is false) ns += ".";
            var result = ns + type.Name;

            if (type.IsGenericType)
            {
                result += "<";
                var subTypes = type.GetGenericArguments();
                for (var i = 0; i < subTypes.Length; i++)
                {
                    if (result.EndsWith("<", StringComparison.Ordinal) is false)
                        result += ", ";
                    result += subTypes[i].FullDescription();
                }
                result += ">";
            }
            return result;
        }

        /// <summary>A a full description of a method or a constructor without assembly details but with generics</summary>
        /// <param name="member">The method/constructor</param>
        /// <returns>A human readable description</returns>
        ///
        public static string FullDescription(this MethodBase member)
        {
            if (member is null)
            {
                return "null";
            }
            var returnType = AccessTools.GetReturnedType(member);

            var result = new StringBuilder();
            if (member.IsStatic) _ = result.Append("static ");
            if (member.IsAbstract) _ = result.Append("abstract ");
            if (member.IsVirtual) _ = result.Append("virtual ");
            _ = result.Append($"{returnType?.FullDescription()} ");
            if (member.DeclaringType is object)
                _ = result.Append($"{member.DeclaringType.FullDescription()}::");
            var parameterString = member.GetParameters().Join(p => $"{p.ParameterType.FullDescription()} {p.Name}");
            _ = result.Append($"{member.Name}({parameterString})");
            return result.ToString();
        }

        /// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
        /// <typeparam name="T">The inner type of the enumeration</typeparam>
        /// <param name="enumeration">The enumeration</param>
        /// <param name="converter">An optional value converter (from T to string)</param>
        /// <param name="delimiter">An optional delimiter</param>
        /// <returns>The values joined into a string</returns>
        ///
        public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string?>? converter = null, string delimiter = ", ")
        {
            if (converter is null)
            {
                converter = t => t?.ToString();
            }
            return enumeration.Aggregate("", (prev, curr) => prev + (prev.Length > 0 ? delimiter : "") + converter(curr));
        }
    }
}
