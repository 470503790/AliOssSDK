# 使用指南（中文）

本文档在 README 的基础上提供更完整的示例。所有示例均可在引用 `AliOssSdk` 包的 .NET Framework 4.8 项目中直接运行。

## 创建客户端

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

## 列举 Bucket

**异步**

```csharp
var listBuckets = new ListBucketsOperation();
var response = await client.ExecuteAsync(listBuckets);

foreach (var bucket in response.Buckets)
{
    Console.WriteLine($"{bucket.Name} ({bucket.CreationDate})");
}
```

**同步**

```csharp
var listBuckets = new ListBucketsOperation();
var response = client.Execute(listBuckets);
```

## 创建 Bucket

**异步**

```csharp
var createBucket = new CreateBucketOperation(
    new CreateBucketRequest("my-demo-bucket")
    {
        Region = configuration.DefaultRegion
    });

await client.ExecuteAsync(createBucket);
```

**同步**

```csharp
client.Execute(createBucket);
```

## 上传对象

```csharp
var request = new PutObjectRequest(
    bucketName: "my-demo-bucket",
    objectKey: "images/logo.png",
    content: File.OpenRead("logo.png"),
    contentType: "image/png");

var operation = new PutObjectOperation(request);
```

**异步**

```csharp
var result = await client.ExecuteAsync(operation);
Console.WriteLine($"ETag: {result.ETag}");
```

**同步**

```csharp
var result = client.Execute(operation);
```

## 下载对象

```csharp
var get = new GetObjectOperation(new GetObjectRequest("my-demo-bucket", "images/logo.png"));
```

**异步**

```csharp
var downloaded = await client.ExecuteAsync(get);
using (var file = File.Create("logo-downloaded.png"))
{
    await downloaded.Content.CopyToAsync(file);
}
```

**同步**

```csharp
var downloaded = client.Execute(get);
using (var file = File.Create("logo-downloaded.png"))
{
    downloaded.Content.CopyTo(file);
}
```

## 删除对象

```csharp
var delete = new DeleteObjectOperation(new DeleteObjectRequest("my-demo-bucket", "images/logo.png"));
```

**异步**

```csharp
await client.ExecuteAsync(delete);
```

**同步**

```csharp
client.Execute(delete);
```

## 在不同位置间复制对象

**异步**

```csharp
var copy = await client.CopyObjectAsync(new CopyObjectRequest(
    sourceBucket: "my-demo-bucket",
    sourceKey: "images/logo.png",
    destinationBucket: "my-demo-bucket",
    destinationKey: "archives/logo-backup.png"));

Console.WriteLine($"Copied object with ETag {copy.ETag}");
```

**同步**

```csharp
var copyResult = client.CopyObject(new CopyObjectRequest(
    "my-demo-bucket",
    "images/logo.png",
    "my-demo-bucket",
    "archives/logo-backup.png"));
```

## 查看对象元数据（HEAD）

**异步**

```csharp
var headResponse = await client.HeadObjectAsync(new HeadObjectRequest("my-demo-bucket", "images/logo.png"));
Console.WriteLine($"Content-Length: {headResponse.ContentLength}, Content-Type: {headResponse.ContentType}");
```

**同步**

```csharp
var metadata = client.HeadObject(new HeadObjectRequest("my-demo-bucket", "images/logo.png"));
```

## 使用 OssRequestException 进行错误处理

当 OSS 返回非 2xx 状态码时，`OssHttpClient` 会抛出 `AliOssSdk.Http.OssRequestException`，其中带有状态码、请求 ID、响应头以及原始响应体：

```csharp
using AliOssSdk.Http;

try
{
    await client.ExecuteAsync(new DeleteObjectOperation(new DeleteObjectRequest("my-demo-bucket", "not-found.txt")));
}
catch (OssRequestException ex)
{
    Console.WriteLine($"异步调用失败: {ex.StatusCode}, RequestId = {ex.RequestId}");
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

## 分片上传

**异步**

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

**同步**

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

若需要取消未完成的分片上传，可调用 `AbortMultipartUpload` 或 `AbortMultipartUploadAsync` 并携带相同的 bucket/key/uploadId。

## 错误处理

每个 Operation 在服务返回错误状态码时都会抛出 `OssRequestException`。若需要检查响应体或状态码，可使用 try/catch 包裹执行：

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

## 自定义日志

任何 `AliOssSdk.Logging.ILogger` 实现都可以通过配置注入。推荐将诊断信息转发到应用日志框架：

```csharp
var configuration = new OssClientConfiguration(endpoint, accessKeyId, accessKeySecret)
{
    Logger = new SerilogLogger(Log.Logger)
};
```

## 依赖注入

由于所有协作者都是抽象，可以在任意容器中注册：

```csharp
services.AddSingleton(new OssClientConfiguration(endpoint, keyId, keySecret));
services.AddSingleton<IOssHttpClient, HttpClientAdapter>();
services.AddSingleton<IOssRequestSigner, OssRequestSignerV4>();
services.AddSingleton<IOssClient, OssClient>();
```

这样就能轻松替换传输层、签名器或日志实现，同时复用现有的高层 Operation。
