namespace CouchbaseDemo
{
    using System;
    using System.Diagnostics;

    using Couchbase;
    using Couchbase.Extensions.DependencyInjection;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/>.
        /// </returns>
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var watch = new Stopwatch();

            var bucketProvider = Services.GetRequiredService<IBucketProvider>();
            var bucket = await bucketProvider.GetBucketAsync("mybucket");

            /* var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "Password");                        
            var bucket = await cluster.BucketAsync("mybucket"); */

            var collection = bucket.DefaultCollection();

            watch.Start();

            for (int i = 0; i < 1000; i++)
            {
                var key = $"user-{i}::{Guid.NewGuid()}";
                var user = new User() { FirstName = $"FirstName_{i}", LastName = $"LastName_{i}", Email = $"test.{i}@mail.com" };

               _  = await collection.UpsertAsync(key, user);
               // Console.WriteLine($"MutationToken: {token.MutationToken}");
            }

            watch.Stop();

            Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds} ms.");

            var cloudBaseLifetimeService = Services.GetRequiredService<ICouchbaseLifetimeService>();
            await cloudBaseLifetimeService.CloseAsync();
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

            // https://docs.couchbase.com/dotnet-sdk/current/ref/client-settings.html
            var config = configBuilder.Build();
            services.AddCouchbase(config.GetSection("CouchbaseClusterOptions"));            
        }
    }
}
