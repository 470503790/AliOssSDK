using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Logging;
using AliOssSdk.Operations;
using AliOssSdk.Security;
using Xunit;

namespace AliOssSdk.Tests;

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

        using var cts = new CancellationTokenSource();
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

    private static OssClientConfiguration CreateConfiguration() =>
        new(new Uri("https://oss.example.com"), "key", "secret");

    private sealed class RecordingHttpClient : IOssHttpClient
    {
        private readonly OssHttpResponse _response;

        public RecordingHttpClient(OssHttpResponse response)
        {
            _response = response;
        }

        public List<OssHttpRequest> Requests { get; } = new();

        public List<CancellationToken> CancellationTokens { get; } = new();

        public int AsyncCallCount { get; private set; }

        public int SyncCallCount { get; private set; }

        public OssHttpResponse Send(OssHttpRequest request)
        {
            SyncCallCount++;
            throw new NotSupportedException("Synchronous send should not be used by OssClient.");
        }

        public Task<OssHttpResponse> SendAsync(OssHttpRequest request, CancellationToken cancellationToken = default)
        {
            AsyncCallCount++;
            Requests.Add(request);
            CancellationTokens.Add(cancellationToken);
            return Task.FromResult(_response);
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
        public List<OssLogEvent> Events { get; } = new();

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
