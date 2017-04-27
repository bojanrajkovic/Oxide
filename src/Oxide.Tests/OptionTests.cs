using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

using static Oxide.Options;

namespace Oxide.Tests
{
    public class OptionTests
    {
        public static IEnumerable<object[]> Options()
        {
            yield return new object[] { Some(5L) };
            yield return new object[] { None<long>() };
        }

#region IsNone/IsSome Tests
        [Fact]
        public void SomeIsNotNone() => Assert.False(Some(1).IsNone);

        [Fact]
        public void SomeIsSome() => Assert.True(Some(1).IsSome);

        [Fact]
        public void NoneIsNone() => Assert.True(None<int>().IsNone);

        [Fact]
        public void NoneIsNotSome() => Assert.False(None<int>().IsSome);
#endregion


#region Equality Tests
        [Theory]
        [MemberData(nameof(Options))]
        public void Is_never_equal_to_null(Option opt)
            => Assert.False(opt.Equals(null));

        [Theory]
        [MemberData(nameof(Options))]
        public void Op_equality_is_correct_for_null(Option opt)
            => Assert.False(opt == null);

        [Theory]
        [MemberData(nameof(Options))]
        public void Op_inequality_is_correct_for_null(Option opt)
            => Assert.True(opt != null);

        [Fact]
        public void Some_is_not_equal_to_none()
            => Assert.False(Some(5).Equals(None<int>()));

        [Fact]
        public void Op_equality_is_correct_for_some_and_none()
            => Assert.False(Some(5) == None<int>());

        [Fact]
        public void Op_inequality_is_correct_for_some_and_none()
            => Assert.True(Some(5) != None<int>());

        [Fact]
        public void None_is_equal_to_none()
            => Assert.True(None<int>().Equals(None<int>()));

        [Fact]
        public void Op_equality_is_correct_for_none()
            => Assert.True(None<int>() == None<int>());

        [Fact]
        public void Op_inequality_is_correct_for_none()
            => Assert.False(None<int>() != None<int>());

        [Fact]
        public void Somes_with_different_values_are_not_equal()
            => Assert.False(Some(5).Equals(Some(10)));

        [Fact]
        public void Op_equality_is_correct_for_some_with_different_value()
            => Assert.False(Some(5) == Some(10));

        [Fact]
        public void Op_inequality_is_correct_for_some_with_different_value()
            => Assert.True(Some(5) != Some(10));

        [Fact]
        public void Somes_with_same_value_are_equal()
            => Assert.True(Some(5).Equals(Some(5)));

        [Fact]
        public void Op_equality_is_correct_for_somes_with_same_value()
            => Assert.True(Some(5) == Some(5));

        [Fact]
        public void Op_inequality_is_correct_for_somes_with_same_value()
            => Assert.False(Some(5) != Some(5));

        [Fact]
        public void Object_equals_is_correct_for_non_options()
            => Assert.False(Some(5).Equals(new object()));

        [Fact]
        public void Object_equals_is_correct_for_matching_values()
            => Assert.True(Some(5).Equals((object)Some(5)));

        [Fact]
        public void Object_equals_is_correct_for_mismatched_values()
            => Assert.False(Some(5).Equals((object)Some(10)));

        [Fact]
        public void Object_equals_is_correct_for_mismatched_types()
            => Assert.False(Some(5).Equals(Some(10L)));

        [Fact]
        public void Object_equals_is_correct_for_some_and_none()
            => Assert.False(Some(5).Equals((object)None<int>()));

        [Fact]
        public void Object_equals_is_correct_for_none_and_none()
            => Assert.True(None<int>().Equals((object)None<int>()));
#endregion

#region Expect/Unwrap Tests
        [Fact]
        public void Expecting_some_returns_value()
            => Assert.Equal(Some(5).Expect("Expected value, got None."), 5);

