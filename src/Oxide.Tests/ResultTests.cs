using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;

using static Oxide.Results;

namespace Oxide.Tests
{
    public class ResultTests
    {
#region Exception Is Captured
        Result<string, Exception> ReadAllText()
        {
            try {
                return File.ReadAllText("test.txt");
            } catch (Exception e) {
                return e;
            }
        }

        [Fact]
        public void Exception_is_captured_correctly()
        {
            var res = ReadAllText();
            Exception ex = null;

            try {
                res.Unwrap ();
            } catch (Exception e) {
                ex = e;
            }

            Assert.NotNull (ex);
            var stack = new StackTrace(ex, true);

            // If the exception was not captured correctly,
            // but rather `throw ex` was used, the stack
            // would be mangled and would appear to be only in
            // Unwrap and Throw in Result. With a properly
            // captured stack, the frame will include `ReadAllText`.
            var ratFrame = stack.GetFrames().SingleOrDefault(
                f => f.GetMethod() ==
                typeof(ResultTests).GetMethod("ReadAllText", BindingFlags.Instance | BindingFlags.NonPublic)
            );

            Assert.NotNull(ratFrame);
        }
#endregion

#region Deconstruction
        [Fact]
        public void Can_deconstruct_result_into_value_and_error()
        {
            var res = Ok<int, Exception>(5);
            var (number, exception) = res;

            Assert.Equal(5, number);
            Assert.Null(exception);
        }
#endregion

#region Equality Tests
        [Fact]
        public void Result_equals_null_is_false()
            => Assert.False(Ok<int, string>(5).Equals(null));

        [Fact]
        public void Result_equalsequals_null_is_false()
            => Assert.False(Ok<int, string>(5) == null);

        [Fact]
        public void Result_not_equals_null_is_true()
            => Assert.True(Ok<int, string>(5) != null);

        [Fact]
        public void Result_not_equals_error()
            => Assert.False(Ok<int, string>(5).Equals(Err<int, string>("5")));

        [Fact]
        public void Result_equals_other_result_if_values_match()
            => Assert.True(Ok<int, string>(5).Equals(Ok<int, string>(5)));

        [Fact]
        public void Result_does_not_equal_other_result_if_values_mismatched()
            => Assert.False(Ok<int, string>(5).Equals(Ok<int, string>(10)));

        [Fact]
        public void Error_equals_other_error_if_errors_match()
            => Assert.True(Err<int, string>("error").Equals(Err<int, string>("error")));

        [Fact]
        public void Error_does_not_equal_other_error_if_errors_mismatched()
            => Assert.False(Err<int, string>("error").Equals(Err<int, string>("mistake")));

        [Fact]
        public void Result_does_not_equal_error_via_object()
            => Assert.False(Ok<int, string>(5).Equals((object)Err<int, string>("error")));

        [Fact]
        public void Result_does_equal_other_result_via_object()
            => Assert.True(Ok<int, string>(5).Equals((object)Ok<int, string>(5)));

        [Fact]
        public void Result_does_not_equal_result_via_object_if_values_mismatch()
            => Assert.False(Ok<int, string>(5).Equals((object)Ok<int, string>(10)));

        [Fact]
        public void Error_does_equal_other_error_via_object()
            => Assert.True(Err<int, string>("error").Equals((object)Err<int, string>("error")));

        [Fact]
        public void Error_does_not_equal_other_error_via_object_if_errors_mismatch()
            => Assert.False(Err<int, string>("error").Equals((object)Err<int, string>("mistake")));

        [Fact]
        public void Result_hashcode_is_minus_one_if_ok_and_value_is_null()
            => Assert.Equal(-1, Ok<string, string>(null).GetHashCode());

        [Fact]
        public void Result_hashcode_is_expected_hashcode_if_value_has_value()
            => Assert.Equal("taco".GetHashCode(), Ok<string, string>("taco").GetHashCode());

