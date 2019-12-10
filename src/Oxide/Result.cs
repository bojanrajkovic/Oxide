using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using static Oxide.Options;

namespace Oxide
{
    /// <summary>
    ///     Helper methods and extension methods for working with <see cref="Result{TResult,TError}" />.
    /// </summary>
    public static class Results
    {
        /// <summary>
        ///     Creates a new <see cref="Result{TResult,TError}" /> that is an <see cref="Ok{TResult,TError}" />,
        ///     which contains a result, but not an error.
        /// </summary>
        /// <param name="value">The value that the result contains.</param>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <returns>A result with a result value and no error value.</returns>
        public static Result<TResult, TError> Ok<TResult, TError>(TResult value)
            => new Ok<TResult, TError>(value);

        /// <summary>
        ///     Creates a new <see cref="Result{TResult,TError}" /> that is an <see cref="Error{TResult,TError}" />,
        ///     which does not contain a result, but contains an error.
        /// </summary>
        /// <param name="error">The error that the result contains.</param>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <typeparam name="TError">The type of the error value.</typeparam>
        /// <returns>A result with an error value and no result value.</returns>
        public static Result<TResult, TError> Err<TResult, TError>(TError error)
            => new Error<TResult, TError>(error);

        /// <summary>
        ///     Awaits the <see cref="Task{TResult}" /> in <paramref name="self" />, and chains the
        ///     resulting <see cref="Result{TResult,TError}" /> with <paramref name="continuation" /> by
        ///     calling <see cref="Result{TResult,TError}.AndThen{TOutput}" />.
        /// </summary>
        /// <param name="self">The task to continue.</param>
        /// <param name="continuation">The function to call on the wrapped value.</param>
        /// <typeparam name="TIn">The type of the wrapped value.</typeparam>
        /// <typeparam name="TOut">The type of the <see cref="Result{TResult,TError}" /> value returned from the continuation.</typeparam>
        /// <typeparam name="TError">The type of the error.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous chaining of two results, which wraps
        ///     the result of calling <paramref name="continuation" /> with the wrapped value from
        ///     <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///     This is a convenience method that allows chaining async computations in a more natural way. Without this,
        ///     the syntax gymnastics around <c>await</c> can become strenuous, doubly so if the compiler is not able to infer
        ///     generic type parameters. This is
        ///     less of a issue with <see cref="Result{TResult,TError}.AndThen{TOutput}" /> because it is not async-oriented, but
        ///     the syntax
        ///     is still cleaner. Consider:
        ///     <code>
        /// <![CDATA[
        /// Task<Result<TResult, TError>> task = GetSomeResultAsync(...);
        /// Result<TTransformed, TError> final = (await task).AndThen(...);
        /// ]]>
        /// </code>
        ///     vs.
        ///     <code>
        /// <![CDATA[
        /// var final = await GetSomeResultAsync(...).AndThenAsync(...);
        /// ]]>
        /// </code>
        /// </remarks>
        public static async Task<Result<TOut, TError>> AndThenAsync<TIn, TOut, TError>(
            this Task<Result<TIn, TError>> self,
            Func<TIn, Result<TOut, TError>> continuation
        ) => (await self.ConfigureAwait(false)).AndThen(continuation);

        /// <summary>
        ///     Awaits the <see cref="Task{TResult}" /> in <paramref name="self" />, and chains the resulting
        ///     <see cref="Result{TResult,TError}" /> with <paramref name="continuation" /> by calling
        ///     <see cref="Result{TResult,TError}.AndThenAsync{TOutput}" />.
        /// </summary>
        /// <param name="self">The task to continue.</param>
        /// <param name="continuation">The function to call on the wrapped value.</param>
        /// <typeparam name="TIn">The type of the wrapped value.</typeparam>
        /// <typeparam name="TOut">The type of the <see cref="Result{TResult,TError}" /> value returned from the continuation.</typeparam>
        /// <typeparam name="TError">The type of the error</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous chaining of two results, which wraps the
        ///     result of calling <paramref name="continuation" /> with the wrapped value from
        ///     <paramref name="self" />.
        /// </returns>
        /// <remarks>
        ///     This is a convenience method that allows chaining async computations in a more natural way. Without this,
        ///     the syntax gymnastics around <c>await</c> can become strenuous, doubly so if the compiler is not able to infer
        ///     generic type parameters.
        ///     Consider:
        ///     <code>
        /// <![CDATA[
        /// await (await (await GetSomeResultAsync(...)).AndThenAsync(...)).AndThenAsync(...);
        ///
        /// // or
        ///
        /// var first = await GetSomeResultAsync(...);
        /// var second = await first.AndThenAsync(...);
        /// var third = await second.AndThenAsync(...);
        /// ]]>
        /// </code>
        ///     vs.
        ///     <code>
        /// <![CDATA[
        /// var final = await GetSomeResultAsync(...).AndThenAsync(...).AndThenAsync(...);
        /// ]]>
        /// </code>
        /// </remarks>
        public static async Task<Result<TOut, TError>> AndThenAsync<TIn, TOut, TError>(
            this Task<Result<TIn, TError>> self,
            Func<TIn, Task<Result<TOut, TError>>> continuation
        ) => await (await self.ConfigureAwait(false))
            .AndThenAsync(continuation)
            .ConfigureAwait(false);

