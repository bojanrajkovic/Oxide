using System.Collections.Generic;

namespace Oxide
{
    public static partial class ObjectExtensions
    {
        public static IEnumerable<T> Yield<T>(this T self)
        {
            yield return self;
            yield break;
        }

        public static T Identity<T>(this T self) => self;
    }
}