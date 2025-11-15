using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Operations;
using AliOssSdk.Security;
using AliOssSdk.Logging;

namespace AliOssSdk
{
    public sealed class OssClient : IOssClient, IDisposable
    {
        private readonly IOssHttpClient _httpClient;
        private readonly IOssRequestSigner _requestSigner;
        private readonly ILogger _logger;
        private readonly bool _ownsHttpClient;
        private readonly OssOperationContext _context;

        public OssClient(OssClientConfiguration configuration)
            : this(configuration, new OssHttpClient(configuration), new HmacSha1RequestSigner(), NullLogger.Instance, ownsHttpClient: true)
        {
        }

        public OssClient(OssClientConfiguration configuration, IOssHttpClient httpClient, IOssRequestSigner requestSigner, bool ownsHttpClient = false)
            : this(configuration, httpClient, requestSigner, NullLogger.Instance, ownsHttpClient)
        {
        }

        public OssClient(OssClientConfiguration configuration, IOssHttpClient httpClient, IOssRequestSigner requestSigner, ILogger logger, bool ownsHttpClient = false)
        {
            _context = new OssOperationContext(configuration ?? throw new ArgumentNullException(nameof(configuration)));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestSigner = requestSigner ?? throw new ArgumentNullException(nameof(requestSigner));
            _logger = logger ?? NullLogger.Instance;
            _ownsHttpClient = ownsHttpClient;
        }

        public TResponse Execute<TResponse>(IOssOperation<TResponse> operation) => ExecuteAsync(operation).GetAwaiter().GetResult();

        public async Task<TResponse> ExecuteAsync<TResponse>(IOssOperation<TResponse> operation, CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var invocationId = Guid.NewGuid();
            var attempt = 1;
            var operationName = operation.GetType().Name;
            OssHttpRequest? request = null;
            var start = DateTimeOffset.UtcNow;

            LogRetry(operationName, invocationId, attempt, isRetry: attempt > 1, exception: null);

            try
            {
                request = operation.BuildRequest(_context);
                _requestSigner.Sign(request, _context.Configuration);

                LogRequestStart(operationName, invocationId, attempt, request);
                LogRequestHeaders(operationName, invocationId, attempt, request);
                LogRequestBody(operationName, invocationId, attempt, request);

                var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                LogResponse(operationName, invocationId, attempt, response, DateTimeOffset.UtcNow - start);
                return operation.ParseResponse(response);
            }
            catch (Exception ex)
            {
                LogError(operationName, invocationId, attempt, request, ex);
                throw;
            }
        }

        public void Dispose()
        {
            if (_ownsHttpClient && _httpClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void LogRequestStart(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var data = new Dictionary<string, object?>
            {
                ["Method"] = request.Method.Method,
                ["ResourcePath"] = request.ResourcePath,
                ["Query"] = request.QueryParameters.Count == 0 ? null : string.Join("&", request.QueryParameters.Select(p => $"{p.Key}={p.Value}"))
            };

            _logger.Log(OssLogEvent.RequestStart(operationName, invocationId, attempt, data));
        }

        private void LogRequestHeaders(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = header.Value;
            }

            if (headers.Count > 0)
            {
                _logger.Log(OssLogEvent.RequestHeaders(operationName, invocationId, attempt, headers));
            }
        }

        private void LogRequestBody(string operationName, Guid invocationId, int attempt, OssHttpRequest request)
        {
            var payload = new Dictionary<string, object?>
            {
                ["HasContent"] = request.Content != null,
                ["ContentType"] = request.ContentType,
                ["ContentLength"] = TryGetContentLength(request.Content)
            };

            _logger.Log(OssLogEvent.RequestBody(operationName, invocationId, attempt, payload));
        }

        private void LogRetry(string operationName, Guid invocationId, int attempt, bool isRetry, Exception? exception)
        {
            _logger.Log(OssLogEvent.Retry(operationName, invocationId, attempt, isRetry, exception));
        }

        private void LogResponse(string operationName, Guid invocationId, int attempt, OssHttpResponse response, TimeSpan duration)
        {
            var data = new Dictionary<string, object?>
            {
                ["StatusCode"] = (int)response.StatusCode,
                ["DurationMs"] = duration.TotalMilliseconds,
                ["RequestId"] = response.RequestId
            };

            foreach (var header in response.Headers)
            {
                data[$"Header:{header.Key}"] = header.Value;
            }

            _logger.Log(OssLogEvent.Response(operationName, invocationId, attempt, data));
        }

        private void LogError(string operationName, Guid invocationId, int attempt, OssHttpRequest? request, Exception exception)
        {
            var data = request == null
                ? null
                : new Dictionary<string, object?>
                {
                    ["Method"] = request.Method.Method,
                    ["ResourcePath"] = request.ResourcePath
                };

            _logger.Log(OssLogEvent.Error(operationName, invocationId, attempt, data, exception));
        }

        private static long? TryGetContentLength(Stream? stream)
        {
            if (stream == null)
            {
                return null;
            }

            if (!stream.CanSeek)
            {
                return null;
            }

            try
            {
                return stream.Length;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
