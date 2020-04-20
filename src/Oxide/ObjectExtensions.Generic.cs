using System.Collections.Generic;
using System.Linq;

namespace Oxide
{
    /// <summary>
    ///     Generic object extensions.
    /// </summary>
    public static partial class ObjectExtensions
    {
        /// <summary>
        ///     Yields the given object as a single-item <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <param name="self">The object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>A single-item enumerable containing the object.</returns>
        public static IEnumerable<T> Yield<T>(this T self) => new[] {self};

        /// <summary>
        ///     Returns the passed object.
        /// </summary>
        /// <remarks>
        ///     This method can be useful as part of LINQ compositions, especially involving
        ///     <see cref="Enumerable.Where{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,bool})" />
        ///     .
        /// </remarks>
        /// <param name="self">The object.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object.</returns>
        public static T Identity<T>(this T self) => self;
    }
}