        /// <summary>
        ///     Combines multiple <see cref="Result{TResult,TError}" /> into a single value.
        /// </summary>
        /// <param name="results">The results to combine.</param>
        /// <typeparam name="TResult">The type of the wrapped value in each result.</typeparam>
        /// <typeparam name="TError">The type of the wrapped error in each result.</typeparam>
        /// <returns>A combined result with all of the values as the wrapped value, or the first error encountered.</returns>
        public static Result<IEnumerable<TResult>, TError> Combine<TResult, TError>(
            params Result<TResult, TError>[] results
        ) => Combine((IEnumerable<Result<TResult, TError>>)results);

        /// <inheritdoc cref="Combine{TResult,TError}(Oxide.Result{TResult,TError}[])" />
        public static Result<IEnumerable<TResult>, TError> Combine<TResult, TError>(
            IEnumerable<Result<TResult, TError>> results
        )
        {
            var list = results.ToList();
            var badResult = list.FirstOrDefault(r => r.IsError);
            return badResult == null
                ? Ok<IEnumerable<TResult>, TError>(list.Select(r => r.Unwrap()))
                : badResult.UnwrapError();
        }
    }

    /// <summary>
    ///     The abstract base class for all Result types.
    /// </summary>
    public abstract class Result
    {
        readonly bool hasError;
        private protected readonly bool HasValue;

        private protected Result(bool hasValue, bool hasError)
        {
            HasValue = hasValue;
            this.hasError = hasError;
        }

        /// <summary>
        ///     Gets whether this <see cref="Result" /> contains a value.
        /// </summary>
        public bool IsOk => HasValue && !hasError;

        /// <summary>
        ///     Gets whether this <see cref="Result" /> contains an error.
        /// </summary>
        public bool IsError => hasError && !HasValue;
    }

    /// <summary>
    ///     The <see cref="Result{TResult,TError}" /> represents an operation that could return either
    ///     a result or an error. Every option is either an <see cref="Ok{TResult, TError}" />, and has
    ///     a value and no error, or is a <see cref="Error{TResult,TError}" /> and has no value and an error.
    /// </summary>
    /// <typeparam name="TResult">The type of the wrapped value.</typeparam>
    /// <typeparam name="TError">The type of the wrapped error.</typeparam>
    public class Result<TResult, TError> : Result, IEquatable<Result<TResult, TError>>
    {
        readonly TError error;
        readonly ExceptionDispatchInfo errorDispatchInfo;
        readonly TResult value;

        private protected Result(TError error) : base(false, true)
        {
            this.error = error;

            if (error is Exception exception)
                errorDispatchInfo = ExceptionDispatchInfo.Capture(exception);
        }

        private protected Result(TResult value) : base(true, false)
            => this.value = value;

        /// <inheritdoc />
        /// <remarks>
        ///     Equality for <see cref="Result{TResult,TError}" /> is determined as follows:
        ///     <list type="numbered">
        ///         <item>
        ///             <description>If <paramref name="other" /> is null, return <c>false</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If they are not both <see cref="Ok{TResult,TError}" /> or <see cref="Error{TResult,TError}" />
        ///                 , return false.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Compare either the values or errors to each other using
        ///                 <see cref="object.Equals(object,object)" />.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public bool Equals(Result<TResult, TError> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (HasValue != other.HasValue)
                return false;

            return HasValue ? Equals(value, other.value) : Equals(error, other.error);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is Result<TResult, TError> result && Equals(result);

        /// <inheritdoc />
        public override int GetHashCode()
            => IsOk ? value?.GetHashCode() ?? -1 : error?.GetHashCode() ?? -2;

