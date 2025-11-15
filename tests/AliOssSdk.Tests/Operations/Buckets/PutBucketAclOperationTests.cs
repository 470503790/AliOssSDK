using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets
{

    public class PutBucketAclOperationTests
    {
        [Fact]
        public void BuildRequest_SetsAclHeader()
        {
            var request = new PutBucketAclRequest("bucket", BucketAcl.PublicReadWrite);
            var operation = new PutBucketAclOperation(request);

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

            Assert.Equal(HttpMethod.Put, result.Method);
            Assert.Equal("/bucket", result.ResourcePath);
            Assert.True(result.QueryParameters.ContainsKey("acl"));
            Assert.Equal("public-read-write", result.Headers["x-oss-acl"]);
        }

        [Fact]
        public void ParseResponse_ReturnsStatus()
        {
            var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.OK);
            var operation = new PutBucketAclOperation(new PutBucketAclRequest("bucket", BucketAcl.Private));

            var result = operation.ParseResponse(response);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
