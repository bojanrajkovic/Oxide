using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        ///     Gets a hash for the signature of a TryParse method.
        /// </summary>
        /// <param name="type">The base type.</param>
        /// <param name="additionalParameterTypes">The types of additional parameters to TryParse.</param>
        /// <returns>A hash value for the signature of a TryParse method.</returns>
        static int GetHashForTryParse(Type type, params Type[] additionalParameterTypes)
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
        ///     Attempts to match a method to a valid, callable <c>TryParse</c> method.
        /// </summary>
        /// <param name="mi">The method to attempt matching.</param>
        /// <param name="additionalParameters">Additional parameters to the <c>TryParse</c> method.</param>
        /// <param name="additionalParameterTypes">The types of the additional parameters to the <c>TryParse</c> method.</param>
        /// <typeparam name="T">The type of the object on which <c>TryParse</c> is being called.</typeparam>
        /// <returns><c>true</c> if the method matches a valid <c>TryParse</c>, <c>false</c> otherwise.</returns>
        static bool MethodIsTryParseMatch<T>(
            MethodInfo mi,
            IReadOnlyCollection<object> additionalParameters,
            IReadOnlyList<Type> additionalParameterTypes
        ) {
            if (mi.Name != "TryParse") {
                return false;
            }

            // It needs to be static, we don't have a T instance.
            if (!mi.IsStatic) {
                return false;
            }

            // If it doesn't return bool, it's no good.
            if (mi.ReturnType != typeof(bool)) {
                return false;
            }

            var parameters = mi.GetParameters();

            // If it doesn't take a string, it's no good.
            if (parameters[0].ParameterType != typeof(string)) {
                return false;
            }

            var lastParameter = parameters.Last();

            // If the last parameter isn't T&, it's no good.
            if (lastParameter.ParameterType != typeof(T).MakeByRefType()) {
                return false;
            }

            var parameterCount = parameters.Length;

            // There's no way this matches, if it doesn't have the right
            // number of parameters.
            if (parameterCount < 2 + additionalParameters.Count) {
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
        ///     Gets a wrapper function that calls a <c>TryParse</c> method on <typeparamref name="T" />.
        /// </summary>
        /// <param name="additionalParameters">Additional parameters to pass to the <c>TryParse</c> method.</param>
        /// <typeparam name="T">The type of the object on which <c>TryParse</c> is being called.</typeparam>
        /// <returns>A wrapper function if one can be identified, or <c>null</c> otherwise.</returns>
        static Option<ParseFunc<T>> GetParseFunc<T>(params object[] additionalParameters)
        {
            var type = typeof(T);
            var additionalParameterTypes = additionalParameters.Select(
                ap => ap != null ? ap.GetType() : typeof(void)
            ).ToArray();
            var lutHash = GetHashForTryParse(type, additionalParameterTypes);

            if (!TryParseLut.TryGetValue(lutHash, out var tryParseMethod)) {
                tryParseMethod = type.GetTypeInfo().DeclaredMethods.FirstOrDefault(
                    mi => MethodIsTryParseMatch<T>(mi, additionalParameters, additionalParameterTypes)
                );
                TryParseLut.Add(lutHash, tryParseMethod);
            }

            if (tryParseMethod == null) {
                return None<ParseFunc<T>>();
            }

            bool ParseFunc(string s, object[] additionalParams, out T result)
            {
                result = default;
                var methodParams = new object[] {s}.Concat(additionalParams)
                    .Concat(new object[] {null})
                    .ToArray();

                var parsed = (bool)tryParseMethod.Invoke(null, methodParams);
                if (parsed) {
                    result = (T)methodParams.Last();
                }

                return parsed;
            }

            return Some<ParseFunc<T>>(ParseFunc);
        }

        // This is crazy, but it works. Unfortunately, it can't find extension methods
        // because those don't appear under the normal reflection mechanism.
        //
        // If we could look at all loaded assemblies, we could try and find them, but those
        // mechanisms are netstandard1.6+ and there's no sense in narrowing the supported
        // platforms of Oxide so much just for that feature.

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
                    var parsed = parse(str, additionalParameters, out var result);
                    return parsed
                        ? Ok<T, Exception>(result)
                        : Err<T, Exception>(new ParseException("TryParse returned false."));
                } catch (Exception e) {
                    return Err<T, Exception>(e.InnerException);
                }
            }).Or(Err<T, Exception>(new ParseException("Could not find matching TryParse method."))).Unwrap();

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
        public static Option<T> TryParse<T>(this string str, params object[] additionalParameters)
            => GetParseFunc<T>(additionalParameters).AndThen(parseFunc => {
                try {
                    var parsed = parseFunc(str, additionalParameters, out var result);
                    return parsed ? Some(result) : None<T>();
                } catch {
                    return None<T>();
                }
            });

        delegate bool ParseFunc<T>(string str, object[] additionalParameters, out T result);
    }
}