        [Fact]
        public void Result_hashcode_is_minus_two_if_error_and_error_is_null()
            => Assert.Equal(-2, Err<string, string>(null).GetHashCode());

        [Fact]
        public void Result_hashcode_is_expected_hashcode_if_error()
            => Assert.Equal("taco".GetHashCode(), Err<string, string>("taco").GetHashCode());
#endregion

#region IsOk/IsErr & Option<T> conversions
        [Fact]
        public void Is_ok_returns_true_on_Ok()
            => Assert.True(Ok<int, string>(5).IsOk);

        [Fact]
        public void Is_error_returns_false_on_Ok()
            => Assert.False(Ok<int, string>(5).IsError);

        [Fact]
        public void Is_error_returns_true_on_Error()
            => Assert.True(Err<int, string>("taco").IsError);

        [Fact]
        public void Is_ok_returns_false_on_Error()
            => Assert.False(Err<int, string>("taco").IsOk);

        [Fact]
        public void Ok_ok_converts_to_some()
            => Assert.IsType<Some<int>>(Ok<int, string>(5).Ok());

        [Fact]
        public void Error_ok_converts_to_none()
            => Assert.IsType<None<int>>(Err<int, string>("taco").Ok());

        [Fact]
        public void Ok_err_converts_to_none()
            => Assert.IsType<None<string>>(Ok<int, string>(5).Err());

        [Fact]
        public void Error_err_converts_to_some()
            => Assert.IsType<Some<string>>(Err<int, string>("taco").Err());
#endregion

#region Map Tests
        [Fact]
        public void Map_function_is_applied_to_ok()
            => Assert.Equal(10, Ok<int, string>(5).Map(i => i*2));

        [Fact]
        public void Map_on_error_returns_error()
            => Assert.Equal("taco", Err<int, string>("taco").Map(i => i*2));

        [Fact]
        public async Task Async_map_function_is_applied_to_ok()
            => Assert.Equal(10, await Ok<int, string>(5).MapAsync(async i => {
                await Task.Delay(TimeSpan.FromMilliseconds(i*100));
                return i*2;
            }));

        [Fact]
        public async Task Async_map_function_is_not_called_on_error()
        {
            var called = false;
            var result = await Err<int, string>("taco").MapAsync(async i => {
                called = true;
                await Task.Delay(TimeSpan.FromMilliseconds(i * 100));
                return i * 2;
            });

            Assert.Equal("taco", result);
            Assert.False(called);
        }

        [Fact]
        public void Map_err_on_ok_returns_ok_value()
            => Assert.Equal (10, Ok<int, string>(10).MapErr(s => s.Length).Unwrap());

        [Fact]
        public void Map_err_on_err_applies_mapper()
            => Assert.Equal (4, Err<int, string>("taco").MapErr(s => s.Length).Err());

        [Fact]
        public async Task Continue_task_of_result_without_await()
        {
            var task = Task.FromResult(Ok<int, string>(10));
            var result = await task.AndThenAsync(i => Ok<int, string>(i*5));

            Assert.True(result.IsOk);
            Assert.Equal(50, result.Unwrap());
        }

        [Fact]
        public async Task Continue_task_of_result_with_async_continuation()
        {
            var task = Task.FromResult(Ok<int, string>(10));
            var result = await task.AndThenAsync(async i => await Task.FromResult(Ok<int, string>(i*5)));

            Assert.True(result.IsOk);
            Assert.Equal(50, result.Unwrap());
        }

        [Fact]
        public async Task Async_map_err_on_ok_returns_ok()
            => Assert.Equal (10, await Ok<int, string>(10).MapErrAsync(async s => {
                var ts = TimeSpan.FromMilliseconds(s.Length * 100);
                await Task.Delay(ts);
                return ts;
            }));

