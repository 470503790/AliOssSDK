using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Logging;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations;
using AliOssSdk.Security;
using Xunit;

namespace AliOssSdk.Tests
{

    public class OssClientTests
    {
        [Fact]
        public async Task ExecuteAsync_UsesHttpClientSignerAndLogger()
        {
            var configuration = CreateConfiguration();
            var request = new OssHttpRequest(HttpMethod.Post, "/bucket/object");
            request.QueryParameters["version"] = "1";
            request.Headers["x-oss-meta-test"] = "value";
            request.Content = new MemoryStream(new byte[] { 1, 2, 3 });
            request.ContentType = "application/json";

            var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETag"] = "\"abc\""
            };
            var httpResponse = new OssHttpResponse(HttpStatusCode.Accepted, new MemoryStream(), responseHeaders)
            {
                RequestId = "request-id"
            };

            var httpClient = new RecordingHttpClient(httpResponse);
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var operation = new StubOperation<string>("TestOperation", request, _ => "parsed-response");
            var client = new OssClient(configuration, httpClient, signer, logger);

            using (var cts = new CancellationTokenSource())
            {
                var result = await client.ExecuteAsync(operation, cts.Token).ConfigureAwait(false);

                Assert.Equal("parsed-response", result);
                Assert.Equal(1, operation.BuildCount);
                Assert.Equal(1, operation.ParseCount);
                Assert.Same(request, signer.LastRequest);
                Assert.Same(configuration, signer.LastConfiguration);
                Assert.Single(httpClient.Requests);
                Assert.Same(request, httpClient.Requests.Single());
                Assert.Equal(cts.Token, httpClient.CancellationTokens.Single());
                Assert.Equal(new[]
                {
                    OssLogEventType.Retry,
                    OssLogEventType.RequestStart,
                    OssLogEventType.RequestHeaders,
                    OssLogEventType.RequestBody,
                    OssLogEventType.Response
                }, logger.Events.Select(e => e.EventType));
            }
        }

        [Fact]
        public async Task ExecuteAsync_WhenHttpClientThrowsOssRequestException_PropagatesDetails()
        {
            var configuration = CreateConfiguration();
            var request = new OssHttpRequest(HttpMethod.Get, "/bucket/object");
            var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["x-oss-request-id"] = "async-request"
            };
            var responseBody = "<Error>Denied</Error>";
            var ossResponse = new OssHttpResponse(HttpStatusCode.Forbidden, new MemoryStream(Encoding.UTF8.GetBytes(responseBody)), responseHeaders)
            {
                RequestId = "async-request"
            };
            var httpClient = new RecordingHttpClient(new OssRequestException(ossResponse, responseBody));
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var operation = new StubOperation<string>("ErrorOperation", request, _ => "never");
            var client = new OssClient(configuration, httpClient, signer, logger);

