using System;
using System.IO;
using AliOssSdk.Configuration;
using Xunit;

namespace AliOssSdk.Tests.Configuration
{
    public class AlibabaOssConfigTests
    {
        [Fact]
        public void FromJson_DeserializesAllProperties()
        {
            var json = """
            {
                "region": "cn-hangzhou",
                "bucket": "demo",
                "endpoint": "https://oss-cn-hangzhou.aliyuncs.com",
                "accessKeyId": "key",
                "accessKeySecret": "secret",
                "sign_duration_second": 3600
            }
            """;

            var config = AlibabaOssConfig.FromJson(json);

            Assert.Equal("cn-hangzhou", config.Region);
            Assert.Equal("demo", config.Bucket);
            Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com", config.Endpoint);
            Assert.Equal("key", config.AccessKeyId);
            Assert.Equal("secret", config.AccessKeySecret);
            Assert.Equal(3600, config.SignDurationSeconds);
        }

        [Fact]
        public void ApplyEnvironmentOverrides_UsesPrefix()
        {
            const string prefix = "ALI_OSS_";
            Environment.SetEnvironmentVariable(prefix + "REGION", "cn-shanghai");
            Environment.SetEnvironmentVariable(prefix + "SIGN_DURATION_SECOND", "123");

            try
            {
                var config = new AlibabaOssConfig
                {
                    Region = "cn-hangzhou",
                    SignDurationSeconds = 321
                };

                config.ApplyEnvironmentOverrides();

                Assert.Equal("cn-shanghai", config.Region);
                Assert.Equal(123, config.SignDurationSeconds);
            }
            finally
            {
                Environment.SetEnvironmentVariable(prefix + "REGION", null);
                Environment.SetEnvironmentVariable(prefix + "SIGN_DURATION_SECOND", null);
            }
        }

        [Fact]
        public void ToOssClientConfiguration_UsesValues()
        {
            var config = new AlibabaOssConfig
            {
                Endpoint = "https://oss-cn-hangzhou.aliyuncs.com",
                AccessKeyId = "key",
                AccessKeySecret = "secret",
                Region = "cn-hangzhou"
            };

            var ossConfig = config.ToOssClientConfiguration();

            Assert.Equal(new Uri("https://oss-cn-hangzhou.aliyuncs.com"), ossConfig.Endpoint);
            Assert.Equal("key", ossConfig.AccessKeyId);
            Assert.Equal("secret", ossConfig.AccessKeySecret);
            Assert.Equal("cn-hangzhou", ossConfig.DefaultRegion);
        }

        [Fact]
        public void FromJsonFile_ReadsFromDisk()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, """
                {
                    "endpoint": "https://oss.example.com",
                    "accessKeyId": "id",
                    "accessKeySecret": "secret"
                }
                """);

                var config = AlibabaOssConfig.FromJsonFile(tempFile);
                Assert.Equal("https://oss.example.com", config.Endpoint);
                Assert.Equal("id", config.AccessKeyId);
                Assert.Equal("secret", config.AccessKeySecret);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
