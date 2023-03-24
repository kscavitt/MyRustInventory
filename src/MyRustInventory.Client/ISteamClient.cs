using MyRustInventory.Domain.Common;

namespace MyRustInventory.Client
{
    public interface ISteamClient
    {
        public Task<RustItemsResponse> GetInventory(string steamId);
        public Task<string> GetInventoryItemImage(string imageId);
        public Task<MarketDataResponse> GetMarketData(string mhn, int currency = 1);
        public Task<MarketDataResponse> GetMarketData(string mhn,string assetId, int currency = 1);
    }
}
