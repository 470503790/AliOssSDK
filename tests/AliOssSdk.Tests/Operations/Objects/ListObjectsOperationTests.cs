using System;
using System.Net.Http;
using AliOssSdk.Models.Objects;
using AliOssSdk.Operations.Objects;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Objects;

public class ListObjectsOperationTests
{
    [Fact]
    public void BuildRequest_AppliesFilters()
    {
        var request = new ListObjectsRequest("bucket")
        {
            Prefix = "logs/",
            Delimiter = "/",
            Marker = "start",
            MaxKeys = 10
        };
        var operation = new ListObjectsOperation(request);

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/bucket", result.ResourcePath);
        Assert.Equal("logs/", result.QueryParameters["prefix"]);
        Assert.Equal("/", result.QueryParameters["delimiter"]);
        Assert.Equal("start", result.QueryParameters["marker"]);
        Assert.Equal("10", result.QueryParameters["max-keys"]);
    }

    [Fact]
    public void ParseResponse_ReturnsSummaries()
    {
        const string payload = """
<ListBucketResult xmlns="http://oss.aliyuncs.com/doc/2012-03-01/">
  <Contents>
    <Key>first</Key>
    <ETag>"etag1"</ETag>
    <Size>42</Size>
    <LastModified>2023-11-11T10:09:08Z</LastModified>
  </Contents>
  <Contents>
    <Key>second</Key>
    <ETag>"etag2"</ETag>
    <Size>100</Size>
  </Contents>
  <IsTruncated>true</IsTruncated>
  <NextMarker>next</NextMarker>
</ListBucketResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new ListObjectsOperation(new ListObjectsRequest("bucket"));

        var result = operation.ParseResponse(response);

        Assert.True(result.IsTruncated);
        Assert.Equal("next", result.NextMarker);
        Assert.Equal(2, result.Objects.Count);
        var first = Assert.Single(result.Objects, s => s.Key == "first");
        Assert.Equal("etag1", first.ETag);
        Assert.Equal(42, first.Size);
        Assert.Equal(new DateTimeOffset(2023, 11, 11, 10, 9, 8, TimeSpan.Zero), first.LastModified);
    }
}
