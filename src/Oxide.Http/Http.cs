using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oxide
{
    public static class Http
    {
        public static async Task<Result<string, Exception>> SafelyGetStringAsync(
            this HttpClient client,
            string requestUri
        )
        {
            try {
                var result = await client.GetAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<Stream, Exception>> SafelyGetStreamAsync(
            this HttpClient client,
            string requestUri
        )
        {
            try {
                var result = await client.GetAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStreamAsync();
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<byte[], Exception>> SafelyGetByteArrayAsync(
            this HttpClient client,
            string requestUri
        )
        {
            try {
                var result = await client.GetAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsByteArrayAsync();
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<HttpResponseMessage, Exception>> SafelyGetAsync(
            this HttpClient client,
            string requestUri
        )
        {
            try {
                var result = await client.GetAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<HttpResponseMessage, Exception>> SafelyPostAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content
        )
        {
            try {
                var result = await client.PostAsync(requestUri, content);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<HttpResponseMessage, Exception>> SafelyPutAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content
        )
        {
            try {
                var result = await client.PutAsync(requestUri, content);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        public static async Task<Result<HttpResponseMessage, Exception>> SafelyDeleteAsync(
            this HttpClient client,
            string requestUri
        )
        {
            try {
                var result = await client.DeleteAsync(requestUri);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }
    }
}
