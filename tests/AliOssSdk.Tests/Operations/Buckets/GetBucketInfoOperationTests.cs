using System;
using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets;

public class GetBucketInfoOperationTests
{
    [Fact]
    public void BuildRequest_SetsBucketInfoQuery()
    {
        var operation = new GetBucketInfoOperation(new GetBucketInfoRequest("bucket"));

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/bucket", result.ResourcePath);
        Assert.True(result.QueryParameters.ContainsKey("bucketInfo"));
    }

    [Fact]
    public void ParseResponse_ExtractsMetadata()
    {
        const string payload = """
<GetBucketInfoResult>
  <Bucket>
    <Name>bucket</Name>
    <Location>cn-shanghai</Location>
    <CreationDate>2023-12-04T05:06:07Z</CreationDate>
  </Bucket>
</GetBucketInfoResult>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new GetBucketInfoOperation(new GetBucketInfoRequest("bucket"));

        var result = operation.ParseResponse(response);

        Assert.Equal("bucket", result.Name);
        Assert.Equal("cn-shanghai", result.Location);
        Assert.Equal(new DateTimeOffset(2023, 12, 4, 5, 6, 7, TimeSpan.Zero), result.CreationDate);
    }
}
