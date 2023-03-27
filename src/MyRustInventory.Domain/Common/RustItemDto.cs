namespace MyRustInventory.Domain.Common
{ 
    public class RustItemDto
    {
        public string? Classid { get; set; }
        public string? BackgroundColor { get; set; }
        public string? IconUrl { get; set; }
        public string? IconUrlLarge { get; set; }
        public string? Description { get; set; }
        public int? Tradable { get; set; }
        public string? Name { get; set; }
        public string? NameColor { get; set; }
        public string? Type { get; set; }
        public string? MarketName { get; set; }
        public int? Commodity { get; set; }
        public int? MarketTradableRestriction { get; set; }
        public int? MarketMarketableRestriction { get; set; }
        public string? MarketHashName { get; set; }
        public int? Marketable { get; set; }
        public List<Tag>? Tags { get; set; }

        public string? Assetid { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? LowestPrice { get; set; }
    }
}
