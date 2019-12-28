using System;
using System.Threading.Tasks;

using static Oxide.Options;

namespace Oxide
{
    /// <summary>
    ///     Contains helper methods and extension methods for working with <see cref="Option{TOption}" />.
    /// </summary>
    public static class Options
    {
        /// <summary>
        ///     Creates a new <see cref="Option{TOption}" /> that is a <see cref="None{T}" />, which does not
        ///     contain a value.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the value possibly contained within the <see cref="Option{TOption}" />.
        /// </typeparam>
        /// <returns>An option with no value.</returns>
        public static Option<T> None<T>() => new None<T>();

        /// <summary>
        ///     Creates a new <see cref="Option{TOption}" /> that is a <see cref="Some{T}" />, which contains
        ///     the given value.
        /// </summary>
        /// <param name="value">The value that the option contains.</param>
        /// <typeparam name="T">The type of the value contained within the <see cref="Option{TOption}" />.</typeparam>
        /// <returns>A <see cref="Some{T}" /> that contains the value <paramref name="value" />.</returns>
        public static Option<T> Some<T>(T value) => new Some<T>(value);

        /// <summary>
        ///     Awaits and unwraps a <see cref="Task{TResult}" /> that contains a <see cref="Option{TOption}" />.
        /// </summary>
        /// <param name="self">The task to unwrap.</param>
        /// <typeparam name="T">The type of the value possibly contained within the <see cref="Option{TOption}" />.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous unwrapping, which wraps the unwrapped value of the option.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the underlying <see cref="Option{TOption}" /> is a
        ///     <see cref="None{T}" />.
        /// </exception>
        public static async Task<T> UnwrapAsync<T>(this Task<Option<T>> self)
            => (await self.ConfigureAwait(false)).Unwrap();

        /// <summary>
        ///     Awaits the <see cref="Task{TResult}" /> in <paramref name="self" />, and chains the resulting
        ///     <see cref="Option{TOption}" /> with <paramref name="continuation" /> by calling
        ///     <see cref="Option{TOption}.AndThen{TResult}" />.
        /// </summary>
        /// <param name="self">The task to continue.</param>
        /// <param name="continuation">The function to call on the wrapped value.</param>
        /// <typeparam name="TIn">The type of the wrapped value.</typeparam>
        /// <typeparam name="TOut">The output type of the continuation.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous chaining of two options, which wraps the
        ///     result of calling <paramref name="continuation" /> with the wrapped value.
        /// </returns>
        /// <remarks>
        ///     This is a convenience method to allow chaining async computations in a natural way. Without this,
        ///     the syntax gymnastics around <c>await</c> can become strenuous. This is less of an issue with
        ///     <see cref="Option{TOption}.AndThen{TResult}" /> because it is not async-oriented, but the syntax is
        ///     still cleaner. Consider:
        ///     <code>
        /// <![CDATA[
        /// Task<Option<SomeResult>> task = GetSomeResultAsync(...);
        /// Option<TransformedResult> finalResult = (await task).AndThen(...);
        /// ]]>
        ///     </code>
        ///     vs.
        ///     <code>
        /// <![CDATA[
        /// var finalResult = await GetSomeResultAsync(...).AndThenAsync(...);
        /// ]]>
        ///     </code>
        /// </remarks>
        public static async Task<Option<TOut>> AndThenAsync<TIn, TOut>(
            this Task<Option<TIn>> self,
            Func<TIn, Option<TOut>> continuation
        ) => (await self).AndThen(continuation);


