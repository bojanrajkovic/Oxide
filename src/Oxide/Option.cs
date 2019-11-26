using System;
using System.Threading.Tasks;

using static Oxide.Options;

namespace Oxide
{
    public static class Options
    {
        public static Option<T> None<T>() => new None<T>();
        public static Option<T> Some<T>(T value) => new Some<T>(value);
        public static async Task<T> Unwrap<T>(this Task<Option<T>> self) => (await self.ConfigureAwait(false)).Unwrap();

        public static async Task<Option<TOut>> AndThenAsync<TIn, TOut>(
            this Task<Option<TIn>> self,
            Func<TIn, Option<TOut>> continuation
        ) {
            return (await self).AndThen(continuation);
        }

        public static async Task<Option<TOut>> AndThenAsync<TIn, TOut>(
            this Task<Option<TIn>> self,
            Func<TIn, Task<Option<TOut>>> continuation
        ) {
            var ret = await self;
            return await ret.AndThenAsync(continuation);
        }
    }

    public abstract class Option
    {
        public bool IsNone => !HasValue;
        public bool IsSome => !IsNone;

        private protected Option(bool hasValue) => HasValue = hasValue;

        protected readonly bool HasValue;
    }

    public abstract class Option<TOption> : Option, IEquatable<Option<TOption>>
    {
        static readonly string UnwrapMessage
            = $"Tried to unwrap a None<{typeof(TOption)}>!";

        readonly TOption value;

        private protected Option() : base(false) { }
        private protected Option(TOption value) : base(true) { this.value = value; }

        public bool Equals(Option<TOption> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (HasValue != other.HasValue)
                return false;

            return !HasValue || Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Option<TOption> option)
                return Equals(option);
            return false;
        }

        public static bool operator ==(Option<TOption> left, Option<TOption> right)
            => left != null && left.Equals(right);
        public static bool operator !=(Option<TOption> left, Option<TOption> right)
            => left != null && !left.Equals(right);
        public static implicit operator Option<TOption>(TOption value)
            => value == null ? None<TOption>() : Some(value);

        public TOption Expect(string msg)
            => IsSome ? value : throw new Exception(msg);
        public TOption Unwrap()
            => IsSome ? value : throw new Exception(UnwrapMessage);

        public bool TryUnwrap(out TOption val) {
            val = value;
            return IsSome;
        }

        public TOption UnwrapOr(TOption def = default)
            => IsSome ? value : def;
        public TOption UnwrapOr(Func<TOption> provider)
            => IsSome ? value : provider();

        public Option<TResult> Map<TResult>(Func<TOption, TResult> converter)
            => IsSome ? Some(converter(value)) : None<TResult>();
        public async Task<Option<TResult>> MapAsync<TResult>(Func<TOption, Task<TResult>> converter)
            => IsSome ? Some(await converter(value).ConfigureAwait(false)) : new None<TResult>();
        public TResult MapOr<TResult>(TResult def, Func<TOption, TResult> converter)
            => IsSome ? converter(value) : def;
        public TResult MapOr<TResult>(Func<TResult> provider, Func<TOption, TResult> converter)
            => IsSome ? converter(value) : provider();

        public Option<TResult> And<TResult>(Option<TResult> option)
            => IsNone ? None<TResult>() : option;
        public Option<TResult> AndThen<TResult>(Func<TOption, Option<TResult>> option)
            => IsNone ? None<TResult>() : option(value);
        public Task<Option<TResult>> AndThenAsync<TResult>(Func<TOption, Task<Option<TResult>>> option)
            => IsNone ? Task.FromResult(None<TResult>()) : option(value);

        public Option<TOption> Or(Option<TOption> other)
            => IsSome ? this : other;
        public Option<TOption> OrElse(Func<Option<TOption>> option)
            => IsSome ? this : option();
        public Task<Option<TOption>> OrElseAsync(Func<Task<Option<TOption>>> option)
            => IsSome ? Task.FromResult(this) : option();

        public override int GetHashCode()
            => !HasValue ? 0 : ReferenceEquals(value, null) ? -1 : value.GetHashCode();
    }

    public sealed class None<T> : Option<T> { }

    public sealed class Some<T> : Option<T>
    {
        internal Some(T value) : base(value) { }
    }
}
