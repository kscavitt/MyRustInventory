using Microsoft.Extensions.DependencyInjection;

namespace MyRustInventory.Client
{
    public static class MyRustInventoryClientSideCarExtention
    {

        public static IServiceCollection AddMyRustInventoryClient(this IServiceCollection services)
        {
            services.AddHttpClient<ISteamClient, SteamService>();
            return services;
        }
    }
}
