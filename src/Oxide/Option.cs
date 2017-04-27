using System;
using System.Threading.Tasks;

using static Oxide.Options;

namespace Oxide
{
    public static class Options
    {
        public static Option<T> None<T>() => new None<T>();
        public static Option<T> Some<T>(T value) => new Some<T>(value);
        public static async Task<T> Unwrap<T>(this Task<Option<T>> self) => (await self).Unwrap();
    }

    public abstract class Option
    {
        public bool IsNone => !hasValue;
        public bool IsSome => !IsNone;

        protected bool hasValue;
    }

    [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(OptionAsyncMethodBuilder<>))]
    public abstract class Option<T> : Option, IEquatable<Option<T>>
    {
        static readonly string UnwrapMessage
            = $"Tried to unwrap a None<{typeof(T)}>!";

        T value;

        // We really want this to be protected _and_ internal, but C# protected internal
        // means protected _or_ internal, so, just go with this for now. Consider modifying
        // the IL for real insanity. What would be better is if C# had sealed-if-not-internal.
        internal Option() { }
        internal Option(T value) { hasValue = true; this.value = value; }

        public bool Equals(Option<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (hasValue != other.hasValue)
                return false;

            return !hasValue || Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Option<T>)
                return Equals((Option<T>)obj);
            return false;
        }

        public static bool operator ==(Option<T> left, Option<T> right)
            => left.Equals(right);
        public static bool operator !=(Option<T> left, Option<T> right)
            => !left.Equals(right);
        public static implicit operator Option<T>(T value)
            => value == null ? None<T>() : Some(value);

        public T Expect(string msg)
            => IsSome ? value : throw new Exception(msg);
        public T Unwrap()
            => IsSome ? value : throw new Exception(UnwrapMessage);
        public T UnwrapOr(T def = default(T))
            => IsSome ? value : def;
        public T UnwrapOr(Func<T> provider)
            => IsSome ? value : provider();

        public Option<U> Map<U>(Func<T, U> converter)
            => IsSome ? Some(converter(value)) : None<U>();
        public async Task<Option<U>> MapAsync<U>(Func<T, Task<U>> converter)
            => IsSome ? Some(await converter(value)) : new None<U>();
        public U MapOr<U>(U def, Func<T, U> converter)
            => IsSome ? converter(value) : def;
        public U MapOr<U>(Func<U> provider, Func<T, U> converter)
            => IsSome ? converter(value) : provider();

        public Option<U> And<U>(Option<U> option)
            => IsNone ? None<U>() : option;
        public Option<U> AndThen<U>(Func<T, Option<U>> option)
            => IsNone ? None<U>() : option(value);
        public Task<Option<U>> AndThenAsync<U>(Func<T, Task<Option<U>>> option)
            => IsNone ? Task.FromResult(None<U>()) : option(value);

        public Option<T> Or(Option<T> other)
            => IsSome ? this : other;
        public Option<T> OrElse(Func<Option<T>> option)
            => IsSome ? this : option();
        public Task<Option<T>> OrElseAsync(Func<Task<Option<T>>> option)
            => IsSome ? Task.FromResult(this) : option();

        public void Take() { value = default(T); hasValue = false; }

        public override int GetHashCode()
            => !hasValue ? 0 : (ReferenceEquals(value, null) ? -1 : value.GetHashCode());
    }

    public sealed class None<T> : Option<T> { }

    public sealed class Some<T> : Option<T>
    {
        internal Some(T value) : base(value) { }
    }
}
