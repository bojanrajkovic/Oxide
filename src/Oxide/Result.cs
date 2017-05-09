using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using static Oxide.Options;

namespace Oxide
{
    public static class Results
    {
        public static Result<T, E> Ok<T, E>(T value)
            => new Ok<T, E>(value);
        public static Result<T, E> Err<T, E>(E error)
            => new Error<T, E>(error);

        public static Func<TIn, Result<TOut, TException>> GetSafeInvoker<TIn, TOut, TException>(Func<TIn, TOut> fn)
            where TException : Exception
            => a => {
                try {
                    return fn(a);
                } catch (TException e) {
                    return e;
                }
            };
    }

    public abstract class Result
    {
        public virtual bool IsOk => hasValue && !hasError;
        public virtual bool IsError => hasError && !hasValue;

        protected bool hasValue, hasError;

        public static Result<IEnumerable<T>, E> Combine<T, E>(params Result<T, E>[] results)
            => new CombinedResult<T, E>(results);

        public static Result<IEnumerable<T>, E> Combine<T, E>(IEnumerable<Result<T, E>> results)
            => new CombinedResult<T, E>(results);
    }

    public class Result<T, E> : Result, IEquatable<Result<T, E>>
    {
        protected T value;
        protected E error;
        ExceptionDispatchInfo errorDispatchInfo;

        // Only used by CombinedResult for convenience.
        internal Result()
        {
            hasValue = true;
            value = default(T);

            hasError = false;
        }

        // See comments on Option constructors.
        internal Result(E error)
        {
            this.error = error;

            if (error is Exception)
                errorDispatchInfo = ExceptionDispatchInfo.Capture(error as Exception);

            hasValue = false;
            hasError = true;
        }

        internal Result(T value)
        {
            this.value = value;
            hasValue = true;
            hasError = false;
        }

        void Throw(E error)
        {
            if (errorDispatchInfo != null)
                errorDispatchInfo.Throw();
            throw new Exception(error.ToString());
        }

        void ThrowWrapped(string message, E error)
        {
            if (error is Exception)
                throw new Exception(message, error as Exception);
            throw new Exception($"{message}: {error}");
        }

        public bool Equals(Result<T, E> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (hasValue != other.hasValue)
                return false;

            if (hasValue)
                return Equals(value, other.value);
            return Equals(error, other.error);
        }

        public override bool Equals(object obj)
        {
            if (obj is Result<T, E>)
                return Equals((Result<T, E>) obj);
            return false;
        }

        public override int GetHashCode() {
            if (hasValue)
                return ReferenceEquals(value, null) ? -1 : value.GetHashCode();
            return ReferenceEquals(error, null) ? -2 : error.GetHashCode();
        }

        public static bool operator==(Result<T, E> left, Result<T, E> right)
            => Equals(left, right);

        public static bool operator !=(Result<T, E> left, Result<T, E> right)
            => !Equals(left, right);

        public static implicit operator Result<T, E>(T value)
            => Results.Ok<T, E>(value);

        public static implicit operator Result<T, E>(E error)
            => Results.Err<T, E>(error);

        public Option<T> Ok() => IsOk ? Some(value) : None<T>();
        public Option<E> Err() => IsOk ? None<E>() : Some(error);

        public Result<U, E> Map<U>(Func<T, U> f) =>
            IsOk ? Results.Ok<U, E>(f(value)) : Results.Err<U, E>(error);
        public async Task<Result<U, E>> Map<U>(Func<T, Task<U>> op)
            => IsOk ? Results.Ok<U, E>(await op(value)) : Results.Err<U, E>(error);
        public Result<T, F> MapErr<F>(Func<E, F> f) =>
            IsOk ? Results.Ok<T, F>(value) : Results.Err<T, F>(f(error));
        public async Task<Result<T, F>> MapErr<F>(Func<E, Task<F>> op)
            => IsOk ? Results.Ok<T, F>(value) : Results.Err<T, F>(await op(error));

        public Result<U, E> And<U>(Result<U, E> other)
            => IsOk ? other : Results.Err<U, E>(error);
        public Result<U, E> AndThen<U>(Func<T, Result<U, E>> op)
            => IsOk ? op(value) : Results.Err<U, E>(error);
        public Task<Result<U, E>> AndThen<U>(Func<T, Task<Result<U, E>>> op)
            => IsOk ? op(value) : Task.FromResult(Results.Err<U, E>(error));

        public Result<T, F> Or<F>(Result<T, F> res)
            => IsError ? res : Results.Ok<T, F>(value);
        public Result<T, F> OrElse<F>(Func<E, Result<T, F>> op)
            => IsError ? op(error) : Results.Ok<T, F>(value);
        public async Task<Result<T, F>> OrElse<F>(Func<E, Task<Result<T, F>>> op)
            => IsError ? await op(error) : Results.Ok<T, F>(value);

        public T UnwrapOr(T optb) => IsOk ? value : optb;
        public T UnwrapOrElse(Func<E, T> op) => IsOk ? value : op(error);
        public async Task<T> UnwrapOrElse(Func<E, Task<T>> op)
            => IsOk ? value : await op(error);

        public T Unwrap() {
            if (IsOk)
                return value;
            Throw(error);

            // This is never actually reached, as `Throw`, well, throws.
            return default(T);
        }

        public T Expect(string msg) {
            if (IsOk)
                return value;
            ThrowWrapped(msg, error);

            // This is never actually reached, as `Throw`, well, throws.
            return default(T);
        }

        public E UnwrapError() {
            if (IsError)
                return error;
            throw new Exception(value.ToString());
        }

        public E ExpectError(string msg) {
            if (IsError)
                return error;
            throw new Exception($"{msg}: {value}");
        }

        public T UnwrapOrDefault() => IsOk ? value : default(T);
    }

    class CombinedResult<T, E> : Result<IEnumerable<T>, E>
    {
        public CombinedResult (IEnumerable<Result<T, E>> results) : base()
        {
            var badResult = results.FirstOrDefault(r => r.IsError);
            if (badResult == null) {
                hasError = false;
                hasValue = true;

                value = results.Select(r => r.Unwrap()).ToList();
            } else {
                hasError = true;
                hasValue = false;

                error = badResult.UnwrapError();
            }
        }
    }

    public sealed class Error<T, E> : Result<T, E>
    {
        internal Error(E error) : base(error) {}
    }

    public sealed class Ok<T, E> : Result<T, E>
    {
        internal Ok(T value) : base(value) {}
    }
}
