using System.Collections.Generic;

namespace Oxide
{
    public static partial class ObjectExtensions
    {
        public static IEnumerable<T> Yield<T>(this T self) => new [] {self};

        public static T Identity<T>(this T self) => self;
    }
}
