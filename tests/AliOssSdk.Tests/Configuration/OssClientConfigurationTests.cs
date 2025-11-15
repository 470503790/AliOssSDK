using System;
using AliOssSdk.Configuration;
using Xunit;

namespace AliOssSdk.Tests.Configuration
{
    public class OssClientConfigurationTests
    {
        [Fact]
        public void Constructor_WithStringEndpoint_ParsesAbsoluteUri()
        {
            var config = new OssClientConfiguration("https://oss-cn-hangzhou.aliyuncs.com", "id", "secret");

            Assert.Equal(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), config.Endpoint);
        }

        [Fact]
        public void Constructor_ThrowsForRelativeEndpoint()
        {
            Assert.Throws<ArgumentException>(() => new OssClientConfiguration(new Uri("/bucket", UriKind.Relative), "id", "secret"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ThrowsForMissingAccessKeyId(string? value)
        {
            Assert.ThrowsAny<ArgumentException>(() => new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), value!, "secret"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ThrowsForMissingAccessKeySecret(string? value)
        {
            Assert.ThrowsAny<ArgumentException>(() => new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "id", value!));
        }

        [Fact]
        public void Clone_CopiesOptionalValuesAndCollections()
        {
            var original = new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "id", "secret", "demo")
            {
                DefaultRegion = "cn-hangzhou",
                SecurityToken = "token"
            };
            original.DefaultHeaders["x-oss-meta-test"] = "123";
            original.DefaultQueryParameters["acl"] = string.Empty;

            var clone = original.Clone();

            Assert.Equal(original.Endpoint, clone.Endpoint);
            Assert.Equal(original.DefaultRegion, clone.DefaultRegion);
            Assert.Equal(original.SecurityToken, clone.SecurityToken);
            Assert.Equal(original.DefaultBucketName, clone.DefaultBucketName);
            Assert.NotSame(original.DefaultHeaders, clone.DefaultHeaders);
            Assert.Equal(original.DefaultHeaders, clone.DefaultHeaders);
            Assert.NotSame(original.DefaultQueryParameters, clone.DefaultQueryParameters);
            Assert.Equal(original.DefaultQueryParameters, clone.DefaultQueryParameters);
        }

        [Fact]
        public void WithTimeout_ReturnsNewInstance()
        {
            var configuration = new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "id", "secret");

            var modified = configuration.WithTimeout(TimeSpan.FromSeconds(5));

            Assert.NotSame(configuration, modified);
            Assert.Equal(TimeSpan.FromSeconds(5), modified.Timeout);
            Assert.Equal(TimeSpan.FromSeconds(100), configuration.Timeout);
        }

        [Fact]
        public void Constructor_WithDefaultBucketName_SetsProperty()
        {
            var configuration = new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "id", "secret", "demo-bucket");

            Assert.Equal("demo-bucket", configuration.DefaultBucketName);
        }

        [Fact]
        public void Constructor_ThrowsForWhitespaceDefaultBucketName()
        {
            Assert.Throws<ArgumentException>(() => new OssClientConfiguration(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), "id", "secret", "   "));
        }
    }
}
