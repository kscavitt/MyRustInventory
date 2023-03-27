using MyRustInventory.Domain.Common;

namespace MyRustInventory.Application.Common.Interfaces
{
    public interface ISteamClient
    {
        Task<List<RustItemDto>> GetInventory(string steamId);
        Task<string> GetInventoryItemImage(string imageId);
        Task<MarketDataResponse> GetMarketData(string mhn, int currency = 1);
        Task<MarketDataResponse> GetMarketData(string mhn,string assetId, int currency = 1);
        bool DoLogin(string username, string password);   

    }
}