        [Fact]
        public async Task Async_map_err_on_err_is_called()
            => Assert.Equal(Err<int, TimeSpan>(TimeSpan.FromMilliseconds(400)), await Err<int, string>("taco").MapErrAsync(async s => {
                var ts = TimeSpan.FromMilliseconds(s.Length * 100);
                await Task.Delay(ts);
                return ts;
            }));
#endregion

#region And Tests
        [Fact]
        public void Ok_and_result_returns_new_result()
            => Assert.Equal(4, Ok<int, string>(5).And<int>(4));

        [Fact]
        public void Err_and_result_returns_error()
            => Assert.Equal("taco", Err<int, string>("taco").And<int>(10));

        [Fact]
        public void Ok_and_then_result_returns_new_result()
            => Assert.Equal(10, Ok<int, string>(5).AndThen<int>(i => i*2));

        [Fact]
        public void Err_and_then_result_returns_error()
            => Assert.Equal("taco", Err<int, string>("taco").AndThen<int>(i => i*2));

        [Fact]
        public async Task Async_ok_and_then_result_returns_new_result()
            => Assert.Equal(10, await Ok<int, string>(5).AndThenAsync<int>(async i => {
                await Task.Delay(i*100);
                return i*2;
            }));

        [Fact]
        public async Task Async_err_and_then_result_returns_error() {
            var called = false;
            Assert.Equal("taco", await Err<int, string>("taco").AndThenAsync<int>(async i => {
                called = true;
                await Task.Delay(i*100);
                return i*2;
            }));
            Assert.False(called);
        }
#endregion

#region Or Tests
        [Fact]
        public void Ok_or_result_gives_value()
            => Assert.Equal(Ok<int, int>(10), Ok<int, string>(10).Or(Err<int, int>(5)));

        [Fact]
        public void Err_or_result_returns_err()
            => Assert.Equal(Err<int, int>(4), Err<int, string>("taco").Or(Err<int, int>(4)));

        [Fact]
        public void Ok_or_else_gives_value()
            => Assert.Equal(Ok<int, int>(10), Ok<int, string>(10).OrElse(err => Err<int, int>(err.Length)));

        [Fact]
        public void Err_or_else_returns_err()
            => Assert.Equal(Err<int, int>(4), Err<int, string>("taco").OrElse(err => Err<int, int>(err.Length)));

        [Fact]
        public async Task Async_ok_or_else_gives_value_and_is_not_called() {
            var called = false;
            Assert.Equal(Ok<int, int>(10), await Ok<int, string>(10).OrElseAsync(async s => {
                called = true;
                await Task.Delay(s.Length * 100);
                return Err<int, int>(s.Length);
            }));
            Assert.False(called);
        }

        [Fact]
        public async Task Async_error_or_else_transforms_error()
            => Assert.Equal(Err<int, TimeSpan>(TimeSpan.FromMilliseconds(400)), await Err<int, string>("taco").OrElseAsync<TimeSpan>(async s => {
                var ts = TimeSpan.FromMilliseconds(s.Length * 100);
                await Task.Delay(ts);
                return ts;
            }));
#endregion

#region Unwrap Tests
        [Fact]
        public void Unwrap_or_ok_returns_value()
            => Assert.Equal(11, Ok<int, string>(11).UnwrapOr(12));

        [Fact]
        public void Unwrap_or_err_returns_or()
            => Assert.Equal(12, Err<int, string>("taco").UnwrapOr(12));

        [Fact]
        public void Unwrap_or_else_ok_returns_value()
            => Assert.Equal(11, Ok<int, string>(11).UnwrapOrElse(err => 15));

        [Fact]
        public void Unwrap_or_else_err_returns_or()
            => Assert.Equal(4, Err<int, string>("taco").UnwrapOrElse(err => err.Length));

        [Fact]
        public async Task Async_unwrap_or_else_ok_returns_value()
            => Assert.Equal(11, await Ok<int, string>(11).UnwrapOrElseAsync(async err => await Task.FromResult(15)));

        [Fact]
        public async Task Async_unwrap_or_else_err_returns_transform()
            => Assert.Equal(4, await Err<int, string>("taco").UnwrapOrElseAsync(async err => await Task.FromResult(err.Length)));

