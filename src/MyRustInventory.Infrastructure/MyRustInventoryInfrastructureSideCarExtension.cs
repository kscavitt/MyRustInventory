using Microsoft.Extensions.DependencyInjection;
using MyRustInventory.Application.Common.Interfaces;
using MyRustInventory.Infrastructure.Services;

namespace MyRustInventory.Infrastructure
{
    public static class MyRustInventoryInfrastructureSideCarExtension
    {
        public static IServiceCollection AddMyRustInventoryInfrastructureServices(this IServiceCollection services)
        {
            // add memory cache
            services.AddMemoryCache();
            // add our cache service
            services.AddScoped<ICacheService, CacheService>();
            // add our custom steam httpclient
            services.AddHttpClient<ISteamClient, SteamService>();
            return services;
        }
    }
}