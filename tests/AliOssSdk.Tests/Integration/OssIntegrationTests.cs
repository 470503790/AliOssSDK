using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AliOssSdk.Configuration;
using AliOssSdk.Models.Objects;
using Xunit;

namespace AliOssSdk.Tests.Integration
{
    /// <summary>
    /// Integration tests that require real OSS credentials and bucket.
    /// These tests will be skipped if OSS credentials are not configured.
    /// 
    /// To run these tests, set the following environment variables:
    /// - OSS_ENDPOINT: e.g., https://oss-cn-hangzhou.aliyuncs.com
    /// - OSS_ACCESS_KEY_ID: Your Alibaba Cloud Access Key ID
    /// - OSS_ACCESS_KEY_SECRET: Your Alibaba Cloud Access Key Secret
    /// - OSS_BUCKET_NAME: The bucket name to use for testing
    /// - OSS_REGION: e.g., cn-hangzhou (optional if endpoint includes region)
    /// </summary>
    public class OssIntegrationTests : IDisposable
    {
        private readonly OssClient? _client;
        private readonly string? _bucketName;
        private readonly bool _skipTests;

        public OssIntegrationTests()
        {
            var endpoint = Environment.GetEnvironmentVariable("OSS_ENDPOINT");
            var accessKeyId = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_ID");
            var accessKeySecret = Environment.GetEnvironmentVariable("OSS_ACCESS_KEY_SECRET");
            _bucketName = Environment.GetEnvironmentVariable("OSS_BUCKET_NAME");
            var region = Environment.GetEnvironmentVariable("OSS_REGION");

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKeyId) || 
                string.IsNullOrEmpty(accessKeySecret) || string.IsNullOrEmpty(_bucketName))
            {
                _skipTests = true;
                return;
            }

            var configuration = new OssClientConfiguration(new Uri(endpoint), accessKeyId, accessKeySecret)
            {
                DefaultBucketName = _bucketName
            };

            if (!string.IsNullOrEmpty(region))
            {
                configuration.DefaultRegion = region;
            }

            _client = new OssClient(configuration);
            _skipTests = false;
        }

        /// <summary>
        /// Integration test: Upload a file to OSS using PutObject.
        /// This test creates a temporary file, uploads it to OSS, and verifies the upload was successful.
        /// </summary>
        [Fact]
        public void PutObject_UploadsFileToOss_Successfully()
        {
            if (_skipTests)
            {
                // Skip test if credentials are not configured
                return;
            }

            // Arrange
            var testFileName = $"test-upload-{Guid.NewGuid()}.txt";
            var testContent = $"Test file content uploaded at {DateTime.UtcNow:O}";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFilePath, testContent);

                // Act
                var response = _client!.PutObjectFromFile(_bucketName, testFileName, tempFilePath, "text/plain");

                // Assert
                Assert.NotNull(response);
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created);
                Assert.NotNull(response.ETag);
                Assert.NotEmpty(response.ETag);
            }
            finally
            {
                // Cleanup: Delete temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                // Cleanup: Delete uploaded file from OSS
                try
                {
                    _client?.DeleteObject(new DeleteObjectRequest(_bucketName, testFileName));
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Integration test: Download a file from OSS using GetObject by key.
        /// This test uploads a file, downloads it by key, and verifies the content matches.
        /// </summary>
        [Fact]
        public void GetObject_DownloadsFileFromOssByKey_Successfully()
        {
            if (_skipTests)
            {
                // Skip test if credentials are not configured
                return;
            }

            // Arrange
            var testFileName = $"test-download-{Guid.NewGuid()}.txt";
            var testContent = $"Test file content for download at {DateTime.UtcNow:O}";

            try
            {
                // First, upload a test file
                using (var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent)))
                {
                    var putRequest = new PutObjectRequest(_bucketName, testFileName, uploadStream)
                    {
                        ContentType = "text/plain"
                    };
                    var putResponse = _client!.PutObject(putRequest);
                    Assert.NotNull(putResponse);
                }

                // Act: Download the file by key
                var getRequest = new GetObjectRequest(_bucketName, testFileName);
                var getResponse = _client!.GetObject(getRequest);

                // Assert
                Assert.NotNull(getResponse);
                Assert.True(getResponse.StatusCode == System.Net.HttpStatusCode.OK || getResponse.StatusCode == System.Net.HttpStatusCode.PartialContent);
                Assert.NotNull(getResponse.Content);

                using (var reader = new StreamReader(getResponse.Content, Encoding.UTF8))
                {
                    var downloadedContent = reader.ReadToEnd();
                    Assert.Equal(testContent, downloadedContent);
                }
            }
            finally
            {
                // Cleanup: Delete uploaded file from OSS
                try
                {
                    _client?.DeleteObject(new DeleteObjectRequest(_bucketName, testFileName));
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Integration test (async): Upload a file to OSS using PutObjectAsync.
        /// This test creates a temporary file, uploads it to OSS asynchronously, and verifies the upload was successful.
        /// </summary>
        [Fact]
        public async Task PutObjectAsync_UploadsFileToOss_Successfully()
        {
            if (_skipTests)
            {
                // Skip test if credentials are not configured
                return;
            }

            // Arrange
            var testFileName = $"test-upload-async-{Guid.NewGuid()}.txt";
            var testContent = $"Test file content uploaded asynchronously at {DateTime.UtcNow:O}";
            var tempFilePath = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFilePath, testContent);

                // Act
                var response = await _client!.PutObjectFromFileAsync(_bucketName, testFileName, tempFilePath, "text/plain");

                // Assert
                Assert.NotNull(response);
                Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Created);
                Assert.NotNull(response.ETag);
                Assert.NotEmpty(response.ETag);
            }
            finally
            {
                // Cleanup: Delete temporary file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                // Cleanup: Delete uploaded file from OSS
                try
                {
                    await _client!.DeleteObjectAsync(new DeleteObjectRequest(_bucketName, testFileName));
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Integration test (async): Download a file from OSS using GetObjectAsync by key.
        /// This test uploads a file, downloads it by key asynchronously, and verifies the content matches.
        /// </summary>
        [Fact]
        public async Task GetObjectAsync_DownloadsFileFromOssByKey_Successfully()
        {
            if (_skipTests)
            {
                // Skip test if credentials are not configured
                return;
            }

            // Arrange
            var testFileName = $"test-download-async-{Guid.NewGuid()}.txt";
            var testContent = $"Test file content for async download at {DateTime.UtcNow:O}";

            try
            {
                // First, upload a test file
                using (var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent)))
                {
                    var putRequest = new PutObjectRequest(_bucketName, testFileName, uploadStream)
                    {
                        ContentType = "text/plain"
                    };
                    var putResponse = await _client!.PutObjectAsync(putRequest);
                    Assert.NotNull(putResponse);
                }

                // Act: Download the file by key
                var getRequest = new GetObjectRequest(_bucketName, testFileName);
                var getResponse = await _client!.GetObjectAsync(getRequest);

                // Assert
                Assert.NotNull(getResponse);
                Assert.True(getResponse.StatusCode == System.Net.HttpStatusCode.OK || getResponse.StatusCode == System.Net.HttpStatusCode.PartialContent);
                Assert.NotNull(getResponse.Content);

                using (var reader = new StreamReader(getResponse.Content, Encoding.UTF8))
                {
                    var downloadedContent = reader.ReadToEnd();
                    Assert.Equal(testContent, downloadedContent);
                }
            }
            finally
            {
                // Cleanup: Delete uploaded file from OSS
                try
                {
                    await _client!.DeleteObjectAsync(new DeleteObjectRequest(_bucketName, testFileName));
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
