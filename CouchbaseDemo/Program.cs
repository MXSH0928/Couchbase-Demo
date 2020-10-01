using Couchbase;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Management.Buckets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace CouchbaseDemo
{
    /// <summary>
    /// SOURCE: https://docs.couchbase.com/tutorials/quick-start/quickstart-dotnet27-aspnetcore31-visualstudio-firstquery-cb65.html
    /// </summary>
    class Program
    {
        /// <summary>
        /// The services.
        /// </summary>
        private static readonly IServiceProvider Services;

        /// <summary>
        /// The Program logger.
        /// </summary>
        private static readonly ILogger<Program> Logger;

        /// <summary>
        /// Initializes static members of the <see cref="Program"/> class.
        /// </summary>
        static Program()
        {
            var serviceCollection = new ServiceCollection();
            string[] arguments = Environment.GetCommandLineArgs();
            Configure(serviceCollection, arguments);

            Services = serviceCollection.BuildServiceProvider();
            Logger = Services.GetRequiredService<ILogger<Program>>();
            Logger.LogInformation($"{nameof(Program)} class has been instantiated.\n\n");
        }

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            /* var bucketProvider = Services.GetRequiredService<IBucketProvider>();
            var bucket = await bucketProvider.GetBucketAsync("default"); */

            var cluster = await Cluster.ConnectAsync("http://localhost:8091", "admin", "Demo.01");            
            var buckets = await cluster.Buckets.GetAllBucketsAsync();
                        
            var bucket = await cluster.BucketAsync("default-1");
            var collection = bucket.DefaultCollection();

            var key = Guid.NewGuid().ToString();
            var user = new User() { FirstName = "Mohammed", LastName = "Hoque", Email = "test@mail.com" };
            _ = await collection.UpsertAsync(key, user);

            Console.WriteLine();
            var cbLifetimeService = Services.GetRequiredService<ICouchbaseLifetimeService>();
            await cbLifetimeService.CloseAsync();
        }

        private static void Configure(IServiceCollection services, string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                // .AddJsonFile($"appsettings.{environmentName}.json", true, true);
                // .AddEnvironmentVariables();

            services.AddLogging(
                builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddConsole();
                });

            var config = configBuilder.Build();
            var cbConfig = config.GetSection("Couchbase");
            services.AddCouchbase(cbConfig);            
        }
    }
}