        /// <summary>
        ///     Deconstructs a <see cref="Result{TResult,TError}" /> into its constituent value and error.
        /// </summary>
        /// <param name="val">The reference to place the value into.</param>
        /// <param name="err">The reference to place the error into.</param>
        /// <remarks>This method is most useful with tuple deconstruction.</remarks>
        /// <example>
        ///     var (value, error) = someResult;
        /// </example>
        public void Deconstruct(out TResult val, out TError err)
        {
            val = value;
            err = error;
        }

        /// <summary>
        ///     Determines whether the two objects are equal to each other.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns><c>true</c> if the objects are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Result<TResult, TError> left, Result<TResult, TError> right)
            => left?.Equals(right) ?? ReferenceEquals(right, null);

        /// <summary>
        ///     Determines whether the two objects are not equal to each other.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns><c>true</c> if the objects are not equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(Result<TResult, TError> left, Result<TResult, TError> right)
            => !(left == right);

        /// <summary>
        ///     Implicitly converts a <typeparamref name="TResult" /> value to a
        ///     <see cref="Result{TResult,TError}" /> with a value.
        /// </summary>
        /// <param name="value">The value to wrap in a result.</param>
        /// <returns>A <see cref="Ok{TResult, TError}" /> wrapping the given value.</returns>
        /// <exception cref="NullReferenceException">If the passed value is null.</exception>
        public static implicit operator Result<TResult, TError>(TResult value)
            => value is null
                ? throw new NullReferenceException(nameof(value))
                : Results.Ok<TResult, TError>(value);

        /// <summary>
        ///     Implicitly converts a <typeparamref name="TError" /> value to a
        ///     <see cref="Result{TResult,TError}" /> with an error.
        /// </summary>
        /// <param name="error">The error to wrap in a result.</param>
        /// <returns>A <see cref="Error{TResult, TError}" /> wrapping the given error.</returns>
        /// <exception cref="NullReferenceException">If the passed error is null.</exception>
        public static implicit operator Result<TResult, TError>(TError error)
            => error is null
                ? throw new NullReferenceException(nameof(error))
                : Results.Err<TResult, TError>(error);

        /// <summary>
        ///     Converts self into an <see cref="Option{TOption}" />, consuming self, and discarding the error, if any.
        /// </summary>
        /// <returns>A <see cref="Some{T}" /> wrapping the value of this result, or a <see cref="None{T}" />.</returns>
        public Option<TResult> Ok()
            => IsOk ? Some(value) : None<TResult>();

        /// <summary>
        ///     Converts self into an <see cref="Option{TOption}" />, consuming self, and discarding the value, if any.
        /// </summary>
        /// <returns>A <see cref="Some{T}" /> wrapping the error from this result, or a <see cref="None{T}" />.</returns>
        public Option<TError> Err()
            => IsOk ? None<TError>() : Some(error);

        /// <summary>
        ///     Maps a <see cref="Result{TResult, TError}" /> to <see cref="Result{TOutput, TError}" /> by
        ///     applying <paramref name="f" /> to a contained <see cref="Ok{TResult,TError}" /> value, leaving
        ///     an <see cref="Error{TResult,TError}" /> untouched.
        /// </summary>
        /// <param name="f">The function to call on the wrapped value.</param>
        /// <typeparam name="TOutput">The type of the output of <paramref name="f" />.</typeparam>
        /// <returns>The result of applying <paramref name="f" /> to the value, or the original error.</returns>
        /// <remarks>This function can be used to compose the results of two functions.</remarks>
        public Result<TOutput, TError> Map<TOutput>(Func<TResult, TOutput> f)
            => IsOk ? f(value) : Results.Err<TOutput, TError>(error);

        /// <summary>
        ///     Maps a <see cref="Result{TResult, TError}" /> to <see cref="Result{TOutput, TError}" /> by
        ///     applying <paramref name="f" /> to a contained <see cref="Ok{TResult,TError}" /> value, leaving
        ///     an <see cref="Error{TResult,TError}" /> untouched.
        /// </summary>
        /// <param name="f">The function to call on the wrapped value.</param>
        /// <typeparam name="TOutput">The type of the output of <paramref name="f" />.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous result of applying <paramref name="f" /> to the value, or the original
        ///     error.
        /// </returns>
        /// <remarks>This function can be used to compose the results of two functions.</remarks>
        public async Task<Result<TOutput, TError>> MapAsync<TOutput>(Func<TResult, Task<TOutput>> f)
            => IsOk
                ? await f(value).ConfigureAwait(false)
                : Results.Err<TOutput, TError>(error);

