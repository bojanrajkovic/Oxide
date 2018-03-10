using System.Collections.Generic;
using System.Linq;

namespace Oxide
{
    public static class EnumerableExtension
    {
        public static T Head<T>(this IEnumerable<T> self) =>
            self.First();

        public static IEnumerable<T> Tail<T>(this IEnumerable<T> self) =>
            self.Skip(1);

        public static IEnumerable<T> Rest<T>(this IEnumerable<T> self) =>
            self.Tail();
    }
}