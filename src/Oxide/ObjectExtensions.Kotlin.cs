using System;
using static Oxide.Options;

namespace Oxide
{
    public static partial class ObjectExtensions
    {
        public static TResult Let<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> block)
            => block(self);

        public static TReceiver Also<TReceiver>(this TReceiver self, Action<TReceiver> block)
        {
            block(self);
            return self;
        }

        public static TResult Use<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> block)
            where TReceiver : IDisposable
        {
            using (self) {
                return block(self);
            }    
        }

        public static (T a, U b) To<T, U>(this T a, U b) => (a, b);

        public static Option<T> TakeUnless<T>(this T self, Func<T, bool> predicate)
            => predicate(self) ? None<T>() : self;

        public static Option<T> TakeIf<T>(this T self, Func<T, bool> predicate)
            => predicate(self) ? self : None<T>();

        
    }
}