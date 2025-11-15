# AliOssSDK

A lightweight, dependency-injection friendly OSS client for .NET Framework 4.8. The solution models the core Alibaba Cloud OSS operations through composable operations and a configurable HTTP layer, enabling both synchronous and asynchronous workflows.

## Projects

- **AliOssSdk** – Class library that exposes abstractions such as `IOssClient`, request/response DTOs, operations, and a pluggable HTTP/signing pipeline.

## Key Features

- OSS operations are represented as `IOssOperation<TResponse>` implementations (e.g., creating buckets, listing buckets, uploading/downloading/deleting objects). New operations can be added without modifying the client.
- Dependency-injected configuration (`OssClientConfiguration`), HTTP layer (`IOssHttpClient`), and request signing (`IOssRequestSigner`).
- Shared request-handling pipeline using template method style execution inside `OssClient`, with both synchronous and asynchronous methods.
- Request/response DTOs for bucket and object operations for discoverable, testable models.

## Usage Example

```csharp
var configuration = new OssClientConfiguration(new Uri("https://oss-your-region.aliyuncs.com"), "accessKey", "secretKey")
{
    DefaultRegion = "oss-your-region"
};

using var client = new OssClient(configuration);
var createBucket = new CreateBucketOperation(new CreateBucketRequest("example-bucket"));
await client.ExecuteAsync(createBucket);
```
