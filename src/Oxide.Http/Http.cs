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
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
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
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
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
                var result = await client.PostAsync(requestUri, content).ConfigureAwait(false);
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
                var result = await client.PutAsync(requestUri, content).ConfigureAwait(false);
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
                var result = await client.DeleteAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }
    }
}
