using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Serilog_ELK.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSerilog(this IServiceCollection services) {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");  
            var configuration = new ConfigurationBuilder()  
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  
                                .AddJsonFile(  
                                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",  
                                    optional: true)  
                                .Build();
            Log.Logger = new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .WriteTo.Console()
                         .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))  
                         {  
                             AutoRegisterTemplate = true,  
                             IndexFormat = $"test-{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
                             ModifyConnectionSettings = x => x.BasicAuthentication("elastic", "your-password"),
                         })  
                         .Enrich.WithProperty("Environment", environment)  
                         .ReadFrom.Configuration(configuration)  
                         .CreateLogger();
        }
    }
}