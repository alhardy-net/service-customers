using System;
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
        public static void AddAwsSecretsManager(this WebHostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                var awsProfileEnv = Environment.GetEnvironmentVariable("AWS_PROFILE");

                if (string.IsNullOrWhiteSpace(awsProfileEnv))
                    throw new ApplicationException(
                        "AWS_PROFILE environment variable not found. Add the environment variable with value of the aws profile to use.");

                var chain = new CredentialProfileStoreChain("/root/.aws/credentials");
                chain.TryGetProfile(awsProfileEnv, out var awsProfile);
                var awsCredentials = awsProfile.GetAWSCredentials(awsProfile.CredentialProfileStore);
                
                configurationBuilder.AddSecretsManager(awsCredentials, awsProfile.Region, SetSecretsOptions);
            }
            else
            {
                configurationBuilder.AddSecretsManager(configurator:SetSecretsOptions);
            }
        }
        
        private static void SetSecretsOptions(SecretsManagerConfigurationProviderOptions ops)
        {
            ops.SecretFilter = entry => entry.Name.StartsWith("customers/customer-api");
            ops.KeyGenerator = (_, name) => name.Replace("__", ":");
        }
    }
}