using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets
{

    public class CreateBucketOperationTests
    {
        [Fact]
        public void BuildRequest_UsesRequestRegionWhenProvided()
        {
            var request = new CreateBucketRequest("bucket-name")
            {
                Region = "cn-beijing"
            };
            var operation = new CreateBucketOperation(request);

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext("cn-hangzhou"));

            Assert.Equal(HttpMethod.Put, result.Method);
            Assert.Equal("/bucket-name/", result.ResourcePath);
            Assert.Equal("cn-beijing", result.Headers["x-oss-bucket-region"]);
        }

        [Fact]
        public void BuildRequest_FallsBackToDefaultRegion()
        {
            var request = new CreateBucketRequest("bucket-name");
            var operation = new CreateBucketOperation(request);

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext("cn-shanghai"));

            Assert.Equal("cn-shanghai", result.Headers["x-oss-bucket-region"]);
        }

        [Fact]
        public void ParseResponse_ReadsLocationHeader()
        {
            var headers = OperationTestHelpers.Headers(("Location", "/bucket-name"));
            var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.OK, headers);
            var operation = new CreateBucketOperation(new CreateBucketRequest("bucket-name"));

            var result = operation.ParseResponse(response);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("/bucket-name", result.Location);
        }
    }
}
