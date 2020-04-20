using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xunit;

namespace Oxide.Tests
{
    public class HttpTests : IDisposable
    {
        readonly HttpClient client;

        public HttpTests()
        {
            client = new HttpClient { BaseAddress = new Uri("https://httpstat.us") };
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public void Dispose()
        {
            client.Dispose();
        }

        #region Error Testing
        [Fact]
        public async Task Errors_on_get_are_propagated()
        {
            var result = await client.SafelyGetAsync("/404");

            Assert.True(result.IsError);
            Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_string_are_propagated()
        {
            var result = await client.SafelyGetStringAsync("/404");

            Assert.True(result.IsError);
            Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_stream_are_propagated()
        {
            var result = await client.SafelyGetStreamAsync("/404");

            Assert.True(result.IsError);
            Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_byte_array_are_propagated()
        {
            var result = await client.SafelyGetByteArrayAsync("/404");

            Assert.True(result.IsError);
            Assert.IsType<HttpRequestException>(result.UnwrapError());
        }
        #endregion

        #region Success Testing

        [Fact]
        public async Task Response_message_is_returned_on_success()
        {
            var result = await client.SafelyGetAsync("/200");

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task String_data_is_returned_on_success()
        {
            var result = await client.SafelyGetStringAsync("/200");

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            var data = JsonConvert.DeserializeObject<StatusResponse>(response);

            Assert.NotNull(data);
            Assert.Equal(200, data.code);
            Assert.Equal("OK", data.description);
        }

        [Fact]
        public async Task Byte_array_data_is_returned_on_success()
        {
            const string goodPath = "https://oxidestorage.blob.core.windows.net/oxide-test-blobs/Zooey-8-Average.png";
            var result = await client.SafelyGetByteArrayAsync(goodPath);

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(1331288, response.Length);
        }

        [Fact]
        public async Task Stream_data_is_returned_on_success()
        {
            const string goodPath = "https://oxidestorage.blob.core.windows.net/oxide-test-blobs/Zooey-8-Average.png";
            var result = await client.SafelyGetStreamAsync(goodPath);

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(1331288, response.Length);
            Assert.Equal(0, response.Position);
            Assert.False(response.CanWrite);
        }
        #endregion

        #region Post/Put/Delete Tests
        [Fact]
        public async Task Post_returns_http_response_on_success()
        {
            var result = await client.SafelyPostAsync("/202", new StringContent("Hello, world!"));

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task Put_returns_http_response_on_success()
        {
            var result = await client.SafelyPutAsync("/202", new StringContent("Hello, world!"));

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task Delete_returns_http_response_on_success()
        {
            var result = await client.SafelyDeleteAsync("/200");

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_returns_error_when_failed()
        {
            var result = await client.SafelyPostAsync("/404", new StringContent("Hello, world!"));

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            Assert.IsType<HttpRequestException>(error);
        }

        [Fact]
        public async Task Put_returns_error_when_failed()
        {
            var result = await client.SafelyPutAsync("/404", new StringContent("Hello, world!"));

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            Assert.IsType<HttpRequestException>(error);
        }

        [Fact]
        public async Task Delete_returns_error_when_failed()
        {
            var result = await client.SafelyDeleteAsync("/404");

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            Assert.IsType<HttpRequestException>(error);
        }
        #endregion
    }

    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public class StatusResponse
    {
        public int code;
        public string description;
    }
}
