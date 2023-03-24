namespace MyRustInventory.Domain.Common
{
    public class MarketDataResponse
    {
        public string? AssetId { get; set; }
        public bool? Success { get; set; }
        public string? Lowest_Price { get; set; }
        public string? Volume { get; set; }
        public string? Median_Price { get; set; }
    }
}
