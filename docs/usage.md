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
services.AddSingleton<IOssRequestSigner, HmacSha1RequestSigner>();
services.AddSingleton<IOssClient, OssClient>();
```

This makes it easy for future contributors to swap transports, signers, or loggers while retaining the same higher-level operation implementations.
