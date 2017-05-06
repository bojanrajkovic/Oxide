using System;
using System.Globalization;
using System.Net;

using Xunit;

namespace Oxide.Tests
{
    public class ParseTests
    {
        class NonStaticTryParse
        {
            public bool TryParse(string foo, out NonStaticTryParse result)
            {
                result = new NonStaticTryParse();
                return true;
            }
        }

        [Fact]
        public void Parse_type_with_non_static_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<NonStaticTryParse>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_static_try_parse_returns_none()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<NonStaticTryParse>();

            Assert.True(parseResult.IsNone);
        }

        class TryParseNotReturningBool
        {
            public static int TryParse(string foo)
                => 5;
        }

        [Fact]
        public void Parse_type_with_non_bool_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<TryParseNotReturningBool>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_bool_try_parse_returns_none()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<TryParseNotReturningBool>();

            Assert.True(parseResult.IsNone);
        }

        class TryParseNotTakingString
        {
            public static bool TryParse(int foo)
                => true;
        }

        [Fact]
        public void Parse_type_with_non_string_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<TryParseNotTakingString>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_string_try_parse_returns_none()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<TryParseNotTakingString>();

            Assert.True(parseResult.IsNone);
        }

        class TryParseWithoutOutParameter
        {
            public static bool TryParse(string s, TryParseWithoutOutParameter fill)
                => true;
        }

        [Fact]
        public void Parse_type_with_non_out_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<TryParseWithoutOutParameter>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_out_try_parse_returns_none()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<TryParseWithoutOutParameter>();

            Assert.True(parseResult.IsNone);
        }

        [Fact]
        public void Can_parse_IP_address_via_generic_try_parse()
        {
            const string addrString = "192.168.1.1";
            var maybeIp = addrString.TryParse<IPAddress>();

            Assert.True(maybeIp.IsSome);
            var addr = maybeIp.Unwrap();
            Assert.Equal(addrString, addr.ToString());
        }

        [Fact]
        public void Can_parse_via_generic_parse()
        {
            const string intString = "deadbeef";
            var parseResult = intString.Parse<int>(NumberStyles.HexNumber, null);

            Assert.True(parseResult.IsOk);
            var @int = parseResult.Unwrap();
            Assert.Equal(intString, @int.ToString("x2"));
        }

        [Fact]
        public void Can_try_parse_with_additional_parameters()
        {
            const string shortString = "cafe";
            var maybeShort = shortString.TryParse<short>(NumberStyles.HexNumber, null);

            Assert.True(maybeShort.IsSome);
            var @short = maybeShort.Unwrap();
            Assert.Equal(shortString, $"{@short.ToString("x2")}");
        }

        [Fact]
        public void Additional_parameters_in_wrong_order_returns_none()
        {
            const string shortString = "cafe";
            var maybeShort = shortString.TryParse<short>(null, NumberStyles.HexNumber);

            Assert.True(maybeShort.IsNone);
        }

        [Fact]
        public void Try_parse_with_incorrect_format_returns_none()
        {
            const string shortString = "ssss";
            var maybeShort = shortString.TryParse<short>();

            Assert.True(maybeShort.IsNone);
        }

        [Fact]
        public void Parse_with_incorrect_format_returns_error()
        {
            const string shortString = "ssss";
            var maybeShort = shortString.Parse<short>();

            Assert.True(maybeShort.IsError);
        }

        [Fact]
        public void Force_throw_returns_none()
        {
            const string shortString = "cafe";
            // The first parameter is an enum, so this should throw. It will
            // pass the parameter checks because `null` aren't type-checked against
            // the target method.
            var maybeShort = shortString.TryParse<short>((NumberStyles)(-1), null);

            Assert.True(maybeShort.IsNone);
        }

        [Fact]
        public void Additional_parameters_in_wrong_order_returns_exception()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<short>(null, NumberStyles.HexNumber);

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Force_throw_returns_thrown_exception()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<short>((NumberStyles)(-1), null);

            Assert.True(parseResult.IsError);

            var exception = parseResult.UnwrapError();
            var argumentException = Assert.IsType<ArgumentException>(exception);

            Assert.Equal("style", argumentException.ParamName);
        }
    }
}
