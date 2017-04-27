using System;
using System.Runtime.CompilerServices;

using static Oxide.Options;

namespace Oxide
{
	public struct OptionAsyncMethodBuilder<T>
	{
		Option<T> task;

		OptionAsyncMethodBuilder(Option<T> initialValue)
		{
			task = initialValue;
		}

		public static OptionAsyncMethodBuilder<T> Create() => new OptionAsyncMethodBuilder<T>(None<T>());

		public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			// Simply start the state machine which will execute our code
			stateMachine.MoveNext();
		}

		public Option<T> Task => task;

		public void SetStateMachine(IAsyncStateMachine stateMachine) { }
		public void SetResult(T result) => task = Some(result);
		public void SetException(Exception ex) { /* We leave the result to None */ }

		public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
				where TAwaiter : INotifyCompletion
				where TStateMachine : IAsyncStateMachine
		{
			throw new NotSupportedException();
		}

		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
				where TAwaiter : ICriticalNotifyCompletion
				where TStateMachine : IAsyncStateMachine
		{
			throw new NotSupportedException();
		}
	}

	public struct OptionAwaiter<T> : INotifyCompletion
	{
		Option<T> option;

		public OptionAwaiter(Option<T> option)
		{
			this.option = option;
		}

		public bool IsCompleted => option.IsSome;

		public void OnCompleted(Action continuation)
		{
			/* We never need to execute the continuation cause
			 * we only reach here when the result is None which
			 * means we are trying to short-circuit everything
			 * else
			 */
		}

		public T GetResult() => ((Some<T>)option).Unwrap();
	}

	public static class OptionAsyncExtensions
	{
		public static OptionAwaiter<T> GetAwaiter<T>(this Option<T> option)
			=> new OptionAwaiter<T>(option);
	}
}
