namespace MyRustInventory.Domain.Common
{
    public class Asset
    {
        public int Appid { get; set; }
        public string? Contextid { get; set; }
        public string? Assetid { get; set; }
        public string? Classid { get; set; }
        public string? Instanceid { get; set; }
        public string? Amount { get; set; }
    }
}