        [Fact]
        public void Expecting_none_throws_exception_matching_string()
        {
            string message = "Expected 5, got None.";
            var ex = Assert.Throws<Exception>(() => None<int>().Expect(message));
            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public void Unwrapping_some_returns_value()
            => Assert.Equal(Some(5).Unwrap(), 5);

        [Fact]
        public void Unwrapping_none_throws_exception() {
            var ex = Assert.Throws<Exception>(() => None<int>().Unwrap());
            Assert.Equal("Tried to unwrap a None<System.Int32>!", ex.Message);
        }

        [Fact]
        public void Unwrap_or_some_returns_some()
            => Assert.Equal(Some(5).UnwrapOr(10), 5);

        [Fact]
        public void Unwrap_or_none_returns_given_value()
            => Assert.Equal(None<int>().UnwrapOr(10), 10);

        [Fact]
        public void Unwrap_or_none_with_unspecified_value_returns_default()
            => Assert.Equal(None<long>().UnwrapOr(), default(long));

        [Fact]
        public void Unwrap_or_function_does_not_call_function_with_some()
        {
            var called = false;
            long Or() { called = true; return 10; }
            var result = Some(5L).UnwrapOr(Or);

            Assert.Equal(5, result);
            Assert.False(called);
        }

        [Fact]
        public void Unwrap_or_function_does_call_function_with_none()
        {
            var called = false;
            long Or() { called = true; return 10; }
            var result = None<long>().UnwrapOr(Or);

            Assert.Equal(10, result);
            Assert.True(called);
        }
#endregion

#region Map Tests
        [Fact]
        public void Mapping_some_returns_some_with_value()
        {
            double Mapper(long value) => Math.Pow(2, value);
            var some = Some(10L);

            var result = some.Map(Mapper);

            Assert.IsType<Some<double>>(result);
            Assert.NotSame(some, result);
            Assert.Equal(1024, result.Unwrap());
        }

        [Fact]
        public void Mapping_none_returns_none()
        {
            double Mapper(long value) => Math.Pow(2, value);
            var none = None<long>();

            var result = none.Map(Mapper);

            Assert.IsType<None<double>>(result);
            Assert.True(result.IsNone);
        }

        [Fact]
        public async Task Async_mapping_returns_awaitable_option()
        {
            Task<double> Mapper(long value) => Task.Run(() => Math.Pow(2, value));
            var some = Some(16L);

            var result = some.MapAsync(Mapper);

            var newSome = await result;
            Assert.IsType<Some<double>>(newSome);
            Assert.Equal(65536, newSome.Unwrap());
        }

        [Fact]
        public async Task Async_mapping_on_a_none_returns_none()
        {
            Task<double> Mapper(long value) => Task.Run(() => Math.Pow(2, value));
            var none = None<long>();

            var result = await none.MapAsync(Mapper);

            Assert.IsType<None<double>>(result);
            Assert.True(result.IsNone);
        }

        [Fact]
        public async Task Can_unwrap_task_of_option_into_task_of_t()
        {
            Task<double> Mapper(long value) => Task.Run(() => Math.Pow(2, value));
            var some = Some(16L);

            var result = await some.MapAsync(Mapper).Unwrap();
            Assert.Equal(65536.0, result);
        }

        [Fact]
        public async Task Unwrapping_task_of_none_throws_as_expected()
        {
            Task<double> Mapper(long value) => Task.Run(() => Math.Pow(2, value));
            var none = None<long>();

            var ex = await Assert.ThrowsAsync<Exception>(
                () => none.MapAsync(Mapper).Unwrap()
            );
            Assert.Equal("Tried to unwrap a None<System.Double>!", ex.Message);
        }

        [Fact]
        public void Map_or_converts_value_if_some()
            => Assert.Equal(
                65536,
                Some(16L).MapOr(
                    10.0,
                    val => Math.Pow(2, val)
                )
            );

        [Fact]
        public void Map_or_returns_default_value_if_none()
            => Assert.Equal(
                10.0,
                None<long>().MapOr(
                    10.0,
                    val => Math.Pow(2, val)
                )
            );

        [Fact]
        public void Map_or_with_function_converts_value_if_some()
            => Assert.Equal(
                65536,
                Some(16L).MapOr(
                    () => 10.0,
                    val => Math.Pow(2, val)
                )
            );

        [Fact]
        public void Map_or_with_function_provides_value_if_none()
            => Assert.Equal(
                10.0,
                None<long>().MapOr(
                    () => 10.0,
                    val => Math.Pow(2, val)
                )
            );
#endregion

#region And Tests
        [Theory]
        [MemberData(nameof(Options))]
        public void Some_and_option_returns_option(Option<long> opt)
            => Assert.True(Some(1L).And(opt) == opt);

        [Theory]
        [MemberData(nameof(Options))]
        public void None_and_option_returns_none(Option<long> opt)
            => Assert.True(None<long>().And(opt).IsNone);

        [Fact]
        public void Some_and_then_func_returns_transform()
            => Assert.Equal(50, Some(10).AndThen<int>(val => 5*val));

        [Fact]
        public void None_and_then_returns_none_of_correct_type()
        {
            var result = None<int>().AndThen<string>(val => val.ToString());
            Assert.IsType<None<string>>(result);
            Assert.True(result.IsNone);
        }

        [Fact]
        public async Task Async_some_and_then_returns_transformed_value()
        {
            var timespan = TimeSpan.FromSeconds(1);
            var some = Some(timespan);

            var res = await some.AndThenAsync<double>(async ts => {
                await Task.Delay(ts);
                return ts.TotalDays;
            }).Unwrap();

            Assert.Equal(timespan.TotalDays, res);
        }

        [Fact]
        public async Task Async_none_and_then_returns_none()
        {
            var none = None<TimeSpan>();
            var res = await none.AndThenAsync<double>(async ts => {
                await Task.Delay(ts);
                return ts.TotalDays;
            });

            Assert.IsType<None<double>>(res);
            Assert.True(res.IsNone);
        }
#endregion

#region Or Tests
        [Fact]
        public void Some_or_other_returns_original()
            => Assert.Equal(10, Some(10).Or(5).Unwrap());

        [Fact]
        public void None_or_other_returns_other()
            => Assert.Equal(5, None<int>().Or(5).Unwrap());

        [Fact]
        public void Some_or_else_other_returns_original()
            => Assert.Equal(10, Some(10).OrElse(() => 5).Unwrap());

        [Fact]
        public void None_or_else_other_returns_other()
            => Assert.Equal(5, None<int>().OrElse(() => 5).Unwrap());

        [Fact]
        public async Task Async_some_or_else_returns_this()
        {
            var timespan = TimeSpan.FromSeconds(1);
            var some = Some(timespan);
            var called = false;

            var res = await some.OrElseAsync(async () => {
                await Task.Delay(timespan);
                called = true;
                return TimeSpan.FromSeconds(10);
            });

            Assert.Same(some, res);
            Assert.False(called);
        }

        [Fact]
        public async Task Async_none_or_else_returns_other()
        {
            var timespan = TimeSpan.FromSeconds(1);
            var none = None<TimeSpan>();
            var called = false;

            var res = await none.OrElseAsync(async () => {
                await Task.Delay(timespan);
                called = true;
                return TimeSpan.FromSeconds(10);
            });

            Assert.NotSame(none, res);
            Assert.True(called);
            Assert.Equal(TimeSpan.FromSeconds(10), res.Unwrap());
        }
#endregion

#region Take Tests
        [Fact]
        public void Option_is_none_after_take()
        {
            var some = Some(100);

            Assert.True(some.IsSome);
            Assert.False(some.IsNone);

            some.Take();

            Assert.False(some.IsSome);
            Assert.True(some.IsNone);
        }
#endregion

#region Hashcode Tests
        [Fact]
        public void Get_hash_code_returns_zero_for_none()
            => Assert.Equal(0, None<int>().GetHashCode());

        [Fact]
        public void Get_hash_code_for_some_with_null_returns_minus_one()
            => Assert.Equal(-1, Some<string>(null).GetHashCode());

        [Fact]
        public void Get_hash_code_returns_hashcode_of_value()
        {
            var obj = new object();
            Assert.Equal(obj.GetHashCode(), Some(obj).GetHashCode());
        }
#endregion

#region Miscellaneous Tests
        [Fact]
        public void ReferenceTypeCanBeSomeWithNull()
        {
            var some = Some<string>(null);

            Assert.True(some.IsSome);
            Assert.False(some.IsNone);
            Assert.Null(some.Unwrap());
        }
#endregion
    }
}
