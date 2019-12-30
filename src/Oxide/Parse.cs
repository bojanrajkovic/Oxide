using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Oxide.Options;
using static Oxide.Results;

namespace Oxide
{
    /// <summary>
    ///     A set of extensions for parsing strings into values.
    /// </summary>
    public static class ParseExtensions
    {
        /// <summary>
        ///     A lookup table to map hash values to MethodInfo's for the purposes of caching TryParse lookups.
        /// </summary>
        static readonly Dictionary<int, MethodInfo> TryParseLut = new Dictionary<int, MethodInfo>();

        /// <summary>
        ///     A lookup table to map hash values to MethodInfo's for the purpose of caching Parse lookups.
        /// </summary>
        static readonly Dictionary<int, MethodInfo> ParseLut = new Dictionary<int, MethodInfo>();

        /// <summary>
        ///     Gets a hash for the signature of a TryParse method.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <param name="additionalParameterTypes">The types of additional parameters to TryParse.</param>
        /// <returns>A hash value for the signature of a TryParse method.</returns>
        static int GetHashForMethodSignature(Type type, params Type[] additionalParameterTypes)
        {
            unchecked {
                int hash = type.GetHashCode();
                return additionalParameterTypes.Aggregate(
                    hash,
                    (current, additionalType) => current * 31 + additionalType.GetHashCode()
                );
            }
        }

        /// <summary>
        ///     Gets a wrapper function that calls a <c>Parse</c> method on <typeparamref name="T" />.
        /// </summary>
        /// <param name="parameters">Additional parameters to pass to the <c>Parse</c> method.</param>
        /// <typeparam name="T">The type of the object on which <c>Parse</c> is being called.</typeparam>
        /// <returns>A wrapper function if one can be identified, or <see cref="None{T}"/> otherwise.</returns>
        static Option<ParseFunc<T>> GetParseFunc<T>(params object[] parameters)
        {
            var type = typeof(T);
            var paramTypes = parameters.Select(ap => ap?.GetType() ?? typeof(void)).ToArray();
            var lutHash = GetHashForMethodSignature(type, paramTypes);

            if (!ParseLut.TryGetValue(lutHash, out var parseMethod)) {
                parseMethod = FindPossibleMethodMatches(type, parameters, paramTypes, false).FirstOrDefault();
                ParseLut.Add(lutHash, parseMethod);
            }

            T ParseFunc(string s, object[] additionalParams)
            {
                var methodParams = s.Yield().Concat(additionalParams).ToArray();
                return (T)parseMethod.Invoke(null, methodParams);
            }

            return parseMethod == null ? None<ParseFunc<T>>() : Some<ParseFunc<T>>(ParseFunc);
        }

        /// <summary>
        ///     Gets a wrapper function that calls a <c>TryParse</c> method on <typeparamref name="T" />.
        /// </summary>
        /// <param name="parameters">Additional parameters to pass to the <c>TryParse</c> method.</param>
        /// <typeparam name="T">The type of the object on which <c>TryParse</c> is being called.</typeparam>
        /// <returns>A wrapper function if one can be identified, or <see cref="None{T}"/> otherwise.</returns>
        static Option<TryParseFunc<T>> GetTryParseFunc<T>(params object[] parameters)
        {
            var type = typeof(T);
            var paramTypes = parameters.Select(ap => ap?.GetType() ?? typeof(void)).ToArray();
            var lutHash = GetHashForMethodSignature(type, paramTypes);

            if (!TryParseLut.TryGetValue(lutHash, out var tryParseMethod)) {
                tryParseMethod = FindPossibleMethodMatches(type, parameters, paramTypes, true).FirstOrDefault();
                TryParseLut.Add(lutHash, tryParseMethod);
            }

            bool ParseFunc(string s, object[] additionalParams, out T result)
            {
                result = default;
                var methodParams = s.Yield().Concat(additionalParams).Concat(new object[] { null }).ToArray();

                var parsed = (bool)tryParseMethod.Invoke(null, methodParams);
                result = (T)methodParams.Last();

                return parsed;
            }

            return tryParseMethod == null ? None<TryParseFunc<T>>() : Some<TryParseFunc<T>>(ParseFunc);
        }

