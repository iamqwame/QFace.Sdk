using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QFace.Sdk.BlobStorage.Extensions;

 public static class BlobStorageExtensions
    {
        public static IServiceCollection AddBlobStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            
            // Register services
            services.AddSingleton<IAmazonS3>(sp => CreateS3Client(sp));
            services.AddScoped<IFileUploadService, FileUploadService>();
            
            return services;
        }
        
        private static IAmazonS3 CreateS3Client(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("BlobStorageExtensions");
            
            try
            {
                logger.LogInformation("Configuring blob storage services...");
                
                // Get configuration values
                var serviceUrl = options.ServiceURL;
                var accessKey = options.Credentials?.AccessKey;
                var secretKey = options.Credentials?.SecretKey;
                var region = options.Region ?? "us-east-1";
                var provider = ParseProviderType(options.Provider ?? "DigitalOcean", logger);
                
                // For Digital Ocean, we should use virtual-hosted style (not path style)
                var isDigitalOcean = provider == S3Provider.DigitalOcean || 
                    (!string.IsNullOrEmpty(serviceUrl) && 
                     serviceUrl.Contains("digitaloceanspaces.com", StringComparison.OrdinalIgnoreCase));
                var forcePathStyle = !isDigitalOcean && options.ForcePathStyle;
                
                logger.LogInformation("Blob Storage Configuration: Provider={Provider}, ServiceURL={ServiceUrl}, Region={Region}, ForcePathStyle={ForcePathStyle}, IsDigitalOcean={IsDigitalOcean}",
                    provider, serviceUrl, region, forcePathStyle, isDigitalOcean);

                // Create custom credentials if provided
                AWSCredentials credentials = null;
                if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                {
                    credentials = new BasicAWSCredentials(accessKey, secretKey);
                    logger.LogInformation("Using BasicAWSCredentials with provided access and secret keys");
                }

                // Create S3 client configuration
                var clientConfig = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(region),
                    ForcePathStyle = forcePathStyle,
                    SignatureVersion = "4", // Always use v4 signature
                    SignatureMethod = SigningAlgorithm.HmacSHA256 // Use SHA256
                };

                // Set service URL if provided (for DigitalOcean or other S3-compatible services)
                if (!string.IsNullOrEmpty(serviceUrl))
                {
                    clientConfig.ServiceURL = serviceUrl;
                    
                    // For Digital Ocean, ensure we're using HTTPS protocol
                    if (isDigitalOcean && Uri.TryCreate(serviceUrl, UriKind.Absolute, out var uri))
                    {
                        // Always ensure we use https for Digital Ocean
                        if (uri.Scheme == "http")
                        {
                            var builder = new UriBuilder(uri) { Scheme = "https" };
                            clientConfig.ServiceURL = builder.Uri.ToString();
                            logger.LogInformation("Converted Digital Ocean service URL to HTTPS: {ServiceUrl}", clientConfig.ServiceURL);
                        }
                    }
                    
                    logger.LogInformation("Using custom service URL: {ServiceUrl}", clientConfig.ServiceURL);
                }
                else
                {
                    switch (provider)
                    {
                        // Provider-specific endpoint formats if no custom URL provided
                        case S3Provider.DigitalOcean:
                            clientConfig.ServiceURL = $"https://{region}.digitaloceanspaces.com";
                            break;
                        case S3Provider.Backblaze:
                            clientConfig.ServiceURL = "https://s3.us-west-001.backblazeb2.com";
                            break;
                        case S3Provider.Wasabi:
                            clientConfig.ServiceURL = $"https://s3.{region}.wasabisys.com";
                            break;
                        case S3Provider.MinIO:
                            // MinIO typically requires a custom URL
                            throw new ArgumentException("ServiceURL is required for MinIO provider");
                        case S3Provider.AWS:
                            // No need to set ServiceURL for AWS
                            break;
                        // Generic provider should provide ServiceURL
                        case S3Provider.Generic when string.IsNullOrEmpty(serviceUrl):
                            throw new ArgumentException("ServiceURL is required for Generic S3 provider");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    logger.LogInformation("Using provider-specific service URL: {ServiceUrl}", clientConfig.ServiceURL);
                }

                // Create client with credentials
                if (credentials != null)
                {
                    logger.LogInformation("Creating S3 client with custom credentials");
                    return new AmazonS3Client(credentials, clientConfig);
                }

                // Fall back to default credentials
                logger.LogInformation("Creating S3 client with default credentials");
                return new AmazonS3Client(clientConfig);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error configuring blob storage services");
                throw;
            }
        }
        
        private static S3Provider ParseProviderType(string providerString, ILogger logger)
        {
            if (Enum.TryParse<S3Provider>(providerString, true, out var provider))
            {
                return provider;
            }
            
            logger.LogWarning("Unknown S3 provider '{Provider}', defaulting to DigitalOcean", providerString);
            return S3Provider.DigitalOcean;
        }
    }