        [Fact]
        public void Unwrap_ok_returns_value()
            => Assert.Equal("taco", Ok<string, int>("taco").Unwrap());

        [Fact]
        public void Unwrap_non_exception_error_throws_exception_with_object_as_string()
        {
            var ex = Assert.Throws<Exception>(() => Err<int, Uri>(new Uri("https://google.com")).Unwrap());
            Assert.Equal("https://google.com/", ex.Message);
        }

        [Fact]
        public void Unwrap_exception_rethrows_exception()
        {
            var thrown = new Exception("taco");
            var stack = thrown.StackTrace;
            var ex = Assert.Throws<Exception>(() => Err<int, Exception>(thrown).Unwrap());
            Assert.Equal("taco", ex.Message);
            Assert.Same(thrown, ex);

            // The stack is modified by being captured in the err constructor
            // and then being rethrown.
            Assert.NotEqual(stack, ex.StackTrace);
        }

        [Fact]
        public void Expect_ok_returns_value()
            => Assert.Equal("taco", Ok<string, int>("taco").Expect("beans"));

        [Fact]
        public void Expect_err_with_object_returns_formatted_string()
        {
            var ex = Assert.Throws<Exception>(() => Err<int, Uri>(new Uri("https://google.com")).Expect("Error"));
            Assert.Equal("Error: https://google.com/", ex.Message);
        }

