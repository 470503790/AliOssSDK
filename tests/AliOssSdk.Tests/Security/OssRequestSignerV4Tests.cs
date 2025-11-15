using System;
using System.Net.Http;
using AliOssSdk.Configuration;
using AliOssSdk.Http;
using AliOssSdk.Security;
using Xunit;

namespace AliOssSdk.Tests.Security
{
    public class OssRequestSignerV4Tests
    {
        private static readonly DateTimeOffset FixedTime = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero);

        [Fact]
        public void Sign_ProducesDeterministicAuthorizationHeader()
        {
            var request = new OssHttpRequest(HttpMethod.Get, "/bucket/object");
            request.QueryParameters["acl"] = string.Empty;
            request.Headers["Content-Type"] = "text/plain";

            var configuration = new OssClientConfiguration(
                new Uri("https://oss-cn-hangzhou.aliyuncs.com"),
                "testAccessKey",
                "testSecretKey");

            var signer = new OssRequestSignerV4(() => FixedTime);
            signer.Sign(request, configuration);

            Assert.Equal("oss-cn-hangzhou.aliyuncs.com", request.Headers["Host"]);
            Assert.Equal("UNSIGNED-PAYLOAD", request.Headers["x-oss-content-sha256"]);
            Assert.Equal("20240102T030405Z", request.Headers["x-oss-date"]);

            var expected = "OSS4-HMAC-SHA256 Credential=testAccessKey/20240102/cn-hangzhou/oss/aliyun_v4_request,SignedHeaders=content-type;host;x-oss-content-sha256;x-oss-date,Signature=bde4fbf59bbbe78b7204a89904e80a899ac3385c9a172624f7af9eca4d676197";
            Assert.Equal(expected, request.Headers["Authorization"]);
        }

        [Fact]
        public void Sign_ThrowsWhenRegionCannotBeResolved()
        {
            var request = new OssHttpRequest(HttpMethod.Get, "/");
            var configuration = new OssClientConfiguration(new Uri("https://static.example.com"), "id", "secret");
            var signer = new OssRequestSignerV4(() => FixedTime);

            Assert.Throws<InvalidOperationException>(() => signer.Sign(request, configuration));
        }

        [Fact]
        public void Sign_AllowsExplicitRegionWithOssPrefix()
        {
            var request = new OssHttpRequest(HttpMethod.Get, "/demo");
            var configuration = new OssClientConfiguration(new Uri("https://files.example.com"), "id", "secret")
            {
                DefaultRegion = "oss-cn-qingdao"
            };

            var signer = new OssRequestSignerV4(() => FixedTime);
            signer.Sign(request, configuration);

            var authorization = request.Headers["Authorization"];
            Assert.Contains("/cn-qingdao/", authorization);
        }
    }
}
