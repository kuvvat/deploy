using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;
using Serilog.Exceptions;

namespace Api
{
    /// <summary>
    /// Program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            // configure logging first
            ConfigureLogging();

            // then create the host, so that if the host fails we can log errors
            CreateHost(args);
        }

        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithCorrelationId()
                .Enrich.WithEnvironment("OS")
                .Enrich.WithProperty("Environment", environment)
                .WriteTo.Console()
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment ?? "Development"))
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {

            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                InlineFields = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name}-{environment}-{DateTime.UtcNow:yyyy-MM}"
            };
        }

        private static void CreateHost(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}", ex);
                throw;
            }
        }

        /// <summary>
        /// Create host builder
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
                }).UseSerilog();
    }
}
