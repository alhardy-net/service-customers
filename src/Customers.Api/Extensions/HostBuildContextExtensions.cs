using System;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Kralizek.Extensions.Configuration.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Hosting
    // ReSharper restore CheckNamespace
{
    public static class HostBuildContextExtensions
    {
        public static (AWSCredentials, CredentialProfile) GetLocalAwsCredentials(this IHostEnvironment environment)
        {
            var awsProfileEnv = Environment.GetEnvironmentVariable("AWS_PROFILE");

            if (string.IsNullOrWhiteSpace(awsProfileEnv))
                throw new ApplicationException(
                    "AWS_PROFILE environment variable not found. Add the environment variable with value of the aws profile to use.");

            var chain = new CredentialProfileStoreChain("/root/.aws/credentials");
            chain.TryGetProfile(awsProfileEnv, out var awsProfile);
            var awsCredentials = awsProfile.GetAWSCredentials(awsProfile.CredentialProfileStore);

            return (awsCredentials, awsProfile);
        }

        public static void AddAwsSecretsManager(this WebHostBuilderContext context,
            IConfigurationBuilder configurationBuilder)
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                var (awsCredentials, awsProfile) = context.HostingEnvironment.GetLocalAwsCredentials();
                configurationBuilder.AddSecretsManager(awsCredentials, awsProfile.Region, SetSecretsOptions);
            }
            else
            {
                configurationBuilder.AddSecretsManager(configurator: SetSecretsOptions);
            }
        }

        private static void SetSecretsOptions(SecretsManagerConfigurationProviderOptions ops)
        {
            ops.SecretFilter = entry => entry.Name.StartsWith("customers/shared");
            ops.KeyGenerator = (_, name) => name.Replace("__", ":");
        }
    }
}