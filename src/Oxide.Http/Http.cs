using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oxide
{
    /// <summary>
    /// <see cref="HttpClient"/> extensions.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Safely gets a string value from the given URI.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <returns>A <see cref="Result{TResult,TError}"/> with the string value, or a caught exception.</returns>
        public static async Task<Result<string, Exception>> SafelyGetStringAsync(
            this HttpClient client,
            string requestUri
        ) {
            try {
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely gets a stream from the given URI.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <returns>A <see cref="Result{TResult,TError}"/> with the stream, or a caught exception.</returns>
        public static async Task<Result<Stream, Exception>> SafelyGetStreamAsync(
            this HttpClient client,
            string requestUri
        ) {
            try {
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely gets bytes from the given URI.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <returns>A <see cref="Result{TResult,TError}"/> with the bytes, or a caught exception.</returns>
        public static async Task<Result<byte[], Exception>> SafelyGetByteArrayAsync(
            this HttpClient client,
            string requestUri
        ) {
            try {
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely gets an <see cref="HttpResponseMessage"/> from the given URI.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>, or a caught exception.</returns>
        /// <remarks>It is up to the caller to process the HTTP response as desired.</remarks>
        public static async Task<Result<HttpResponseMessage, Exception>> SafelyGetAsync(
            this HttpClient client,
            string requestUri
        )  {
            try {
                var result = await client.GetAsync(requestUri).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely posts <paramref name="content"/> to <paramref name="requestUri"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <param name="content">The content to POST.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>, or a caught exception.</returns>
        /// <remarks>It is up to the caller to process the HTTP response as desired.</remarks>
        public static async Task<Result<HttpResponseMessage, Exception>> SafelyPostAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content
        )  {
            try {
                var result = await client.PostAsync(requestUri, content).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely puts <paramref name="content"/> to <paramref name="requestUri"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <param name="content">The content to POST.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>, or a caught exception.</returns>
        /// <remarks>It is up to the caller to process the HTTP response as desired.</remarks>
        public static async Task<Result<HttpResponseMessage, Exception>> SafelyPutAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content
        ) {
            try {
                var result = await client.PutAsync(requestUri, content).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return result;
            } catch (Exception e) {
                return e;
            }
        }

        /// <summary>
        /// Safely sends an HTTP DELETE to <paramref name="requestUri"/>.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="requestUri">The URI to send a GET request to.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>, or a caught exception.</returns>
        /// <remarks>It is up to the caller to process the HTTP response as desired.</remarks>
        public static async Task<Result<HttpResponseMessage, Exception>> SafelyDeleteAsync(
            this HttpClient client,
            string requestUri
        ) {
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
