// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyRustInventory.Application;
using MyRustInventory.Infrastructure;
using Microsoft.Extensions.Logging;
using MyRustInventory.Application.Common.Interfaces;

//using IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices(services =>
//    {
//        services.AddMyRustInventoryInfrastructureServices();

//    })
//    .Build();
public class Program
{
    public static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(c => c.AddConsole(opt => opt.LogToStandardErrorThreshold = LogLevel.Debug))
            .AddMyRustInventoryInfrastructureServices()
            .AddMyRustInventoryApplicationServices()
            .BuildServiceProvider();

        var logger = serviceProvider.GetService<ILoggerFactory>()
            .CreateLogger<Program>();
        logger.LogDebug("Starting application");

        var a = Convert.ToDecimal(null);
        Console.WriteLine(a);

        //do the actual work here
        //var steam = serviceProvider.GetService<ISteamClient>();
        //if (steam is not null)
        //    Console.WriteLine($"Login Sucessful? {steam.DoLogin("Netmonster01", "H@mmockH@ng3r01!")}");

        

        logger.LogDebug("All done!");
        Console.ReadKey();
    }

    private IServiceCollection ConfigureLogging(IServiceCollection factory)
    {
        factory.AddLogging(opt =>
        {
            opt.AddConsole();
        });
      return factory;
    }
}
