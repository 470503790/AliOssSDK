using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AliOssSdk.Configuration;
using AliOssSdk.Http;

namespace AliOssSdk.Security
{
    public sealed class HmacSha1RequestSigner : IOssRequestSigner
    {
        public void Sign(OssHttpRequest request, OssClientConfiguration configuration)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var date = DateTimeOffset.UtcNow.ToString("r", CultureInfo.InvariantCulture);
            request.Headers["Date"] = date;

            var stringToSign = string.Join("\n", new[]
            {
                request.Method.Method,
                string.Empty,
                request.ContentType ?? string.Empty,
                date,
                request.ResourcePath
            });

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(configuration.AccessKeySecret)))
            {
                var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
                request.Headers["Authorization"] = $"OSS {configuration.AccessKeyId}:{signature}";
            }
        }
    }
}