        /// <summary>
        ///     Maps a <see cref="Result{TResult, TError}" /> to <see cref="Result{TResult,TErrorOutput}" /> by
        ///     applying <paramref name="f" /> to a contained <see cref="Error{TResult,TError}" /> value, leaving
        ///     an <see cref="Ok{TResult,TError}" /> untouched.
        /// </summary>
        /// <param name="f">The function to call on the wrapped value.</param>
        /// <typeparam name="TErrorOutput">The type of the error output of <paramref name="f" />.</typeparam>
        /// <returns>The result of applying <paramref name="f" /> to the error, or the original value.</returns>
        public Result<TResult, TErrorOutput> MapErr<TErrorOutput>(Func<TError, TErrorOutput> f)
            => IsOk ? value : Results.Err<TResult, TErrorOutput>(f(error));

        /// <summary>
        ///     Maps a <see cref="Result{TResult, TError}" /> to <see cref="Result{TResult,TErrorOutput}" /> by
        ///     applying <paramref name="op" /> to a contained <see cref="Error{TResult,TError}" /> value, leaving
        ///     an <see cref="Ok{TResult,TError}" /> untouched.
        /// </summary>
        /// <param name="op">The function to call on the wrapped value.</param>
        /// <typeparam name="TErrorOutput">The type of the error output of <paramref name="op" />.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous result of applying <paramref name="op" /> to the error, or the
        ///     original value.
        /// </returns>
        public async Task<Result<TResult, TErrorOutput>> MapErrAsync<TErrorOutput>(Func<TError, Task<TErrorOutput>> op)
            => IsOk
                ? Results.Ok<TResult, TErrorOutput>(value)
                : await op(error).ConfigureAwait(false);

        /// <summary>
        ///     Chains two results together, discarding the value of the current result.
        /// </summary>
        /// <param name="other">The other result.</param>
        /// <typeparam name="TOutput">The type of the value contained in the other result.</typeparam>
        /// <returns>
        ///     <paramref name="other" /> if this is an <see cref="Ok{TResult,TError}" />, otherwise
        ///     the error from this result.
        /// </returns>
        /// <remarks>
        ///     <paramref name="other" /> is eagerly evaluated. If lazy evaluation is desired, use
        ///     <see cref="AndThen{TOutput}" />.
        /// </remarks>
        public Result<TOutput, TError> And<TOutput>(Result<TOutput, TError> other)
            => IsOk ? other : Results.Err<TOutput, TError>(error);

        /// <summary>
        ///     Chains two results together, passing the value of the current result to the chaining
        ///     function <paramref name="op" />.
        /// </summary>
        /// <param name="op">The function to call to get the new <see cref="Result{TResult,TError}" />.</param>
        /// <typeparam name="TOutput">The type of the value contained in the new result.</typeparam>
        /// <returns>
        ///     The result of calling <paramref name="op" /> with the value from this result if this is an
        ///     <see cref="Ok{TResult,TError}" />,
        ///     otherwise the error from this result.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the value of the result, unlike
        ///     <see cref="And{TOutput}" />.
        /// </remarks>
        public Result<TOutput, TError> AndThen<TOutput>(Func<TResult, Result<TOutput, TError>> op)
            => IsOk ? op(value) : Results.Err<TOutput, TError>(error);

        /// <summary>
        ///     Chains two results together, passing the value of the current result to the chaining
        ///     function <paramref name="op" />.
        /// </summary>
        /// <param name="op">The function to call to get the new <see cref="Result{TResult,TError}" />.</param>
        /// <typeparam name="TOutput">The type of the value contained in the new result.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous result of calling <paramref name="op" /> with the value from this result if
        ///     this is an <see cref="Ok{TResult,TError}" />,
        ///     or the error from this result.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the value of the result, unlike
        ///     <see cref="And{TOutput}" />.
        /// </remarks>
        public Task<Result<TOutput, TError>> AndThenAsync<TOutput>(Func<TResult, Task<Result<TOutput, TError>>> op)
            => IsOk ? op(value) : Task.FromResult(Results.Err<TOutput, TError>(error));

