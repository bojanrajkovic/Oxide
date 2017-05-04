using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xunit;

namespace Oxide.Tests
{
    public class HttpTests : IDisposable
    {
        const string ClientBase = "https://weathers.co/";
        HttpClient client;

        public HttpTests()
        {
            var handler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.None,
            };
            client = new HttpClient (handler) {
                BaseAddress = new Uri(ClientBase),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Oxide.Tests");
        }

        public void Dispose()
        {
            client.Dispose();
        }

        #region Error Testing
        [Fact]
        public async Task Errors_on_get_are_propagated()
        {
            const string badPath = "api2.php?city=Boston&f=1";
            var result = await client.SafelyGetAsync(badPath);

            Assert.True(result.IsError);
            var wex = Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_string_are_propagated()
        {
            const string badPath = "api2.php?city=Boston&f=1";
            var result = await client.SafelyGetStringAsync(badPath);

            Assert.True(result.IsError);
            var wex = Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_stream_are_propagated()
        {
            const string badPath = "api2.php?city=Boston&f=1";
            var result = await client.SafelyGetStreamAsync(badPath);

            Assert.True(result.IsError);
            var wex = Assert.IsType<HttpRequestException>(result.UnwrapError());
        }

        [Fact]
        public async Task Errors_on_get_byte_array_are_propagated()
        {
            const string badPath = "api2.php?city=Boston&f=1";
            var result = await client.SafelyGetByteArrayAsync(badPath);

            Assert.True(result.IsError);
            var wex = Assert.IsType<HttpRequestException>(result.UnwrapError());
        }
        #endregion

        #region Success Testing
        [Fact]
        public async Task Response_message_is_returned_on_success()
        {
            const string goodPath = "api.php?city=02131&f=1";
            var result = await client.SafelyGetAsync(goodPath);

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task String_data_is_returned_on_success()
        {
            const string goodPath = "api.php?city=02131&f=1";
            var result = await client.SafelyGetStringAsync(goodPath);

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            var data = JsonConvert.DeserializeObject<WeatherResponse>(response);

            // The actual content is weather data, no need to test its contents,
            // merely that we actually read it.
            Assert.NotNull(data);
            Assert.NotNull(data.Data);
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
            const string goodPath = "https://hpqec98e0j4e.runscope.net/";
            var result = await client.SafelyPostAsync(goodPath, new StringContent("Hello, world!"));

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Put_returns_http_response_on_success()
        {
            const string goodPath = "https://hpqec98e0j4e.runscope.net/";
            var result = await client.SafelyPutAsync(goodPath, new StringContent("Hello, world!"));

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_returns_http_response_on_success()
        {
            const string goodPath = "https://hpqec98e0j4e.runscope.net/";
            var result = await client.SafelyDeleteAsync(goodPath);

            Assert.True(result.IsOk);

            var response = result.Unwrap();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_returns_error_when_failed()
        {
            const string badPath = "http://httpstat.us/404";
            var result = await client.SafelyPostAsync(badPath, new StringContent("Hello, world!"));

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            var ex = Assert.IsType<HttpRequestException>(error);
        }

        [Fact]
        public async Task Put_returns_error_when_failed()
        {
            const string badPath = "https://httpstat.us/404";
            var result = await client.SafelyPutAsync(badPath, new StringContent("Hello, world!"));

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            var ex = Assert.IsType<HttpRequestException>(error);
        }

        [Fact]
        public async Task Delete_returns_error_when_failed()
        {
            const string badPath = "https://httpstat.us/404";
            var result = await client.SafelyDeleteAsync(badPath);

            Assert.True(result.IsError);

            var error = result.UnwrapError();
            Assert.NotNull(error);
            var ex = Assert.IsType<HttpRequestException>(error);
        }
        #endregion
    }

    class WeatherResponse
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("data")]
        public WeatherData Data { get; set; }
    }

    class WeatherData
    {
        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("temperature")]
        public int Temperature { get; set; }

        [JsonProperty("skytext")]
        public string Conditions { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("wind")]
        public string WindSpeed { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("day")]
        public string Day { get; set; }
    }
}
