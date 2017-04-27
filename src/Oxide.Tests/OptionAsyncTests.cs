using Xunit;

using static Oxide.Options;

namespace Oxide.Tests
{
	public class OptionAsyncTests
	{
		[Fact]
		void Good_computation_should_return_some()
		{
			var result = GoodComputation();
			Assert.IsType<Some<int>>(result);
			Assert.Equal(15, result.Unwrap());
		}

		async Option<int> GoodComputation()
		{
			var val1 = await TryDivide(120, 2);
			var val2 = await TryDivide(val1, 2);
			var val3 = await TryDivide(val2, 2);

			return val3;
		}

		[Fact]
		void Bad_computation_should_return_none()
		{
			var result = BadComputation();
			Assert.IsType<None<int>>(result);
		}

		[Fact]
		void Bad_computation_should_not_execute_beyond_error()
		{
			var helper = new TestHelper();
			var result = BadComputation(helper);
			Assert.True(helper.ReachedBeforeError);
			Assert.False(helper.ReachedAfterError);
		}

		async Option<int> BadComputation(TestHelper helper = null)
		{
			var val1 = await TryDivide(120, 2);
			if (helper != null)
				helper.ReachedBeforeError = true;
			var val2 = await TryDivide(val1, 0);
			if (helper != null)
				helper.ReachedAfterError = true;
			var val3 = await TryDivide(val2, 2);

			return val3;
		}

		static Option<int> TryDivide(int up, int down)
		{
			if (down == 0)
				return None<int>();

			return Some (up / down);
		}

		class TestHelper
		{
			public bool ReachedBeforeError;
			public bool ReachedAfterError;
		}
	}
}
