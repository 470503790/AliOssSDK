using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using Xunit;

namespace AliOssSdk.Tests.Http;

public class OssHttpClientTests
{
    [Fact]
    public async Task SendAsync_WhenStatusNotSuccessful_ThrowsOssRequestExceptionWithDetails()
    {
        var configuration = new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "key", "secret");
        var handler = new StubHttpMessageHandler();
        handler.ResponseFactory = () =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("<Error>Invalid bucket</Error>", Encoding.UTF8)
            };
            response.Headers.Add("x-oss-request-id", "ABC123");
            return response;
        };

        using var client = new OssHttpClient(configuration, handler);
        var request = new OssHttpRequest(HttpMethod.Get, "/demo");

        var exception = await Assert.ThrowsAsync<OssRequestException>(() => client.SendAsync(request)).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("ABC123", exception.RequestId);
        Assert.Equal("<Error>Invalid bucket</Error>", exception.ResponseBody);
        Assert.NotNull(exception.Response);
        Assert.True(exception.ResponseHeaders.ContainsKey("x-oss-request-id"));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpResponseMessage>? ResponseFactory { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ResponseFactory == null)
            {
                throw new InvalidOperationException("ResponseFactory must be provided.");
            }

            return Task.FromResult(ResponseFactory());
        }
    }
}
