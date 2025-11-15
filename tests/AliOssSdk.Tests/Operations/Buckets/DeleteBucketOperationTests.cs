using System.Net;
using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets;

public class DeleteBucketOperationTests
{
    [Fact]
    public void BuildRequest_TargetsBucket()
    {
        var operation = new DeleteBucketOperation(new DeleteBucketRequest("bucket"));

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Delete, result.Method);
        Assert.Equal("/bucket", result.ResourcePath);
    }

    [Fact]
    public void ParseResponse_ReturnsStatusCode()
    {
        var response = OperationTestHelpers.CreateResponse(string.Empty, HttpStatusCode.NoContent);
        var operation = new DeleteBucketOperation(new DeleteBucketRequest("bucket"));

        var result = operation.ParseResponse(response);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
}
