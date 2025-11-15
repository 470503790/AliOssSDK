using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AliOssSdk.Configuration
{
    /// <summary>
    /// Represents a POCO that can be hydrated from JSON or other configuration sources
    /// to build <see cref="OssClientConfiguration"/> instances.
    /// </summary>
    [DataContract]
    public sealed class AlibabaOssConfig
    {
        public const string DefaultEnvironmentPrefix = "ALI_OSS_";

        public const string DefaultEndpoint = "https://oss-cn-hangzhou.aliyuncs.com";

        [DataMember(Name = "region", EmitDefaultValue = false)]
        public string? Region { get; set; }

        [DataMember(Name = "bucket", EmitDefaultValue = false)]
        public string? Bucket { get; set; }

        [DataMember(Name = "endpoint", EmitDefaultValue = false)]
        public string? Endpoint { get; set; }

        [DataMember(Name = "accessKeyId", IsRequired = true)]
        public string? AccessKeyId { get; set; }

        [DataMember(Name = "accessKeySecret", IsRequired = true)]
        public string? AccessKeySecret { get; set; }

        [DataMember(Name = "sign_duration_second", EmitDefaultValue = false)]
        public long? SignDurationSeconds { get; set; }

        public static AlibabaOssConfig FromJson(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return Deserialize(stream);
        }

        public static AlibabaOssConfig FromJsonFile(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            using var stream = File.OpenRead(filePath);
            return Deserialize(stream);
        }

        public AlibabaOssConfig ApplyEnvironmentOverrides(string prefix = DefaultEnvironmentPrefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            Region = ResolveEnvironmentValue(prefix + "REGION") ?? Region;
            Bucket = ResolveEnvironmentValue(prefix + "BUCKET") ?? Bucket;
            Endpoint = ResolveEnvironmentValue(prefix + "ENDPOINT") ?? Endpoint;
            AccessKeyId = ResolveEnvironmentValue(prefix + "ACCESS_KEY_ID") ?? AccessKeyId;
            AccessKeySecret = ResolveEnvironmentValue(prefix + "ACCESS_KEY_SECRET") ?? AccessKeySecret;

            var duration = ResolveEnvironmentValue(prefix + "SIGN_DURATION_SECOND");
            if (!string.IsNullOrEmpty(duration) && long.TryParse(duration, out var seconds))
            {
                SignDurationSeconds = seconds;
            }

            return this;
        }

        public OssClientConfiguration ToOssClientConfiguration()
        {
            if (string.IsNullOrWhiteSpace(AccessKeyId))
            {
                throw new InvalidOperationException("AccessKeyId is required to build OssClientConfiguration.");
            }

            if (string.IsNullOrWhiteSpace(AccessKeySecret))
            {
                throw new InvalidOperationException("AccessKeySecret is required to build OssClientConfiguration.");
            }

            var endpoint = string.IsNullOrWhiteSpace(Endpoint)
                ? new Uri(DefaultEndpoint)
                : new Uri(Endpoint);

            var configuration = new OssClientConfiguration(endpoint, AccessKeyId, AccessKeySecret)
            {
                DefaultRegion = Region
            };

            return configuration;
        }

        private static AlibabaOssConfig Deserialize(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(AlibabaOssConfig));
            if (serializer.ReadObject(stream) is not AlibabaOssConfig config)
            {
                throw new SerializationException("Unable to deserialize AlibabaOssConfig");
            }

            return config;
        }

        private static string? ResolveEnvironmentValue(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}
