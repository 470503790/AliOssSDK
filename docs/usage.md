# Usage guide

This document expands on the README with practical examples. Every snippet is valid in .NET Framework 4.8 projects that reference the `AliOssSdk` package.

## Creating a client

```csharp
var configuration = new OssClientConfiguration(
    new Uri("https://oss-cn-hangzhou.aliyuncs.com"),
    Environment.GetEnvironmentVariable("ALI_OSS_ACCESS_KEY_ID"),
    Environment.GetEnvironmentVariable("ALI_OSS_ACCESS_KEY_SECRET"))
{
    DefaultRegion = "oss-cn-hangzhou",
    Logger = new ConsoleLogger()
};

IOssClient client = new OssClient(configuration);
```

## Listing buckets

**Async**

```csharp
var listBuckets = new ListBucketsOperation();
var response = await client.ExecuteAsync(listBuckets);

foreach (var bucket in response.Buckets)
{
    Console.WriteLine($"{bucket.Name} ({bucket.CreationDate})");
}
```

**Sync**

```csharp
var listBuckets = new ListBucketsOperation();
var response = client.Execute(listBuckets);
```

## Creating a bucket

**Async**

```csharp
var createBucket = new CreateBucketOperation(
    new CreateBucketRequest("my-demo-bucket")
    {
        Region = configuration.DefaultRegion
    });

await client.ExecuteAsync(createBucket);
```

**Sync**

```csharp
client.Execute(createBucket);
```

## Uploading an object

```csharp
var request = new PutObjectRequest(
    bucketName: "my-demo-bucket",
    objectKey: "images/logo.png",
    content: File.OpenRead("logo.png"),
    contentType: "image/png");

var operation = new PutObjectOperation(request);
```

**Async**

```csharp
var result = await client.ExecuteAsync(operation);
Console.WriteLine($"ETag: {result.ETag}");
```

**Sync**

```csharp
var result = client.Execute(operation);
```

## Downloading an object

```csharp
var get = new GetObjectOperation(new GetObjectRequest("my-demo-bucket", "images/logo.png"));
```

**Async**

```csharp
var downloaded = await client.ExecuteAsync(get);
using (var file = File.Create("logo-downloaded.png"))
{
    await downloaded.Content.CopyToAsync(file);
}
```

**Sync**

```csharp
var downloaded = client.Execute(get);
using (var file = File.Create("logo-downloaded.png"))
{
    downloaded.Content.CopyTo(file);
}
```

## Deleting an object

```csharp
var delete = new DeleteObjectOperation(new DeleteObjectRequest("my-demo-bucket", "images/logo.png"));
```

**Async**

```csharp
await client.ExecuteAsync(delete);
```

**Sync**

```csharp
client.Execute(delete);
```

## Listing objects within a bucket

```csharp
var listObjectsRequest = new ListObjectsRequest("my-demo-bucket")
{
    Prefix = "images/",
    MaxKeys = 100
};

var operation = await client.ListObjectsAsync(listObjectsRequest);
foreach (var obj in operation.Objects)
{
    Console.WriteLine($"{obj.Key} ({obj.Size} bytes)");
}
```

**Sync**

```csharp
var objects = client.ListObjects(listObjectsRequest);
```

## Retrieving bucket metadata and ACL

```csharp
var bucketInfo = await client.GetBucketInfoAsync(new GetBucketInfoRequest("my-demo-bucket"));
Console.WriteLine($"{bucketInfo.Name} lives in {bucketInfo.Location}");

var acl = await client.GetBucketAclAsync(new GetBucketAclRequest("my-demo-bucket"));
Console.WriteLine($"Owner: {acl.OwnerDisplayName}, Grant: {acl.Grant}");
```

**Sync**

```csharp
var bucketInfoSync = client.GetBucketInfo(new GetBucketInfoRequest("my-demo-bucket"));
var aclSync = client.GetBucketAcl(new GetBucketAclRequest("my-demo-bucket"));
```

## Copying objects between locations

```csharp
var copy = await client.CopyObjectAsync(new CopyObjectRequest(
    sourceBucket: "my-demo-bucket",
    sourceKey: "images/logo.png",
    destinationBucket: "my-demo-bucket",
    destinationKey: "archives/logo-backup.png"));

Console.WriteLine($"Copied object with ETag {copy.ETag}");
```

**Sync**

```csharp
var copyResult = client.CopyObject(new CopyObjectRequest(
    "my-demo-bucket",
    "images/logo.png",
    "my-demo-bucket",
    "archives/logo-backup.png"));
```

