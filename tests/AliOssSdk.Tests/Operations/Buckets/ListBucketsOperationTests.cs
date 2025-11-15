using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets;

public class ListBucketsOperationTests
{
    [Fact]
    public void BuildRequest_AddsOptionalFilters()
    {
        var request = new ListBucketsRequest
        {
            Prefix = "project-",
            MaxKeys = 50
        };
        var operation = new ListBucketsOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/", result.ResourcePath);
        Assert.Equal("project-", result.QueryParameters["prefix"]);
        Assert.Equal("50", result.QueryParameters["max-keys"]);
    }

    [Fact]
    public void ParseResponse_ReadsBucketNamesAndMarker()
    {
        const string payload = """
<ListAllMyBucketsResult>
  <Buckets>
    <Bucket><Name>first</Name></Bucket>
    <Bucket><Name>second</Name></Bucket>
  </Buckets>
  <NextMarker>next-token</NextMarker>
</ListAllMyBucketsResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new ListBucketsOperation(new ListBucketsRequest());

        var result = operation.ParseResponse(response);

        Assert.Equal(new[] { "first", "second" }, result.Buckets);
        Assert.Equal("next-token", result.NextMarker);
    }
}
