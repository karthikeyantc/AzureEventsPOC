using System;
using System.IO;
using AzureEvent.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureEvent.Function.Startup))]
namespace AzureEvent.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            try
            {
                Console.WriteLine("Configuring services");
                builder.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build());
                Console.WriteLine("configuration added");
                builder.Services.AddLogging();
                Console.WriteLine("logging added");
                builder.Services.AddHttpClient();
                Console.WriteLine("http client added");
                builder.Services.AddSingleton<IErrorStorageHandler, ErrorStorageHandler>();
                Console.WriteLine("error storage handler added");
                builder.Services.AddSingleton<IEventModelConverterFactory, EventModelConverterFactory>();
                Console.WriteLine("event model converter factory added");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error configuring services: {ex.Message}");
            }
        }
    }
}