        [Fact]
        public void Expect_err_with_exception_wraps_exception()
        {
            var thrown = new Exception("taco");
            var stack = thrown.StackTrace;
            var ex = Assert.Throws<Exception>(() => Err<int, Exception>(thrown).Expect("Error!"));
            Assert.Equal("Error!", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.Same(thrown, ex.InnerException);
            Assert.Equal(stack, ex.InnerException.StackTrace);
        }

        [Fact]
        public void Unwrap_error_on_OK_throws_exception()
        {
            var ex = Assert.Throws<Exception>(() => Ok<int, string>(5).UnwrapError());
            Assert.Equal("5", ex.Message);
        }

        [Fact]
        public void Unwrap_error_on_error_returns_error()
            => Assert.Equal("taco", Err<int, string>("taco").UnwrapError());

        [Fact]
        public void Expect_error_on_ok_throws_exception()
        {
            var ex = Assert.Throws<Exception>(() => Ok<int, string>(10).ExpectError("Expected error, got"));
            Assert.Equal("Expected error, got: 10", ex.Message);
        }

        [Fact]
        public void Expect_error_on_error_throws_exception()
            => Assert.Equal("taco", Err<int, string>("taco").ExpectError("Expected error, got"));

        [Fact]
        public void Unwrap_or_default_on_ok_returns_value()
            => Assert.Equal(10, Ok<int, string>(10).UnwrapOr());

        [Fact]
        public void Unwrap_or_default_on_err_returns_default_value()
            => Assert.Equal(default, Err<string, int>(5).UnwrapOr());

        [Fact]
        public void Can_try_unwrap_for_pattern_matching()
        {
            var ok = Ok<int, string>(10);
            switch (ok) {
                case var _ when ok.TryUnwrap(out var result):
                    Assert.Equal(10, result);
                    break;
                case var _ when ok.TryUnwrapError(out _):
                    Assert.True(false, "Should not be reached.");
                    break;
            }
        }

        public static IEnumerable<object[]> Results => new [] {
            new object[] { Ok<int, Exception>(5) },
            new object[] { Err<int, Exception>(new DivideByZeroException()) }
        };

        [Theory, MemberData(nameof(Results))]
        public void If_ok_passes_through_original_result(Result<int, Exception> res)
            => Assert.Same(res, res.IfOk(_ => { }));

        [Theory, MemberData(nameof(Results))]
        public void If_error_passes_through_original_result(Result<int, Exception> res)
            => Assert.Same(res, res.IfError(_ => { }));

        [Fact]
        public void If_ok_calls_function_when_ok()
        {
            var newVal = -1;
            var res = Ok<int, Exception>(5);

            var ret = res.IfOk(val => newVal = val * 2);

            Assert.Same(res, ret);
            Assert.Equal(10, newVal);
        }

        [Fact]
        public void If_ok_does_not_call_function_when_err()
        {
            var newVal = -1;
            var res = Err<int, Exception>(new DivideByZeroException());

            var ret = res.IfOk(val => newVal = val * 2);

            Assert.Same(res, ret);
            Assert.Equal(-1, newVal);
        }

        [Fact]
        public void If_err_calls_function_when_err()
        {
            Exception e = null;
            var res = Err<int, Exception>(new DivideByZeroException());

            var ret = res.IfError(err => e = err);

            Assert.Same(res, ret);
            Assert.NotNull(e);
            Assert.IsType<DivideByZeroException>(e);
        }

        [Fact]
        public void If_err_does_not_call_function_when_ok()
        {
            Exception e = null;
            var res = Ok<int, Exception>(5);

            var ret = res.IfError(err => e = err);

            Assert.Same(res, ret);
            Assert.Null(e);
        }

        [Fact]
        public void Can_try_unwrap_both_and_output_value()
        {
            var ok = Ok<int, string>(10);
            if (ok.TryUnwrap(out var value, out var error)) {
                Assert.Equal(10, value);
                Assert.Null(error);
            } else {
                Assert.False(true, "Should not be reached, TryUnwrap returned false for Ok<T, E>.");
            }
        }

        [Fact]
        public void Can_try_unwrap_both_and_output_error()
        {
            var error = Err<int, string>("error");
            if (error.TryUnwrap(out var value, out var err)) {
                Assert.False(true, "Should not be reached, TryUnwrap returned true for Error<T, E>.");
            } else {
                Assert.Equal(default, value);
                Assert.Equal("error", err);
            }
        }

        [Fact]
        public void Can_use_positional_patterns_with_ok()
        {
            var ok = Ok<int, string>(10);
            var val = ok switch {
                var (value, _) when ok.IsOk => value*10,
                var (_, _) when ok.IsError => -1,
                _ => 0
            };
            Assert.Equal(10*10, val);
        }

        [Fact]
        public void Can_use_positional_patterns_with_err()
        {
            var err = Err<int, string>("hello");
            var val = err switch {
                var (_, _) when err.IsOk => null,
                var (_, error) when err.IsError => error.ToUpper(),
                _ => null
            };
            Assert.Equal("HELLO", val);
        }

        [Fact]
        public void Can_try_unwrap_error_for_pattern_matching()
        {
            var ok = Err<int, string>("error");
            switch (ok) {
                case var _ when ok.TryUnwrap(out _):
                    Assert.True(false, "Should not be reached.");
                    break;
                case var _ when ok.TryUnwrapError(out var error):
                    Assert.Equal("error", error);
                    break;
            }
        }
        #endregion

        #region Combined result tests

        [Fact]
        public void Multiple_combined_OKs_return_all_values()
        {
            var r = new Random();
            var oks = Enumerable.Range(0, 10).Select(_ => Ok<int, string>(r.Next(1, 1001))).ToArray();
            var sum = oks.Sum(o => o.Unwrap());
            var combined = Combine(oks);

            Assert.True(combined.IsOk);
            Assert.Equal(sum, combined.AndThen<int>(values => values.Sum()));
        }

        [Fact]
        public void Combining_mix_of_OK_and_Err_returns_first_err()
        {
            var r = new Random();
            var results = Enumerable.Range(0, 10).Select(_ => {
                var rand = r.Next(0, 11);
                if (rand > 5)
                    return Err<int, string>(rand.ToString());
                return Ok<int, string>(rand);
            }).ToList();
            var firstError = results.First(res => res.IsError).UnwrapError();

            var combined = Combine(results);

            Assert.True(combined.IsError);
            Assert.Equal(firstError, combined.UnwrapError());
        }

        #endregion

    }
}
