using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

using Xunit;

namespace Oxide.Tests
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public class ParseTests
    {
        class NonStaticMethods
        {
            public bool TryParse(string foo, out NonStaticMethods result)
            {
                result = new NonStaticMethods();
                return true;
            }

            public NonStaticMethods Parse(string foo)
                => new NonStaticMethods();
        }

        class MethodsNotReturningBool
        {
            public static int TryParse(string foo)
                => 5;

            public static int Parse(string foo)
                => 5;
        }

        class MethodsNotTakingString
        {
            public static bool TryParse(int foo)
                => true;

            public static bool Parse(int foo)
                => true;
        }

        class TryParseWithoutOutParameter
        {
            public static bool TryParse(string s, TryParseWithoutOutParameter fill)
                => true;
        }

        [Fact]
        public void Additional_parameters_in_wrong_order_returns_exception()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<short>(null, NumberStyles.HexNumber);

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching Parse method.", parseException.Message);
        }

        [Fact]
        public void Additional_parameters_in_wrong_order_returns_none()
        {
            const string shortString = "cafe";
            var maybeShort = shortString.TryParse<short>(null, NumberStyles.HexNumber);

            Assert.True(maybeShort.IsError);
            var exception = maybeShort.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Can_parse_IP_address_via_generic_try_parse()
        {
            const string addrString = "192.168.1.1";
            var maybeIp = addrString.TryParse<IPAddress>();

            Assert.True(maybeIp.IsOk);
            var addr = maybeIp.Unwrap().Unwrap();
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

            Assert.True(maybeShort.IsOk);
            var @short = maybeShort.Unwrap().Unwrap();
            Assert.Equal(shortString, $"{@short:x2}");
        }

        [Fact]
        public void Force_throw_returns_none()
        {
            const string shortString = "cafe";
            // The first parameter is an enum, so this should throw. It will
            // pass the parameter checks because `null` aren't type-checked against
            // the target method.
            var maybeShort = shortString.TryParse<short>((NumberStyles)(-1), null);

            Assert.True(maybeShort.IsError);
            var exception = maybeShort.UnwrapError();
            var argumentException = Assert.IsType<ArgumentException>(exception);
            Assert.Equal("style", argumentException.ParamName);
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

        [Fact]
        public void Parse_from_declared_method_returns_error_with_invalid_source_string()
        {
            const string shortString = "ssss";
            var maybeShort = shortString.Parse<short>();

            Assert.True(maybeShort.IsError);
            var ex = Assert.IsType<FormatException>(maybeShort.UnwrapError());
            Assert.Equal(-2146233033, ex.HResult);
        }

        [Fact]
        public void Parse_from_declared_method_returns_result_with_valid_source_string()
        {
            const string shortString = "32000";
            var maybeShort = shortString.Parse<short>();

            Assert.True(maybeShort.IsOk);
            Assert.Equal(32000, maybeShort.Unwrap());
        }

        [Fact]
        public void Parse_from_extension_method_returns_exception_with_invalid_source_string()
        {
            const string uriString = "./foo/5/bar";

            var uri = uriString.Parse<Uri>(UriKind.Absolute);
            Assert.True(uri.IsError);
            var urifex = Assert.IsType<UriFormatException>(uri.UnwrapError());
            Assert.Equal(-2146233033, urifex.HResult);
        }

        [Fact]
        public void Parse_from_extension_method_returns_result_with_valid_source_string()
        {
            const string uriString = "https://google.com/";

            var uri = uriString.Parse<Uri>(UriKind.Absolute);
            Assert.True(uri.IsOk);
            Assert.Equal(uriString, uri.Unwrap().ToString());
        }

        [Fact]
        public void Parse_type_with_non_bool_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<MethodsNotReturningBool>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching Parse method.", parseException.Message);
        }

        [Fact]
        public void Parse_type_with_non_static_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<NonStaticMethods>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching Parse method.", parseException.Message);
        }

        [Fact]
        public void Parse_type_with_non_string_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.Parse<MethodsNotTakingString>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching Parse method.", parseException.Message);
        }

        [Fact]
        public void Parse_with_incorrect_format_returns_error()
        {
            const string shortString = "ssss";
            var maybeShort = shortString.Parse<short>();

            Assert.True(maybeShort.IsError);
        }

        [Fact]
        public void Try_parse_from_declared_method_returns_none_with_invalid_source_string()
        {
            const string shortString = "ssss";
            var maybeShort = shortString.TryParse<short>();

            Assert.True(maybeShort.IsOk);
            Assert.True(maybeShort.Unwrap().IsNone);
        }

        [Fact]
        public void Try_parse_from_declared_method_returns_result_with_valid_source_string()
        {
            const string shortString = "32000";
            var maybeShort = shortString.TryParse<short>();

            Assert.True(maybeShort.IsOk);
            Assert.Equal(32000, maybeShort.Unwrap().Unwrap());
        }

        [Fact]
        public void Try_parse_from_extension_method_returns_none_with_invalid_source_string()
        {
            const string uriString = "./foo/5/bar";

            var res = uriString.TryParse<Uri>(UriKind.Absolute);

            Assert.True(res.IsOk);
            var uriOption = res.Unwrap();
            Assert.True(uriOption.IsNone);
        }

        [Fact]
        public void Try_parse_from_extension_method_returns_result_with_valid_source_string()
        {
            const string uriString = "https://google.com/";

            var res = uriString.TryParse<Uri>(UriKind.Absolute);

            Assert.True(res.IsOk);
            var uriOption = res.Unwrap();
            Assert.True(uriOption.IsSome);
            Assert.Equal(uriString, uriOption.Unwrap().ToString());
        }

        [Fact]
        public void Try_parse_type_with_non_bool_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<MethodsNotReturningBool>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_out_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<TryParseWithoutOutParameter>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_static_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<NonStaticMethods>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }

        [Fact]
        public void Try_parse_type_with_non_string_try_parse_returns_method_not_found()
        {
            const string shortString = "cafe";
            var parseResult = shortString.TryParse<MethodsNotTakingString>();

            Assert.True(parseResult.IsError);
            var exception = parseResult.UnwrapError();
            var parseException = Assert.IsType<ParseException>(exception);
            Assert.Equal("Could not find matching TryParse method.", parseException.Message);
        }
    }

    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ExtensionTryParse
    {
        public static bool TryParse(this string uriString, UriKind kind, out Uri uri)
            => Uri.TryCreate(uriString, kind, out uri);

        public static Uri Parse(this string uriString, UriKind kind)
            => new Uri(uriString, kind);
    }

    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class ExtensionTryParseWithWrongReturnType
    {
        public static string Parse(this string uriString, UriKind kind)
            => uriString;
    }
}