## Checking object metadata (HEAD)

```csharp
var headResponse = await client.HeadObjectAsync(new HeadObjectRequest("my-demo-bucket", "images/logo.png"));
Console.WriteLine($"Content-Length: {headResponse.ContentLength}, Content-Type: {headResponse.ContentType}");
```

**Sync**

```csharp
var metadata = client.HeadObject(new HeadObjectRequest("my-demo-bucket", "images/logo.png"));
```

## Error handling with OssRequestException

`OssHttpClient` throws `AliOssSdk.Http.OssRequestException` whenever OSS returns a non-success HTTP status code. The exception exposes the status code, headers, request ID, and the raw response body:

```csharp
using AliOssSdk.Http;

try
{
    await client.ExecuteAsync(new DeleteObjectOperation(new DeleteObjectRequest("my-demo-bucket", "not-found.txt")));
}
catch (OssRequestException ex)
{
    Console.WriteLine($"Async failure {ex.StatusCode}, request ID {ex.RequestId}");
    Console.WriteLine(ex.ResponseBody);
}

try
{
    client.Execute(new GetObjectOperation(new GetObjectRequest("my-demo-bucket", "not-found.txt")));
}
catch (OssRequestException ex)
{
    Console.WriteLine(string.Join("\n", ex.ResponseHeaders));
}
```

## Multipart uploads

```csharp
var initiateResponse = await client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest("my-demo-bucket", "videos/demo.mp4"));

var uploadedParts = new List<CompleteMultipartUploadRequest.UploadedPart>();
using (var part1 = File.OpenRead("part1.bin"))
{
    var part1Response = await client.UploadPartAsync(new UploadPartRequest("my-demo-bucket", "videos/demo.mp4", initiateResponse.UploadId!, 1, part1));
    uploadedParts.Add(new CompleteMultipartUploadRequest.UploadedPart(1, part1Response.ETag!));
}

using (var part2 = File.OpenRead("part2.bin"))
{
    var part2Response = await client.UploadPartAsync(new UploadPartRequest("my-demo-bucket", "videos/demo.mp4", initiateResponse.UploadId!, 2, part2));
    uploadedParts.Add(new CompleteMultipartUploadRequest.UploadedPart(2, part2Response.ETag!));
}

var completeResponse = await client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest(
    "my-demo-bucket",
    "videos/demo.mp4",
    initiateResponse.UploadId!,
    uploadedParts));
```

**Sync**

```csharp
var initiate = client.InitiateMultipartUpload(new InitiateMultipartUploadRequest("my-demo-bucket", "videos/demo.mp4"));

var parts = new List<CompleteMultipartUploadRequest.UploadedPart>();
using (var part = File.OpenRead("part1.bin"))
{
    var result = client.UploadPart(new UploadPartRequest("my-demo-bucket", "videos/demo.mp4", initiate.UploadId!, 1, part));
    parts.Add(new CompleteMultipartUploadRequest.UploadedPart(1, result.ETag!));
}

var completed = client.CompleteMultipartUpload(new CompleteMultipartUploadRequest("my-demo-bucket", "videos/demo.mp4", initiate.UploadId!, parts));
```

Call `AbortMultipartUpload` or `AbortMultipartUploadAsync` with the same bucket/key/uploadId if you need to cancel an unfinished upload.

## Error handling

Every operation throws `OssRequestException` when the service returns a failure status code. Wrap executions with try/catch if you need to inspect the response body or status code.

```csharp
try
{
    await client.ExecuteAsync(delete);
}
catch (OssRequestException ex)
{
    Console.WriteLine($"Failed with status {ex.StatusCode}: {ex.Message}");
}
```

## Custom logging

Any `AliOssSdk.Logging.ILogger` implementation can be supplied through the configuration. This is the recommended way to forward diagnostics to your application's logging framework.

```csharp
var configuration = new OssClientConfiguration(endpoint, accessKeyId, accessKeySecret)
{
    Logger = new SerilogLogger(Log.Logger)
};
```

## Dependency injection

Because all collaborators are abstractions, register them with your container of choice:

```csharp
services.AddSingleton(new OssClientConfiguration(endpoint, keyId, keySecret));
services.AddSingleton<IOssHttpClient, HttpClientAdapter>();
services.AddSingleton<IOssRequestSigner, OssRequestSignerV4>();
services.AddSingleton<IOssClient, OssClient>();
```

This makes it easy for future contributors to swap transports, signers, or loggers while retaining the same higher-level operation implementations.