        /// <summary>
        ///     Chains two results together.
        /// </summary>
        /// <param name="res">The other result.</param>
        /// <typeparam name="TErrorOutput">The type of the error value from <paramref name="res" />.</typeparam>
        /// <returns>
        ///     <paramref name="res" /> if this is an <see cref="Error{TResult,TError}" />, or the value from this result
        ///     otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="res" /> is eagerly evaluated. If lazy evaluation is desired, use
        ///     <see cref="OrElse{TErrorOutput}" />.
        /// </remarks>
        public Result<TResult, TErrorOutput> Or<TErrorOutput>(Result<TResult, TErrorOutput> res)
            => IsError ? res : Results.Ok<TResult, TErrorOutput>(value);

        /// <summary>
        ///     Chains two results together, calling <paramref name="op" /> to provide a result if the option
        ///     is a <see cref="Error{TResult,TError}" />.
        /// </summary>
        /// <param name="op">The function to call to get the new <see cref="Result{TResult,TError}" />.</param>
        /// <typeparam name="TErrorOutput">The type of the error value from the result of calling <paramref name="op" />.</typeparam>
        /// <returns>
        ///     The result of calling <paramref name="op" /> with the error value from this result if this is an
        ///     <see cref="Error{TResult,TError}" />, or the value from this result otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the error value from this result. If eager
        ///     evaluation is desired, use <see cref="Or{TErrorOutput}" />.
        /// </remarks>
        public Result<TResult, TErrorOutput> OrElse<TErrorOutput>(Func<TError, Result<TResult, TErrorOutput>> op)
            => IsError ? op(error) : Results.Ok<TResult, TErrorOutput>(value);

        /// <summary>
        ///     Chains two results together, calling <paramref name="op" /> to provide a result if the option
        ///     is a <see cref="Error{TResult,TError}" />.
        /// </summary>
        /// <param name="op">The function to call to get the new <see cref="Result{TResult,TError}" />.</param>
        /// <typeparam name="TErrorOutput">The type of the error value from the result of calling <paramref name="op" />.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous result of calling <paramref name="op" /> with the error value from this
        ///     result if this is an
        ///     <see cref="Error{TResult,TError}" />, or the value from this result otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the error value from this result. If eager
        ///     evaluation is desired, use <see cref="Or{TErrorOutput}" />.
        /// </remarks>
        public async Task<Result<TResult, TErrorOutput>> OrElseAsync<TErrorOutput>(
            Func<TError, Task<Result<TResult, TErrorOutput>>> op
        ) => IsError
            ? await op(error).ConfigureAwait(false)
            : Results.Ok<TResult, TErrorOutput>(value);

        /// <summary>
        ///     Unwraps the value from this result, returning a fallback if there is no value.
        /// </summary>
        /// <param name="other">The fallback value to return if this result is an <see cref="Error{TResult,TError}" />.</param>
        /// <returns>
        ///     The value from this result if it is a <see cref="Ok{TResult,TError}" />, or <paramref name="other" />
        ///     otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="other" /> is eagerly evaluated. If lazy evaluation is desired, use <see cref="UnwrapOrElse" />
        ///     .
        /// </remarks>
        public TResult UnwrapOr(TResult other = default)
            => IsOk ? value : other;


        /// <summary>
        ///     Unwraps the value from this result, or the result of calling <paramref name="op" /> if there is no value.
        /// </summary>
        /// <param name="op">The function to call to get a fallback value.</param>
        /// <returns>
        ///     The value from this result if it is a <see cref="Ok{TResult,TError}" />, or the result of calling
        ///     <paramref name="op" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the error value from this result. If eager
        ///     evaluation is desired, use <see cref="UnwrapOr(TResult)" />.
        /// </remarks>
        public TResult UnwrapOrElse(Func<TError, TResult> op)
            => IsOk ? value : op(error);

        /// <summary>
        ///     Unwraps the value from this result, or the result of calling <paramref name="op" /> if there is no value.
        /// </summary>
        /// <param name="op">The function to call to get a fallback value.</param>
        /// <returns>
        ///     A task that represents the value from this result if it is a <see cref="Ok{TResult,TError}" />, or the result
        ///     of calling <paramref name="op" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="op" /> is lazily evaluated, and is passed the error value from this result. If eager
        ///     evaluation is desired, use <see cref="UnwrapOr(TResult)" />.
        /// </remarks>
        public async Task<TResult> UnwrapOrElseAsync(Func<TError, Task<TResult>> op)
            => IsOk ? value : await op(error).ConfigureAwait(false);