        /// <summary>
        ///     Gets a list of methods to search that could be valid Parse/TryParse methods for the return type.
        /// </summary>
        /// <param name="returnType">The return type to search for.</param>
        /// <param name="additionalParameters">Additional parameters provided.</param>
        /// <param name="additionalParameterTypes">The additional parameter types.</param>
        /// <param name="tryParse">Whether we're looking for a Parse or TryParse method.</param>
        /// <returns>A list of potentially valid Parse/TryParse methods.</returns>
        static IEnumerable<MethodInfo> FindPossibleMethodMatches(
            Type returnType,
            IReadOnlyCollection<object> additionalParameters,
            IReadOnlyList<Type> additionalParameterTypes,
            bool tryParse
        ) {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            bool IsMaybeUsableMethod(MethodInfo methodInfo)
            {
                try {
                    var parameters = methodInfo.GetParameters();
                    var looksLikeTryParseMethod = LooksLikeTryParseMethod(methodInfo, parameters);
                    var looksLikeRegularParseMethod = LooksLikeRegularParseMethod(methodInfo, parameters);
                    var argumentCountsMatch = ArgumentCountsMatch(methodInfo, additionalParameters, additionalParameterTypes);
                    return (tryParse ? looksLikeTryParseMethod : looksLikeRegularParseMethod) && argumentCountsMatch;
                } catch {
                    return false;
                }
            }

            bool LooksLikeRegularParseMethod(MethodInfo methodInfo, IReadOnlyList<ParameterInfo> parameters)
                => parameters[0].ParameterType == typeof(string) &&
                   methodInfo.ReturnType == returnType &&
                   methodInfo.Name == "Parse";

            bool LooksLikeTryParseMethod(MethodInfo methodInfo, IReadOnlyList<ParameterInfo> parameters)
                => parameters[0].ParameterType == typeof(string) &&
                   methodInfo.ReturnType == typeof(bool) &&
                   parameters[parameters.Count - 1].ParameterType == returnType.MakeByRefType() &&
                   methodInfo.Name == "TryParse";

            var declaredMethods = returnType.GetMethods(flags).Where(IsMaybeUsableMethod).ToList();

            // Now look in all static classes for extension methods that take the
            // type from `type` in loaded assemblies.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyTypes = assemblies.SelectMany(a => a.DefinedTypes);
            var exposedTypes = assemblyTypes.Where(t => t.IsSealed && !t.IsGenericType && !t.IsNested);
            var exposedMethods = exposedTypes.SelectMany(t => t.GetMethods(flags));
            var extensionMethods = exposedMethods.Where(methodInfo => methodInfo.IsDefined(typeof(ExtensionAttribute), false));
            var usableExtensionMethods = extensionMethods.Where(IsMaybeUsableMethod).ToList();

            return declaredMethods.Concat(usableExtensionMethods);
        }

        /// <summary>
        ///     Checks if argument counts for the given <see cref="MethodInfo"/> <paramref name="mi"/> match
        ///     the count of additional parameters and parameter types.
        /// </summary>
        /// <param name="mi">The method to check.</param>
        /// <param name="additionalParameters">The additional parameters passed.</param>
        /// <param name="additionalParameterTypes">The additional parameter types.</param>
        /// <returns><c>true</c> if the argument counts match, <c>false</c> otherwise.</returns>
        static bool ArgumentCountsMatch(
            MethodBase mi,
            IReadOnlyCollection<object> additionalParameters,
            IReadOnlyList<Type> additionalParameterTypes
        ) {
            var parameters = mi.GetParameters();
            var parameterCount = parameters.Length;

            // There's no way this matches, if it doesn't have the right
            // number of parameters.
            if (parameterCount < 1 + additionalParameters.Count) {
                return false;
            }

            var additionalTypesMatch = true;
            for (var i = 0; i < additionalParameterTypes.Count; i++) {
                // Skip the first parameter.
                var additionalParameterType = additionalParameterTypes[i];

                // Null was passed, so don't type-check it.
                if (additionalParameterType == typeof(void)) {
                    continue;
                }

                var parameter = parameters[i + 1];
                additionalTypesMatch &= parameter.ParameterType == additionalParameterType;
            }

            // If the additional types don't match, it's not match.
            return additionalTypesMatch;
        }

        /// <summary>
        ///     Converts the string representation of <typeparamref name="T" /> to its <typeparamref name="T" /> equivalent.
        /// </summary>
        /// <param name="str">A string containing a <typeparamref name="T" /> to convert.</param>
        /// <param name="additionalParameters">Additional parameters to pass to the <c>TryParse</c> function.</param>
        /// <typeparam name="T">The type of the object to parse.</typeparam>
        /// <returns>
        ///     A <see cref="Result{TResult,TError}" /> containing the result of calling a found <c>TryParse</c> method, or an
        ///     error if one could not be found, or if one was found and calling it failed.
        /// </returns>
        public static Result<T, Exception> Parse<T>(this string str, params object[] additionalParameters)
            => GetParseFunc<T>(additionalParameters).AndThen<Result<T, Exception>>(parse => {
                try {
                    return Ok<T, Exception>(parse(str, additionalParameters));
                } catch (Exception e) {
                    return Err<T, Exception>(e.InnerException);
                }
            }).Or(Err<T, Exception>(new ParseException("Could not find matching Parse method."))).Unwrap();

        /// <summary>
        ///     Converts the string representation of <typeparamref name="T" /> to its <typeparamref name="T" /> equivalent,
        ///     discarding any errors that occur.
        /// </summary>
        /// <param name="str">A string containing a <typeparamref name="T" /> to convert.</param>
        /// <param name="additionalParameters">Additional parameters to pass to the <c>TryParse</c> function.</param>
        /// <typeparam name="T">The type of the object to parse.</typeparam>
        /// <returns>
        ///     An <see cref="Option{TOption}" /> wrapping the result of the conversion, which is a <see cref="Some{T}" /> if
        ///     conversion was successful, or <see cref="None{T}" /> if it failed.
        /// </returns>
        public static Result<Option<T>, Exception> TryParse<T>(this string str, params object[] additionalParameters)
            => GetTryParseFunc<T>(additionalParameters).AndThen<Result<Option<T>, Exception>>(parseFunc => {
                try {
                    var parsed = parseFunc(str, additionalParameters, out var result);
                    return Ok<Option<T>, Exception>(parsed ? result : None<T>());
                } catch (Exception e) {
                    return Err<Option<T>, Exception>(e.InnerException);
                }
            }).Or(Err<Option<T>, Exception>(new ParseException("Could not find matching TryParse method."))).Unwrap();

        delegate bool TryParseFunc<T>(string str, object[] additionalParameters, out T result);

        delegate T ParseFunc<out T>(string str, object[] additionalParameters);
    }
}
