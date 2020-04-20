using System.Collections.Generic;
using System.Linq;

namespace Oxide
{
    /// <summary>
    ///     Extensions for <see cref="IEnumerable{T}" />.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        ///     Returns the head of a collection. Equivalent to
        ///     <see cref="Enumerable.First{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>.
        /// </summary>
        /// <param name="self">The collection.</param>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <returns>The first collection element.</returns>
        public static T Head<T>(this IEnumerable<T> self) =>
            self.First();

        /// <summary>
        ///     Returns the tail of a collection.
        /// </summary>
        /// <param name="self">The collection.</param>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <returns>Everything but the first collection element.</returns>
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> self) =>
            self.Skip(1);

        /// <summary>
        ///     Returns the rest of the collection. Equivalent to <see cref="Tail{T}" />.
        /// </summary>
        /// <param name="self">The collection.</param>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <returns>Everything but the first collection element.</returns>
        public static IEnumerable<T> Rest<T>(this IEnumerable<T> self) =>
            self.Tail();
    }
}
