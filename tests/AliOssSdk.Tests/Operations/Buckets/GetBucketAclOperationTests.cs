using System.Net.Http;
using AliOssSdk.Models.Buckets;
using AliOssSdk.Operations.Buckets;
using Xunit;
using AliOssSdk.Tests.Operations;

namespace AliOssSdk.Tests.Operations.Buckets;

public class GetBucketAclOperationTests
{
    [Fact]
    public void BuildRequest_SetsAclQuery()
    {
        var operation = new GetBucketAclOperation(new GetBucketAclRequest("bucket"));

        var result = operation.BuildRequest(OperationTestHelpers.CreateContext());

        Assert.Equal(HttpMethod.Get, result.Method);
        Assert.Equal("/bucket", result.ResourcePath);
        Assert.True(result.QueryParameters.ContainsKey("acl"));
    }

    [Fact]
    public void ParseResponse_ReadsOwnerAndGrant()
    {
        const string payload = """
<AccessControlPolicy>
  <Owner>
    <ID>owner-id</ID>
    <DisplayName>owner</DisplayName>
  </Owner>
  <AccessControlList>
    <Grant>public-read</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";
        var response = OperationTestHelpers.CreateResponse(payload);
        var operation = new GetBucketAclOperation(new GetBucketAclRequest("bucket"));

        var result = operation.ParseResponse(response);

        Assert.Equal("owner-id", result.OwnerId);
        Assert.Equal("owner", result.OwnerDisplayName);
        Assert.Equal("public-read", result.Grant);
    }
}