            var exception = await Assert.ThrowsAsync<OssRequestException>(() => client.ExecuteAsync(operation)).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
            Assert.Equal("async-request", exception.RequestId);
            Assert.Equal(responseBody, exception.ResponseBody);
        }

        [Fact]
        public async Task Constructor_UsesConfigurationOverrides()
        {
            var request = new OssHttpRequest(HttpMethod.Post, "/bucket/object");
            request.Headers["x-oss-test"] = "value";

            var httpResponse = new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(), new Dictionary<string, string>());
            var httpClient = new RecordingHttpClient(httpResponse);
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var configuration = new OssClientConfiguration(new Uri("https://oss.example.com"), "key", "secret")
            {
                HttpClient = httpClient,
                RequestSigner = signer,
                Logger = logger
            };

            var operation = new StubOperation<string>("ConfiguredOperation", request, _ => "configured-response");
            var client = new OssClient(configuration);

            var result = await client.ExecuteAsync(operation).ConfigureAwait(false);

            Assert.Equal("configured-response", result);
            Assert.Same(request, signer.LastRequest);
            Assert.Same(configuration, signer.LastConfiguration);
            Assert.Single(httpClient.Requests, request);
            Assert.Equal(new[]
            {
                OssLogEventType.Retry,
                OssLogEventType.RequestStart,
                OssLogEventType.RequestHeaders,
                OssLogEventType.RequestBody,
                OssLogEventType.Response
            }, logger.Events.Select(e => e.EventType));
        }

        [Fact]
        public void Execute_WhenHttpClientThrowsOssRequestException_PropagatesDetails()
        {
            var configuration = CreateConfiguration();
            var request = new OssHttpRequest(HttpMethod.Get, "/resource");
            var responseBody = "<Error>SyncDenied</Error>";
            var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["x-oss-request-id"] = "sync-request"
            };
            var ossResponse = new OssHttpResponse(HttpStatusCode.BadRequest, new MemoryStream(Encoding.UTF8.GetBytes(responseBody)), responseHeaders)
            {
                RequestId = "sync-request"
            };
            var httpClient = new RecordingHttpClient(new OssRequestException(ossResponse, responseBody));
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var operation = new StubOperation<int>("SyncError", request, _ => 0);
            var client = new OssClient(configuration, httpClient, signer, logger);

            var exception = Assert.Throws<OssRequestException>(() => client.Execute(operation));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("sync-request", exception.RequestId);
            Assert.Equal(responseBody, exception.ResponseBody);
        }

        [Fact]
        public void Execute_UsesAsyncPipeline()
        {
            var configuration = CreateConfiguration();
            var request = new OssHttpRequest(HttpMethod.Get, "/resource")
            {
                Content = new MemoryStream(),
                ContentType = "text/plain"
            };
            request.Headers["Custom"] = "value";

            var httpResponse = new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(), new Dictionary<string, string>());
            var httpClient = new RecordingHttpClient(httpResponse);
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var operation = new StubOperation<int>("SyncOperation", request, _ => 42);
            var client = new OssClient(configuration, httpClient, signer, logger);

            var result = client.Execute(operation);

            Assert.Equal(42, result);
            Assert.Equal(1, httpClient.AsyncCallCount);
            Assert.Equal(0, httpClient.SyncCallCount);
            Assert.Equal(new[]
            {
                OssLogEventType.Retry,
                OssLogEventType.RequestStart,
                OssLogEventType.RequestHeaders,
                OssLogEventType.RequestBody,
                OssLogEventType.Response
            }, logger.Events.Select(e => e.EventType));
        }

        [Fact]
        public void PutObjectFromFile_DelegatesToPutOperation()
        {
            var configuration = CreateConfiguration();
            var httpResponse = new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(Encoding.UTF8.GetBytes("")), new Dictionary<string, string>());
            var httpClient = new RecordingHttpClient(httpResponse);
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var client = new OssClient(configuration, httpClient, signer, logger);

            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello");

            try
            {
                var result = client.PutObjectFromFile("my-bucket", "images/logo.png", path, "text/plain");

                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var request = Assert.Single(httpClient.Requests);
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.Equal("/my-bucket/images/logo.png", request.ResourcePath);
                Assert.Equal("text/plain", request.ContentType);
                Assert.NotNull(request.Content);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task PutObjectFromFileAsync_UsesDefaultContentType()
        {
            var configuration = CreateConfiguration();
            var httpResponse = new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(Encoding.UTF8.GetBytes("")), new Dictionary<string, string>());
            var httpClient = new RecordingHttpClient(httpResponse);
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var client = new OssClient(configuration, httpClient, signer, logger);

            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello async");

            try
            {
                var response = await client.PutObjectFromFileAsync(null, "images/logo.png", path).ConfigureAwait(false);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var request = Assert.Single(httpClient.Requests);
                Assert.Equal("application/octet-stream", request.ContentType);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void MoveObjects_ChainsCopyAndDelete()
        {
            var configuration = CreateConfiguration();
            var httpClient = new RecordingHttpClient(() =>
                new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(Encoding.UTF8.GetBytes("<CopyObjectResult />")), new Dictionary<string, string>()));
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var client = new OssClient(configuration, httpClient, signer, logger);

            var descriptors = new[]
            {
                new ObjectMoveDescriptor("source", "from.txt", "dest", "to.txt"),
                new ObjectMoveDescriptor("my-bucket", "docs/report.pdf", "my-bucket", "reports/2024/report.pdf")
            };

            client.MoveObjects(descriptors);

            Assert.Equal(4, httpClient.Requests.Count);
            Assert.Equal(HttpMethod.Put, httpClient.Requests[0].Method);
            Assert.Equal("/dest/to.txt", httpClient.Requests[0].ResourcePath);
            Assert.Equal(HttpMethod.Delete, httpClient.Requests[1].Method);
            Assert.Equal("/source/from.txt", httpClient.Requests[1].ResourcePath);
            Assert.Equal(HttpMethod.Put, httpClient.Requests[2].Method);
            Assert.Equal("/my-bucket/reports/2024/report.pdf", httpClient.Requests[2].ResourcePath);
            Assert.Equal(HttpMethod.Delete, httpClient.Requests[3].Method);
            Assert.Equal("/my-bucket/docs/report.pdf", httpClient.Requests[3].ResourcePath);
        }

        [Fact]
        public async Task MoveObjectsAsync_HonorsDescriptorsSequentially()
        {
            var configuration = CreateConfiguration();
            var httpClient = new RecordingHttpClient(() =>
                new OssHttpResponse(HttpStatusCode.OK, new MemoryStream(Encoding.UTF8.GetBytes("<CopyObjectResult />")), new Dictionary<string, string>()));
            var signer = new RecordingSigner();
            var logger = new TestLogger();
            var client = new OssClient(configuration, httpClient, signer, logger);

            var descriptors = new[]
            {
                new ObjectMoveDescriptor("source", "from.txt", "dest", "to.txt"),
                new ObjectMoveDescriptor("source", "second.txt", "dest", "second.txt")
            };

            await client.MoveObjectsAsync(descriptors).ConfigureAwait(false);

            Assert.Equal(4, httpClient.Requests.Count);
            Assert.Equal(HttpMethod.Put, httpClient.Requests[0].Method);
            Assert.Equal(HttpMethod.Delete, httpClient.Requests[1].Method);
            Assert.Equal(HttpMethod.Put, httpClient.Requests[2].Method);
            Assert.Equal(HttpMethod.Delete, httpClient.Requests[3].Method);
        }

        private static OssClientConfiguration CreateConfiguration()
        {
            return new OssClientConfiguration(new Uri("https://oss.example.com"), "key", "secret");
        }

        private sealed class RecordingHttpClient : IOssHttpClient
        {
            private readonly OssHttpResponse? _response;
            private readonly Func<OssHttpResponse>? _responseFactory;
            private readonly Exception? _exception;

            public RecordingHttpClient(OssHttpResponse response)
            {
                _response = response;
            }

            public RecordingHttpClient(Func<OssHttpResponse> responseFactory)
            {
                _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
            }

            public RecordingHttpClient(Exception exception)
            {
                _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            }

            public List<OssHttpRequest> Requests { get; } = new List<OssHttpRequest>();

            public List<CancellationToken> CancellationTokens { get; } = new List<CancellationToken>();

            public int AsyncCallCount { get; private set; }

            public int SyncCallCount { get; private set; }

            public OssHttpResponse Send(OssHttpRequest request)
            {
                SyncCallCount++;
                throw new NotSupportedException("Synchronous send should not be used by OssClient.");
            }

            public Task<OssHttpResponse> SendAsync(OssHttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                AsyncCallCount++;
                Requests.Add(request);
                CancellationTokens.Add(cancellationToken);
                if (_exception != null)
                {
                    return Task.FromException<OssHttpResponse>(_exception);
                }

                var response = _responseFactory?.Invoke() ?? _response;
                if (response == null)
                {
                    throw new InvalidOperationException("RecordingHttpClient requires either a response or exception.");
                }

                return Task.FromResult(response);
            }
        }

        private sealed class RecordingSigner : IOssRequestSigner
        {
            public OssHttpRequest? LastRequest { get; private set; }

            public OssClientConfiguration? LastConfiguration { get; private set; }

            public void Sign(OssHttpRequest request, OssClientConfiguration configuration)
            {
                LastRequest = request;
                LastConfiguration = configuration;
            }
        }

        private sealed class TestLogger : ILogger
        {
            public List<OssLogEvent> Events { get; } = new List<OssLogEvent>();

            public void Log(OssLogEvent logEvent)
            {
                Events.Add(logEvent);
            }
        }

        private sealed class StubOperation<T> : IOssOperation<T>
        {
            private readonly OssHttpRequest _request;
            private readonly Func<OssHttpResponse, T> _responseFactory;

            public StubOperation(string name, OssHttpRequest request, Func<OssHttpResponse, T> responseFactory)
            {
                Name = name;
                _request = request;
                _responseFactory = responseFactory;
            }

            public string Name { get; }

            public int BuildCount { get; private set; }

            public int ParseCount { get; private set; }

            public OssHttpRequest BuildRequest(OssOperationContext context)
            {
                BuildCount++;
                return _request;
            }

            public T ParseResponse(OssHttpResponse response)
            {
                ParseCount++;
                return _responseFactory(response);
            }
        }
    }
}