        /// <summary>
        ///     Unwraps a result, yielding the content of an <see cref="Ok{TResult,TError}" />.
        /// </summary>
        /// <returns>The value from an <see cref="Ok{TResult,TError}" />.</returns>
        /// <exception cref="TError">
        ///     If <typeparamref name="TError" /> is an exception and this is an
        ///     <see cref="Error{TResult,TError}" />.
        /// </exception>
        /// <exception cref="Exception">
        ///     If <typeparamref name="TError" /> is not an exception and this is an
        ///     <see cref="Error{TResult,TError}" />.
        /// </exception>
        public TResult Unwrap()
        {
            if (IsOk)
                return value;

            errorDispatchInfo?.Throw();
            throw new Exception(error.ToString());
        }

        /// <summary>
        ///     Tries to unwrap the result of an <see cref="Ok{TResult,TError}" /> into <paramref name="val" />, returning
        ///     <c>true</c>
        ///     if a value was unwrapped, and <c>false</c> otherwise.
        /// </summary>
        /// <param name="val">A location to unwrap the value into.</param>
        /// <returns><c>true</c> if a value was unwrapped, <c>false</c> otherwise.</returns>
        public bool TryUnwrap(out TResult val)
        {
            val = value;
            return IsOk;
        }

        /// <summary>
        ///     Tries to unwrap the result of an <see cref="Ok{TResult,TError}" /> into <paramref name="val" /> for the value and
        ///     <paramref name="err" /> for the error, returning <c>true</c>
        ///     if a value was unwrapped, and <c>false</c> otherwise.
        /// </summary>
        /// <param name="val">A location to unwrap the value into.</param>
        /// <param name="err">A location to unwrap the error into.</param>
        /// <returns><c>true</c> if a value was unwrapped, <c>false</c> otherwise.</returns>
        public bool TryUnwrap(out TResult val, out TError err)
        {
            Deconstruct(out val, out err);
            return IsOk;
        }

        /// <summary>
        ///     Unwraps a result, yielding the content of an <see cref="Ok{TResult,TError}" />.
        /// </summary>
        /// <returns>The value from an <see cref="Ok{TResult,TError}" />.</returns>
        /// <exception cref="Exception">
        ///     If <typeparamref name="TError" /> is not an exception and this is an
        ///     <see cref="Error{TResult,TError}" />.
        /// </exception>
        /// <remarks>
        ///     The exception thrown will have <paramref name="msg" /> as its message. If the error value is an exception, it
        ///     will be included as the inner exception, otherwise it will be stringified into the exception message.
        /// </remarks>
        public TResult Expect(string msg)
        {
            if (IsOk)
                return value;

            if (error is Exception exception)
                throw new Exception(msg, exception);

            throw new Exception($"{msg}: {error}");
        }

        /// <summary>
        ///     Unwraps an error, yielding the content of an <see cref="Error{TResult,TError}" />.
        /// </summary>
        /// <returns>The error from an <see cref="Error{TResult,TError}" />.</returns>
        /// <exception cref="Exception">If this is not an error value.</exception>
        public TError UnwrapError()
            => IsError ? error : throw new Exception(value.ToString());

        /// <summary>
        ///     Tries to unwrap the error of an <see cref="Error{TResult,TError}" /> into <paramref name="err" />, returning
        ///     <c>true</c> if an
        ///     error was unwrapped, and <c>false</c> otherwise.
        /// </summary>
        /// <param name="err">A location to unwrap the error into.</param>
        /// <returns><c>true</c> if an error was unwrapped, <c>false</c> otherwise.</returns>
        public bool TryUnwrapError(out TError err)
        {
            err = error;
            return IsError;
        }

        /// <summary>
        ///     Unwraps an error, yielding the error of an <see cref="Error{TResult,TError}" />.
        /// </summary>
        /// <param name="msg">A message to throw as an exception if this is not an <see cref="Error{TResult,TError}" />.</param>
        /// <returns>The error value.</returns>
        /// <exception cref="Exception">If this is not an <see cref="Error{TResult,TError}" />.</exception>
        public TError ExpectError(string msg)
            => IsError ? error : throw new Exception($"{msg}: {value}");
    }

    /// <inheritdoc />
    /// <summary>
    ///     A <see cref="Result{TResult,TError}" /> representing an error.
    /// </summary>
    public sealed class Error<TResult, TError> : Result<TResult, TError>
    {
        internal Error(TError error) : base(error) { }
    }

    /// <inheritdoc />
    /// <summary>
    ///     A <see cref="Result{TResult,TError}" /> representing a value.
    /// </summary>
    public sealed class Ok<TResult, TError> : Result<TResult, TError>
    {
        internal Ok(TResult value) : base(value) { }
    }
}
