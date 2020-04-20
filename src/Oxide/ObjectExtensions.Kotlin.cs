using System;

using static Oxide.Options;

namespace Oxide
{
    public static partial class ObjectExtensions
    {
        /// <summary>
        ///     Calls the specified function <paramref name="block" /> with <paramref name="self" /> as its argument,
        ///     and returns the result.
        /// </summary>
        /// <remarks>
        ///     Let can be used instead of a direct call to avoid temporary variables in scope or clean up operations
        ///     against possibly-null values. For example, consider <code>maybeNull?.Let(...);</code> vs.
        ///     <code>if (maybeNull != null) { ... }</code>.
        /// </remarks>
        /// <param name="self">The object itself.</param>
        /// <param name="block">The function to call.</param>
        /// <typeparam name="TReceiver">The type of the receiving object, <paramref name="self" />.</typeparam>
        /// <typeparam name="TResult">The type of the value returned from <paramref name="block" />.</typeparam>
        /// <returns>The result of <paramref name="block" /> with <paramref name="self" /> as the argument.</returns>
        public static TResult Let<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> block)
            => block(self);

        /// <summary>
        ///     Calls the specified function <paramref name="block" /> with <paramref name="self" /> as its argument,
        ///     and returns <paramref name="self" />.
        /// </summary>
        /// <remarks>
        ///     Also can be used to perform actions that take the object as an argument, but can possibly be removed
        ///     without breaking program logic, such as logging, debug printing, etc..
        /// </remarks>
        /// <param name="self">The object itself.</param>
        /// <param name="block">The function to call.</param>
        /// <typeparam name="TReceiver">The type of the object.</typeparam>
        /// <returns><paramref name="self" />.</returns>
        public static TReceiver Also<TReceiver>(this TReceiver self, Action<TReceiver> block)
        {
            block(self);
            return self;
        }

        /// <summary>
        /// Calls a method, passing an <see cref="IDisposable"/> <paramref name="self"/> to the function <paramref name="block"/>, and then disposes the object.
        /// </summary>
        /// <param name="self">The disposable object.</param>
        /// <param name="block">The function to execute.</param>
        /// <typeparam name="TReceiver">The type of the object.</typeparam>
        /// <typeparam name="TResult">The type of the result from <paramref name="block"/>.</typeparam>
        /// <returns>The result of <paramref name="block"/>.</returns>
        public static TResult Use<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> block)
            where TReceiver : IDisposable
        {
            using (self) {
                return block(self);
            }
        }

        /// <summary>
        /// Converts a pair of objects <paramref name="a"/> and <paramref name="b"/> into a tuple containing both.
        /// </summary>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        /// <typeparam name="TA">The type of <paramref name="a"/>.</typeparam>
        /// <typeparam name="TB">The type of <paramref name="b"/>.</typeparam>
        /// <returns>A <see cref="ValueTuple{T1,T2}"/> containing both <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static (TA a, TB b) To<TA, TB>(this TA a, TB b) => (a, b);

        /// <summary>
        /// Evaluates <paramref name="self"/> against <paramref name="predicate"/>, and returns an optional value
        /// depending on whether the predicate returns <c>true</c> or <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Can be used to start or break option chains depending on some predicate.
        /// </remarks>
        /// <param name="self">The object itself.</param>
        /// <param name="predicate">The predicate against which to evaluate the object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns><see cref="None{T}"/> if the object matches the predicate, or the object otherwise.</returns>
        public static Option<T> TakeUnless<T>(this T self, Func<T, bool> predicate)
            => predicate(self) ? None<T>() : self;

        /// <summary>
        /// Evaluates <paramref name="self"/> against <paramref name="predicate"/>, and returns an optional value
        /// depending on whether the predicate returns <c>true</c> or <c>false</c>.
        /// </summary>
        /// <remarks>
        /// The inverse of <see cref="TakeUnless{T}"/>.
        /// </remarks>
        /// <param name="self">The object itself.</param>
        /// <param name="predicate">The predicate against which to evaluate the object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object, if it matches the predicate, or <see cref="None{T}"/> otherwise.</returns>
        public static Option<T> TakeIf<T>(this T self, Func<T, bool> predicate)
            => predicate(self) ? self : None<T>();
    }
}
