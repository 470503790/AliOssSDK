# Virtual-Host Style Endpoint Support

## Overview

AliOssSDK now supports both path-style and virtual-host style OSS endpoints. This document explains the difference and how to use each style.

## Endpoint Styles

### Path-Style (Default)

In path-style addressing, the bucket name is part of the URL path:

```
https://oss-cn-hangzhou.aliyuncs.com/bucket-name/object-key
```

**Example:**
```csharp
var config = new OssClientConfiguration(
    "https://oss-cn-hangzhou.aliyuncs.com",
    "accessKeyId",
    "accessKeySecret"
);
```

### Virtual-Host Style

In virtual-host style addressing, the bucket name is part of the hostname:

```
https://bucket-name.oss-cn-hangzhou.aliyuncs.com/object-key
```

**Example:**
```csharp
var config = new OssClientConfiguration(
    "https://mybucket.oss-cn-hangzhou.aliyuncs.com",
    "accessKeyId",
    "accessKeySecret"
)
{
    DefaultBucketName = "mybucket"
};
```

## Auto-Detection

The SDK automatically detects whether you're using virtual-host or path-style based on the endpoint URL:

- If the hostname starts with `{bucket}.oss-`, it's detected as virtual-host style
- If the hostname starts with `oss-`, it's detected as path-style
- For custom domains, path-style is assumed by default

## Explicit Configuration

You can explicitly set the endpoint style if auto-detection doesn't work for your scenario:

```csharp
var config = new OssClientConfiguration(
    "https://mybucket.oss-cn-hangzhou.aliyuncs.com",
    "accessKeyId",
    "accessKeySecret"
)
{
    DefaultBucketName = "mybucket",
    UseVirtualHostStyle = true  // Explicitly set to virtual-host style
};
```

## Why This Matters

The endpoint style affects how the SDK builds request URLs and calculates signatures:

### Path-Style Request
- Resource path: `/bucket/object-key`
- Full URL: `https://oss-cn-hangzhou.aliyuncs.com/bucket/object-key`

### Virtual-Host Style Request
- Resource path: `/object-key`
- Full URL: `https://bucket.oss-cn-hangzhou.aliyuncs.com/object-key`

If the wrong style is used, the signature calculation will fail, resulting in a `403 Forbidden` error with `SignatureDoesNotMatch`.

## Troubleshooting

### Error: SignatureDoesNotMatch

If you're getting signature errors:

1. **Check your endpoint**: Does it have the bucket name in the hostname?
   - Virtual-host: `https://mybucket.oss-cn-hangzhou.aliyuncs.com`
   - Path-style: `https://oss-cn-hangzhou.aliyuncs.com`

2. **Verify auto-detection**: Check if the SDK is detecting the correct style:
   ```csharp
   bool isVirtualHost = config.IsVirtualHostStyle("mybucket");
   Console.WriteLine($"Using virtual-host style: {isVirtualHost}");
   ```

3. **Explicitly set the style** if auto-detection is incorrect:
   ```csharp
   config.UseVirtualHostStyle = true;  // or false
   ```

### Example Error from the Issue

The original error showed:
```
CanonicalRequest: HEAD
/rca7/rca7/RcaFiles/G/a2/s_a1d593bb-a284-4dda-b95d-817c091736a2.png
```

This indicates the bucket name "rca7" was appearing twice in the path. With the fix:

**Before (incorrect):**
- Endpoint: `https://rca7.oss-cn-guangzhou.aliyuncs.com`
- Object key: `rca7/RcaFiles/G/a2/s_a1d593bb-a284-4dda-b95d-817c091736a2.png`
- Resource path: `/rca7/rca7/RcaFiles/G/a2/s_a1d593bb-a284-4dda-b95d-817c091736a2.png` ❌

**After (correct):**
- Endpoint: `https://rca7.oss-cn-guangzhou.aliyuncs.com`
- Object key: `rca7/RcaFiles/G/a2/s_a1d593bb-a284-4dda-b95d-817c091736a2.png`
- Resource path: `/rca7/RcaFiles/G/a2/s_a1d593bb-a284-4dda-b95d-817c091736a2.png` ✅

## Migration Guide

### Existing Code (Path-Style)

No changes needed! Your existing code continues to work:

```csharp
var config = new OssClientConfiguration(
    "https://oss-cn-hangzhou.aliyuncs.com",
    "accessKeyId",
    "accessKeySecret"
);

var client = new OssClient(config);
client.HeadObject(new HeadObjectRequest("mybucket", "mykey"));
```

### New Code (Virtual-Host Style)

Just use a virtual-host style endpoint, and the SDK handles the rest:

```csharp
var config = new OssClientConfiguration(
    "https://mybucket.oss-cn-hangzhou.aliyuncs.com",
    "accessKeyId",
    "accessKeySecret"
)
{
    DefaultBucketName = "mybucket"
};

var client = new OssClient(config);
client.HeadObject(new HeadObjectRequest("mybucket", "mykey"));
```

## Best Practices

1. **Use the same style consistently** throughout your application
2. **Set DefaultBucketName** when using virtual-host style for cleaner code
3. **Test signature calculation** when switching between styles
4. **Verify the endpoint URL** matches your OSS bucket configuration

## References

- [Alibaba Cloud OSS Documentation](https://www.alibabacloud.com/help/en/oss/)
- [OSS API Signature V4](https://www.alibabacloud.com/help/en/oss/developer-reference/signature-version-4)