        /// <summary>
        ///     Awaits the <see cref="Task{TResult}" /> in <paramref name="self" />, and chains the resulting
        ///     <see cref="Option{TOption}" /> with <paramref name="continuation" />
        ///     by calling <see cref="Option{TOption}.AndThenAsync{TResult}" />.
        /// </summary>
        /// <param name="self">The task to continue.</param>
        /// <param name="continuation">The function to call on the wrapped value.</param>
        /// <typeparam name="TIn">The type of the wrapped value.</typeparam>
        /// <typeparam name="TOut">The output type of the continuation.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous chaining of two options, which wraps the result of calling
        ///     <paramref name="continuation" /> with the wrapped value.
        /// </returns>
        /// <remarks>
        ///     This is a convenience method to allow chaining async computations in a natural way. Without this, the
        ///     syntax gymnastics around <c>await</c> can become strenuous. Consider the following:
        ///     <code>
        /// <![CDATA[
        /// await (await (await GetSomeResultAsync()).AndThenAsync(...)).AndThenAsync(....);
        ///
        /// // or
        ///
        /// var first = await GetSomeResultAsync();
        /// var second = await first.AndThenAsync(...);
        /// var third = await second.AndThenAsync(...);
        /// ]]>
        /// </code>
        ///     vs.
        ///     <code>
        /// <![CDATA[
        /// var someOptionTask = GetSomeResultAsync(...);
        /// await someOptionTask.AndThenAsync(...).AndThenAsync(...).AndThenAsync(...);
        /// ]]>
        /// </code>
        /// </remarks>
        public static async Task<Option<TOut>> AndThenAsync<TIn, TOut>(
            this Task<Option<TIn>> self,
            Func<TIn, Task<Option<TOut>>> continuation
        ) => await (await self).AndThenAsync(continuation);
    }

    /// <summary>
    ///     The abstract base class for all Option types.
    /// </summary>
    public abstract class Option
    {
        private protected readonly bool HasValue;
        private protected Option(bool hasValue) => HasValue = hasValue;

        /// <summary>
        ///     Gets whether this <see cref="Option" /> does not contain a value.
        /// </summary>
        public bool IsNone => !HasValue;

        /// <summary>
        ///     Gets whether this <see cref="Option" /> contains a value.
        /// </summary>
        public bool IsSome => !IsNone;
    }

    /// <summary>
    ///     The <see cref="Option{TOption}" /> type represents an optional value. Every option is either
    ///     a <see cref="Some{T}" />, and has a value, or is a <see cref="None{T}" /> that has no value.
    /// </summary>
    /// <remarks>
    ///     Option types can be useful in a number of places:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Replace <c>null</c> or other "no value" markers (initial values, optional parameters, etc.)</description>
    ///         </item>
    ///         <item>
    ///             <description>Return values for partially defined (in the mathematical sense) functions</description>
    ///         </item>
    ///         <item>
    ///             <description>Return values for simple error reporting, where a <see cref="None{T}" /> can be returned</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <typeparam name="TOption">The type of the wrapped value.</typeparam>
    public abstract class Option<TOption> : Option, IEquatable<Option<TOption>>
    {
        static readonly string UnwrapMessage = $"Tried to unwrap a None<{typeof(TOption)}>!";
        readonly TOption value;

        private protected Option() : base(false) { }
        private protected Option(TOption value) : base(true) { this.value = value; }

        /// <inheritdoc />
        /// <remarks>
        ///     Equality for <see cref="Option{TOption}" /> is determined as follows:
        ///     <list type="numbered">
        ///         <item>
        ///             <description>If <paramref name="other" /> is null, return <c>false</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>If they are not both <see cref="Some{T}" /> or <see cref="None{T}" />, return <c>false</c>.</description>
        ///         </item>
        ///         <item>
        ///             <description>If both are <see cref="None{T}" />, return true.</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Compare the values in both <see cref="Some{T}" />, using
        ///                 <see cref="object.Equals(object,object)" />.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public bool Equals(Option<TOption> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (HasValue != other.HasValue)
                return false;

            if (IsNone && other.IsNone)
                return true;

