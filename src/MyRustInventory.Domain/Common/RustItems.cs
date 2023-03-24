namespace MyRustInventory.Domain.Common
{

    public class RustItems
    {
        public List<Asset>? Assets { get; set; }
        public List<Description>? Descriptions { get; set; }
        public int? Total_inventory_count { get; set; }
        public int? Success { get; set; }
        public int? Rwgrsn { get; set; }
    }
}
