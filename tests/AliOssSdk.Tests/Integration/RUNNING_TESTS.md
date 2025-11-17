# Running Integration Tests - Example

This document provides a step-by-step guide on how to run the integration tests with real Alibaba Cloud OSS credentials.

## Prerequisites

1. An Alibaba Cloud account with OSS service enabled
2. A test bucket created in OSS (or permission to create one)
3. Access credentials (Access Key ID and Secret)

## Quick Start

### Step 1: Set Environment Variables

Choose your platform and set the required environment variables:

**Linux/macOS (bash/zsh):**
```bash
export OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
export OSS_ACCESS_KEY_ID="your-access-key-id"
export OSS_ACCESS_KEY_SECRET="your-access-key-secret"
export OSS_BUCKET_NAME="your-test-bucket"
export OSS_REGION="cn-hangzhou"
```

**Windows PowerShell:**
```powershell
$env:OSS_ENDPOINT="https://oss-cn-hangzhou.aliyuncs.com"
$env:OSS_ACCESS_KEY_ID="your-access-key-id"
$env:OSS_ACCESS_KEY_SECRET="your-access-key-secret"
$env:OSS_BUCKET_NAME="your-test-bucket"
$env:OSS_REGION="cn-hangzhou"
```

**Windows CMD:**
```cmd
set OSS_ENDPOINT=https://oss-cn-hangzhou.aliyuncs.com
set OSS_ACCESS_KEY_ID=your-access-key-id
set OSS_ACCESS_KEY_SECRET=your-access-key-secret
set OSS_BUCKET_NAME=your-test-bucket
set OSS_REGION=cn-hangzhou
```

### Step 2: Run the Integration Tests

Navigate to the project root and run:

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Step 3: Verify Results

You should see output similar to:

```
Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: X.XXX Seconds
```

## What the Tests Do

The integration tests perform the following operations against real OSS:

1. **PutObject_UploadsFileToOss_Successfully**
   - Creates a temporary file with test content
   - Uploads it to your OSS bucket
   - Verifies the upload succeeded
   - Cleans up by deleting the file

2. **GetObject_DownloadsFileFromOssByKey_Successfully**
   - Uploads a test file to OSS
   - Downloads it back using the object key
   - Verifies the downloaded content matches the original
   - Cleans up by deleting the file

3. **PutObjectAsync_UploadsFileToOss_Successfully**
   - Same as test 1, but uses async operations

4. **GetObjectAsync_DownloadsFileFromOssByKey_Successfully**
   - Same as test 2, but uses async operations

## Troubleshooting

### Tests are skipped
If you see "Total tests: 4, Passed: 4" but the tests complete instantly (< 100ms), the tests are being skipped because credentials are not configured. Verify your environment variables are set correctly.

### Authentication errors
If you get 403 Forbidden errors, check:
- Access Key ID and Secret are correct
- The credentials have permissions to put/get/delete objects in the bucket
- The bucket exists and is in the specified region

### Network errors
If you get connection timeouts:
- Check your network connectivity
- Verify the endpoint URL is correct for your region
- Check if there are any firewall rules blocking access to Alibaba Cloud

## Available OSS Regions

Common OSS endpoints:
- China (Hangzhou): `https://oss-cn-hangzhou.aliyuncs.com`
- China (Shanghai): `https://oss-cn-shanghai.aliyuncs.com`
- China (Beijing): `https://oss-cn-beijing.aliyuncs.com`
- China (Shenzhen): `https://oss-cn-shenzhen.aliyuncs.com`
- US (Silicon Valley): `https://oss-us-west-1.aliyuncs.com`
- Singapore: `https://oss-ap-southeast-1.aliyuncs.com`

For a complete list, see: https://www.alibabacloud.com/help/en/oss/user-guide/regions-and-endpoints

## Security Best Practices

- **Never commit credentials to source control**
- Use environment variables or secure credential management
- Use a dedicated test bucket, not a production bucket
- Regularly rotate your access keys
- Use RAM (Resource Access Management) to create limited-permission credentials for testing