            return Equals(value, other.value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is Option<TOption> option && Equals(option);

        /// <summary>
        ///     Determines whether the two objects are equal to each other.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns><c>true</c> if the objects are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Option<TOption> left, Option<TOption> right)
            => !ReferenceEquals(left, null) && left.Equals(right);

        /// <summary>
        ///     Determines whether the two objects are not equal to each other.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns><c>true</c> if the objects are not equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(Option<TOption> left, Option<TOption> right)
            => !ReferenceEquals(left, null) && !left.Equals(right);

        /// <summary>
        ///     Implicitly converts a <typeparamref name="TOption" /> value to an <see cref="Option{TOption}" />.
        /// </summary>
        /// <param name="value">The value to wrap in an option.</param>
        /// <returns>
        ///     A <see cref="Some{T}" /> wrapping the given value, or a <see cref="None{T}" /> if the value is <c>null</c>.
        /// </returns>
        public static implicit operator Option<TOption>(TOption value)
            => value is null ? None<TOption>() : Some(value);

        /// <summary>
        ///     Unwraps the option, yielding the content of a <see cref="Some{T}" />, throwing an exception with a
        ///     custom message provided by <paramref name="msg" /> if it is a <see cref="None{T}" />.
        /// </summary>
        /// <param name="msg">The custom exception message to throw.</param>
        /// <returns>The value in the option.</returns>
        public TOption Expect(string msg)
            => IsSome ? value : throw new InvalidOperationException(msg);

        /// <summary>
        ///     Unwraps the value out of the option if it is a <see cref="Some{T}" />, and throws
        ///     an exception otherwise.
        /// </summary>
        /// <returns>The value in the option.</returns>
        public TOption Unwrap()
            => IsSome ? value : throw new InvalidOperationException(UnwrapMessage);

        /// <summary>
        ///     Tries to unwrap the value out of the option if it is a <see cref="Some{T}" />, returning <c>true</c>
        ///     if a value was unwrapped, and <c>false</c> otherwise.
        /// </summary>
        /// <param name="val">A location to store the value into.</param>
        /// <returns><c>true</c> if a value was unwrapped, <c>false</c> otherwise.</returns>
        public bool TryUnwrap(out TOption val)
        {
            val = value;
            return IsSome;
        }

        /// <summary>
        ///     Unwraps the value out of the option if it is a <see cref="Some{T}" />, or returns <paramref name="def" />
        ///     otherwise.
        /// </summary>
        /// <param name="def">The value to return if the option is a <see cref="None{T}" />.</param>
        /// <returns>The value in the option, or <paramref name="def" />.</returns>
        /// <remarks>
        ///     <paramref name="def" /> is eagerly evaluated—if you want lazy evaluation, use
        ///     <see cref="UnwrapOr(Func{TOption})" />.
        /// </remarks>
        public TOption UnwrapOr(TOption def = default)
            => IsSome ? value : def;

        /// <summary>
        ///     Unwraps the value out of the option if it is a <see cref="Some{T}" />, or returns the result of calling
        ///     <paramref name="provider" /> otherwise.
        /// </summary>
        /// <param name="provider">The function to call to provide a default value.</param>
        /// <returns>The value in the option, or the result of calling <paramref name="provider" /> otherwise.</returns>
        public TOption UnwrapOr(Func<TOption> provider)
            => IsSome ? value : provider();

        /// <summary>
        ///     Maps the value of the option to an <see cref="Option{TResult}" /> by applying the <paramref name="converter" />
        ///     function.
        /// </summary>
        /// <param name="converter">The converter function.</param>
        /// <typeparam name="TResult">The type of the resulting value after conversion.</typeparam>
        /// <returns>A <see cref="Some{TResult}" /> if the option has a value, and a <see cref="None{TResult}" /> otherwise.</returns>
        public Option<TResult> Map<TResult>(Func<TOption, TResult> converter)
            => IsSome ? Some(converter(value)) : None<TResult>();

        /// <summary>
        ///     Maps the value of the option to an <see cref="Option{TResult}" /> by applying the
        ///     <paramref name="converter" /> function.
        /// </summary>
        /// <param name="converter">The conversion function.</param>
        /// <typeparam name="TResult">The type of the resulting value after conversion.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous conversion of the value to an <see cref="Option{TResult}" />, which
        ///     wraps the result of the conversion function, if the option is a <see cref="Some{T}" />. Otherwise, a
        ///     <see cref="None{T}" /> is returned.
        /// </returns>
        public async Task<Option<TResult>> MapAsync<TResult>(Func<TOption, Task<TResult>> converter)
            => IsSome ? Some(await converter(value).ConfigureAwait(false)) : new None<TResult>();

#pragma warning disable 1574
        /// <summary>
        ///     Maps a <see cref="Some{T}"/> to a <typeparamref name="TResult" /> by applying the <paramref name="converter" />
        ///     function,
        ///     or returns <paramref name="def" /> otherwise.
        /// </summary>
        /// <param name="converter">The conversion function.</param>
        /// <param name="def">The value to return if the option is a <see cref="None{T}" />.</param>
        /// <typeparam name="TResult">The type of the resulting value after conversion.</typeparam>
        /// <returns>
        ///     The <see cref="TResult" /> output of the conversion function if the option has a value, or
        ///     <paramref name="def" /> if it is a <see cref="None{T}" />.
        /// </returns>
        /// <remarks>
        ///     <paramref name="def" /> is eagerly evaluated—if you want lazy evaluation, use
        ///     <see cref="MapOr{TResult}(System.Func{TOption,TResult},System.Func{TResult})" />.
        /// </remarks>
        public TResult MapOr<TResult>(Func<TOption, TResult> converter, TResult def)
            => IsSome ? converter(value) : def;
#pragma warning restore 1574

        /// <summary>
        ///     Maps a <see cref="Some{T}" /> to a <typeparamref name="TResult" /> by
        ///     applying the <paramref name="converter" /> function or returns the result of calling
        ///     <paramref name="provider" /> otherwise.
        /// </summary>
        /// <param name="converter">The conversion function.</param>
        /// <param name="provider">The function to call to provide a default value.</param>
        /// <typeparam name="TResult">The type of the resulting value after conversion.</typeparam>
        /// <returns>
        ///     The <typeparamref name="TResult" /> output of the conversion function if the option has a value, or the
        ///     result of calling <paramref name="provider" /> if it is a <see cref="None{T}" />.
        /// </returns>
        public TResult MapOr<TResult>(Func<TOption, TResult> converter, Func<TResult> provider)
            => IsSome ? converter(value) : provider();

        /// <summary>
        ///     Chains two options together, discarding the result of the current option.
        /// </summary>
        /// <param name="option">The other option.</param>
        /// <typeparam name="TResult">The type of the other option.</typeparam>
        /// <returns><see cref="None{T}" /> if the option is a <see cref="None{T}" />, otherwise <paramref name="option" />.</returns>
        /// <remarks>
        ///     <paramref name="option" /> is eagerly evaluated. If lazy evaluation is desired, use
        ///     <see cref="AndThen{TResult}" />.
        /// </remarks>
        public Option<TResult> And<TResult>(Option<TResult> option)
            => IsNone ? None<TResult>() : option;

        /// <summary>
        ///     Chains two options together if the option is <see cref="Some{T}" />, passing the value of the option to
        ///     the chained option provider <paramref name="optionProvider" />.
        /// </summary>
        /// <param name="optionProvider">The function to call to get the resulting <see cref="Option{TOption}" />.</param>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <returns>
        ///     The result of calling <paramref name="optionProvider" /> with the value of the option, if it is a
        ///     <see cref="Some{T}" />, or <see cref="None{T}" /> otherwise.
        /// </returns>
        /// <remarks>
        ///     <paramref name="optionProvider" /> is lazily evaluated, and is passed the value of the option, unlike
        ///     <see cref="And{TResult}" />.
        /// </remarks>
        public Option<TResult> AndThen<TResult>(Func<TOption, Option<TResult>> optionProvider)
            => IsNone ? None<TResult>() : optionProvider(value);

        /// <summary>
        /// Chains a continuation without modifying the option, if the option is a <see cref="Some{T}"/>.
        /// </summary>
        /// <param name="action">The action to call.</param>
        /// <returns>The option itself.</returns>
        public Option<TOption> Finally(Action<TOption> action)
        {
            if (IsSome) {
                action(value);
            }

            return this;
        }

        /// <summary>
        /// Chains a continuation without modifying the option if the option is a <see cref="None{T}"/>.
        /// </summary>
        /// <param name="action">The action to call.</param>
        /// <returns>The option itself.</returns>
        public Option<TOption> IfNone(Action action)
        {
            if (IsNone) {
                action();
            }

            return this;
        }

        /// <summary>
        ///     Chains two options together if the option is <see cref="Some{T}" />, passing the value of the option to
        ///     the chained option provider <paramref name="optionProvider" />.
        /// </summary>
        /// <param name="optionProvider">The function to call to get the resulting <see cref="Option{TOption}" />.</param>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <returns>
        ///     A task that represents the asynchronous conversion of the value to an <see cref="Option{TResult}" />, which wraps
        ///     the result of calling the conversion function, if the option is a <see cref="Some{T}" />. Otherwise, a
        ///     <see cref="None{T}" /> is returned.
        /// </returns>
        /// <remarks>
        ///     <paramref name="optionProvider" /> is lazily evaluated, and is passed the value of the option, unlike
        ///     <see cref="And{TResult}" />.
        /// </remarks>
        public Task<Option<TResult>> AndThenAsync<TResult>(Func<TOption, Task<Option<TResult>>> optionProvider)
            => IsNone ? Task.FromResult(None<TResult>()) : optionProvider(value);

        /// <summary>
        ///     Chains two options together.
        /// </summary>
        /// <param name="other">The other option.</param>
        /// <returns>The option if it is a <see cref="Some{T}" />, <paramref name="other" /> otherwise.</returns>
        /// <remarks><paramref name="other" /> is eagerly evaluated. If lazy evaluation is desired, use <see cref="OrElse" />.</remarks>
        public Option<TOption> Or(Option<TOption> other)
            => IsSome ? this : other;

        /// <summary>
        ///     Chains two options together, calling <paramref name="optionProvider" /> to provide a result if the option is a
        ///     <see cref="None{T}" />.
        /// </summary>
        /// <param name="optionProvider">The function to call to get the resulting <see cref="Option{TOption}" />.</param>
        /// <returns>
        ///     The option if it is a <see cref="Some{T}" />, or the result of calling <paramref name="optionProvider" />
        ///     otherwise.
        /// </returns>
        /// <remarks><paramref name="optionProvider" /> is lazily evaluated.</remarks>
        public Option<TOption> OrElse(Func<Option<TOption>> optionProvider)
            => IsSome ? this : optionProvider();

        /// <summary>
        ///     Chains two options together, calling <paramref name="optionProvider" /> to provide a result if the option is a
        ///     <see cref="None{T}" />.
        /// </summary>
        /// <param name="optionProvider">The function to call to get the resulting <see cref="Task{TResult}" />.</param>
        /// .
        /// <returns>
        ///     A task that represents the asynchronous chaining of two options, which wraps the option if it is
        ///     a <see cref="Some{T}" />, or the result of calling <paramref name="optionProvider" /> otherwise.
        /// </returns>
        public Task<Option<TOption>> OrElseAsync(Func<Task<Option<TOption>>> optionProvider)
            => IsSome ? Task.FromResult(this) : optionProvider();

        /// <inheritdoc />
        public override int GetHashCode()
            => !HasValue ? 0 : ReferenceEquals(value, null) ? -1 : value.GetHashCode();
    }

    /// <summary>
    ///     An <see cref="Option{TOption}" /> representing no value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public sealed class None<T> : Option<T>
    {
    }

    /// <summary>
    ///     An <see cref="Option{TOption}" /> representing some value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public sealed class Some<T> : Option<T>
    {
        internal Some(T value) : base(value) { }
    }
}
