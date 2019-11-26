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
        public static Result<TResult, TError> Ok<TResult, TError>(TResult value)
            => new Ok<TResult, TError>(value);
        public static Result<TResult, TError> Err<TResult, TError>(TError error)
            => new Error<TResult, TError>(error);

        public static Func<TIn, Result<TOut, TException>> GetSafeInvoker<TIn, TOut, TException>(Func<TIn, TOut> fn)
            where TException : Exception
            => a => {
                try {
                    return fn(a);
                } catch (TException e) {
                    return e;
                }
            };

        public static async Task<Result<TOut, TError>> AndThenAsync<TIn, TOut, TError>(
            this Task<Result<TIn, TError>> resTask,
            Func<TIn, Result<TOut, TError>> continuation
        ) {
            return (await resTask).AndThen(continuation);
        }

        public static async Task<Result<TOut, TError>> AndThenAsync<TIn, TOut, TError>(
            this Task<Result<TIn, TError>> self,
            Func<TIn, Task<Result<TOut, TError>>> continuation
        ) {
            var ret = await self;
            return await ret.AndThen(continuation);
        }
    }

    public abstract class Result
    {
        public bool IsOk => HasValue && !hasError;
        public bool IsError => hasError && !HasValue;

        private protected Result(bool hasValue, bool hasError)
        {
            HasValue = hasValue;
            this.hasError = hasError;
        }

        protected readonly bool HasValue;
        readonly bool hasError;

        public static Result<IEnumerable<TResult>, TError> Combine<TResult, TError>(params Result<TResult, TError>[] results)
            => Combine((IEnumerable<Result<TResult, TError>>)results);

        public static Result<IEnumerable<TResult>, TError> Combine<TResult, TError>(IEnumerable<Result<TResult, TError>> results)
        {
            var list = results.ToList();
            var badResult = list.FirstOrDefault(r => r.IsError);
            return badResult == null ? Results.Ok<IEnumerable<TResult>, TError>(list.Select(r => r.Unwrap())) : badResult.UnwrapError();
        }
    }

    public class Result<TResult, TError> : Result, IEquatable<Result<TResult, TError>>
    {
        protected readonly TResult Value;
        protected readonly TError Error;
        readonly ExceptionDispatchInfo errorDispatchInfo;

        private protected Result(TError error) : base(false, true)
        {
            Error = error;

            if (error is Exception exception)
                errorDispatchInfo = ExceptionDispatchInfo.Capture(exception);
        }

        private protected Result(TResult value) : base(true, false)
        {
            Value = value;
        }

        void Throw(TError error)
        {
            errorDispatchInfo?.Throw();
            throw new Exception(error.ToString());
        }

        static void ThrowWrapped(string message, TError error)
        {
            if (error is Exception exception)
                throw new Exception(message, exception);
            throw new Exception($"{message}: {error}");
        }

        public bool Equals(Result<TResult, TError> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (HasValue != other.HasValue)
                return false;

            return HasValue ? Equals(Value, other.Value) : Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            return obj is Result<TResult, TError> result && Equals(result);
        }

        public override int GetHashCode() {
            if (IsOk)
                return ReferenceEquals(Value, null) ? -1 : Value.GetHashCode();
            return ReferenceEquals(Error, null) ? -2 : Error.GetHashCode();
        }

        public void Deconstruct(out TResult value, out TError error)
        {
            value = Value;
            error = Error;
        }

        // TODO: Properly implement == and != to check IsOk & compare values or errors as needed.
        public static bool operator==(Result<TResult, TError> left, Result<TResult, TError> right)
            => Equals(left, right);

        public static bool operator !=(Result<TResult, TError> left, Result<TResult, TError> right)
            => !Equals(left, right);

        public static implicit operator Result<TResult, TError>(TResult value)
            => Results.Ok<TResult, TError>(value);

        public static implicit operator Result<TResult, TError>(TError error)
            => Results.Err<TResult, TError>(error);

        public Option<TResult> Ok() => IsOk ? Some(Value) : None<TResult>();
        public Option<TError> Err() => IsOk ? None<TError>() : Some(Error);

        public Result<TOutput, TError> Map<TOutput>(Func<TResult, TOutput> f) =>
            IsOk ? Results.Ok<TOutput, TError>(f(Value)) : Results.Err<TOutput, TError>(Error);
        public async Task<Result<TOutput, TError>> Map<TOutput>(Func<TResult, Task<TOutput>> op)
            => IsOk ? Results.Ok<TOutput, TError>(await op(Value).ConfigureAwait(false)) : Results.Err<TOutput, TError>(Error);
        public Result<TResult, TErrorOutput> MapErr<TErrorOutput>(Func<TError, TErrorOutput> f) =>
            IsOk ? Results.Ok<TResult, TErrorOutput>(Value) : Results.Err<TResult, TErrorOutput>(f(Error));
        public async Task<Result<TResult, TErrorOutput>> MapErr<TErrorOutput>(Func<TError, Task<TErrorOutput>> op)
            => IsOk ? Results.Ok<TResult, TErrorOutput>(Value) : Results.Err<TResult, TErrorOutput>(await op(Error).ConfigureAwait(false));

        public Result<TOutput, TError> And<TOutput>(Result<TOutput, TError> other)
            => IsOk ? other : Results.Err<TOutput, TError>(Error);
        public Result<TOutput, TError> AndThen<TOutput>(Func<TResult, Result<TOutput, TError>> op)
            => IsOk ? op(Value) : Results.Err<TOutput, TError>(Error);
        public Task<Result<TOutput, TError>> AndThen<TOutput>(Func<TResult, Task<Result<TOutput, TError>>> op)
            => IsOk ? op(Value) : Task.FromResult(Results.Err<TOutput, TError>(Error));

        public Result<TResult, TErrorOutput> Or<TErrorOutput>(Result<TResult, TErrorOutput> res)
            => IsError ? res : Results.Ok<TResult, TErrorOutput>(Value);
        public Result<TResult, TErrorOutput> OrElse<TErrorOutput>(Func<TError, Result<TResult, TErrorOutput>> op)
            => IsError ? op(Error) : Results.Ok<TResult, TErrorOutput>(Value);
        public async Task<Result<TResult, TErrorOutput>> OrElse<TErrorOutput>(Func<TError, Task<Result<TResult, TErrorOutput>>> op)
            => IsError ? await op(Error).ConfigureAwait(false) : Results.Ok<TResult, TErrorOutput>(Value);

        public TResult UnwrapOr(TResult other) => IsOk ? Value : other;
        public TResult UnwrapOrElse(Func<TError, TResult> op) => IsOk ? Value : op(Error);
        public async Task<TResult> UnwrapOrElse(Func<TError, Task<TResult>> op)
            => IsOk ? Value : await op(Error).ConfigureAwait(false);

        public TResult Unwrap() {
            if (IsOk)
                return Value;

            Throw(Error);

            // This is never actually reached, as `Throw`, well, throws.
            return default;
        }

        public bool TryUnwrap(out TResult value) {
            value = Value;
            return IsOk;
        }

        public bool TryUnwrap(out TResult value, out TError error) {
            Deconstruct(out value, out error);
            return IsOk;
        }

        public TResult Expect(string msg) {
            if (IsOk)
                return Value;
            ThrowWrapped(msg, Error);

            // This is never actually reached, as `Throw`, well, throws.
            return default;
        }

        public TError UnwrapError() {
            if (IsError)
                return Error;
            throw new Exception(Value.ToString());
        }

        public bool TryUnwrapError(out TError error) {
            error = Error;
            return IsError;
        }

        public TError ExpectError(string msg) {
            if (IsError)
                return Error;
            throw new Exception($"{msg}: {Value}");
        }

        public TResult UnwrapOrDefault() => IsOk ? Value : default;
    }

    public sealed class Error<TResult, TError> : Result<TResult, TError>
    {
        internal Error(TError error) : base(error) {}
    }

    public sealed class Ok<TResult, TError> : Result<TResult, TError>
    {
        internal Ok(TResult value) : base(value) {}
    }
}
