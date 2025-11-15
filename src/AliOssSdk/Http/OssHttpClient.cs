using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using AliOssSdk.Configuration;

namespace AliOssSdk.Http
{
    public sealed class OssHttpClient : IOssHttpClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        public OssHttpClient(OssClientConfiguration configuration, HttpMessageHandler? handler = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = handler == null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
            _httpClient.BaseAddress = configuration.Endpoint;
            _httpClient.Timeout = configuration.Timeout;
        }

        public OssClientConfiguration Configuration { get; }

        public OssHttpResponse Send(OssHttpRequest request) => SendAsync(request).GetAwaiter().GetResult();

        public async Task<OssHttpResponse> SendAsync(OssHttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (var message = new HttpRequestMessage(request.Method, BuildRelativeUri(request)))
            {
                if (request.Content != null)
                {
                    message.Content = new StreamContent(request.Content);
                    if (!string.IsNullOrWhiteSpace(request.ContentType))
                    {
                        message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
                    }
                }

                foreach (var header in request.Headers)
                {
                    if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    {
                        message.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                var stream = new MemoryStream();
                await response.Content.CopyToAsync(stream).ConfigureAwait(false);
                stream.Position = 0;

                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var header in response.Headers)
                {
                    headers[header.Key] = string.Join(",", header.Value);
                }

                foreach (var header in response.Content.Headers)
                {
                    headers[header.Key] = string.Join(",", header.Value);
                }

                var isSuccess = response.IsSuccessStatusCode;
                var ossResponse = new OssHttpResponse(response.StatusCode, stream, headers)
                {
                    RequestId = headers.TryGetValue("x-oss-request-id", out var requestId) ? requestId : null
                };
                response.Dispose();
                if (!isSuccess)
                {
                    string? responseBody = null;
                    if (stream.Length > 0)
                    {
                        stream.Position = 0;
                        using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                        {
                            responseBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                            stream.Position = 0;
                        }
                    }

                    throw new OssRequestException(ossResponse, responseBody);
                }

                return ossResponse;
            }
        }

        private static string BuildRelativeUri(OssHttpRequest request)
        {
            if (request.QueryParameters.Count == 0)
            {
                return request.ResourcePath;
            }

            var query = string.Join("&", request.QueryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            return $"{request.ResourcePath}?{query}";
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
