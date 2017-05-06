using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using static Oxide.Options;

namespace Oxide
{
    public static class ParseExtensions
    {
        delegate bool ParseFunc<T>(string str, object[] additionalParameters, out T result);

        static Dictionary<int, MethodInfo> tryParseLut = new Dictionary<int, MethodInfo>();

        static int GetHashForTryParse(Type type, params Type[] additionalParameterTypes)
        {
            unchecked {
                int hash = type.GetHashCode();
                foreach (var additionalType in additionalParameterTypes)
                    hash = hash * 31 + additionalType.GetHashCode();
                return hash;
            }
        }

        static ParseFunc<T> GetParseFunc<T>(params object[] additionalParameters)
        {
            var type = typeof(T);
            var additionalParameterTypes = additionalParameters.Select(ap => ap != null ? ap.GetType() : typeof(void))
                                                               .ToArray();
            var lutHash = GetHashForTryParse(type, additionalParameterTypes);

            if (!tryParseLut.TryGetValue(lutHash, out var tryParseMethod)) {
                tryParseMethod = type.GetTypeInfo().DeclaredMethods
                                         .Where(mi => {
                                             if (mi.Name != "TryParse")
                                                 return false;

                                             // It needs to be static, we don't have a T instance.
                                             if (!mi.IsStatic)
                                                 return false;

                                             // If it doesn't return bool, it's no good.
                                             if (mi.ReturnType != typeof(bool))
                                                 return false;

                                             var parameters = mi.GetParameters();

                                             // If it doesn't take a string, it's no good.
                                             if (parameters[0].ParameterType != typeof(string))
                                                 return false;

                                             var lastParameter = parameters.Last();

                                             // If the last parameter isn't T&, it's no good.
                                             if (lastParameter.ParameterType != typeof(T).MakeByRefType())
                                                 return false;

                                             var parameterCount = parameters.Length;

                                             // There's no way this matches, if it doesn't have the right
                                             // number of parameters.
                                             if (parameterCount < (2 + additionalParameters.Length))
                                                 return false;

                                             var additionalTypesMatch = true;
                                             for (var i = 0; i < additionalParameterTypes.Length; i++) {
                                                 // Skip the first parameter.
                                                 var additionalParameterType = additionalParameterTypes[i];
                                                 // Null was passed, so don't type-check it.
                                                 if (additionalParameterType == typeof(void))
                                                     continue;
                                                 var parameter = parameters[i + 1];
                                                 additionalTypesMatch &= parameter.ParameterType == additionalParameterType;
                                             }

                                             // If the additional types don't match, it's not match.
                                             if (!additionalTypesMatch)
                                                 return false;

                                             return true;
                                         }).FirstOrDefault();

                tryParseLut.Add(lutHash, tryParseMethod);
            }

            if (tryParseMethod == null)
                return null;

            ParseFunc<T> del = delegate (string s, object[] additionalParams, out T result) {
                result = default(T);
                var methodParams = new object[] { s }.Concat(additionalParams)
                                                     .Concat(new object[] { null })
                                                     .ToArray();
                var parsed = (bool)tryParseMethod.Invoke(null, methodParams);
                if (parsed)
                    result = (T)methodParams.Last();
                return parsed;
            };

            return del;
        }

        public static Result<T, Exception> Parse<T>(this string str, params object[] additionalParameters)
        {
            var parseFunc = GetParseFunc<T>(additionalParameters);

            if (parseFunc == null)
                return new ParseException("Could not find matching TryParse method.");

            try {
                var parsed = parseFunc(str, additionalParameters, out var result);
                if (parsed)
                    return result;
                return new ParseException("TryParse returned false.");
            } catch (Exception e) {
                return e.InnerException;
            }
        }

        // This is crazy, but it works. Unfortunately, it can't find extension methods
        // because those don't appear under the normal reflection mechanism.
        //
        // If we could look at all loaded assemblies, we could try and find them, but those
        // mechanisms are netstandard1.6+ and there's no sense in narrowing the supported
        // platforms of Oxide so much just for that feature.
        public static Option<T> TryParse<T>(this string str, params object[] additionalParameters)
        {
            var parseFunc = GetParseFunc<T>(additionalParameters);

            if (parseFunc == null)
                return None<T>();

            try {
                var parsed = parseFunc(str, additionalParameters, out var result);
                return parsed ? Some(result) : None<T>();
            } catch {
                return None<T>();
            }
        }
    }
}
