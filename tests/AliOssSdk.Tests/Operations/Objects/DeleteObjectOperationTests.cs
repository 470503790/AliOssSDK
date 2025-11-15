using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects
{

    public class DeleteObjectOperationTests
    {
        [Fact]
        public void BuildRequest_TargetsObject()
        {
            var operation = new DeleteObjectOperation(new DeleteObjectRequest("bucket", "key"));

            var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

            Assert.Equal(HttpMethod.Delete, result.Method);
            Assert.Equal("/bucket/key", result.ResourcePath);
        }

        [Fact]
        public void ParseResponse_ReturnsStatus()
        {
            var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.NoContent);
            var operation = new DeleteObjectOperation(new DeleteObjectRequest("bucket", "key"));

            var result = operation.ParseResponse(response);

            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }
    }
}
