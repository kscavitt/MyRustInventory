namespace MyRustInventory.Domain.Common
{ 
    public class Description
    {
        public int? Appid { get; set; }
        public string? Classid { get; set; }
        public string? Instanceid { get; set; }
        public int? Currency { get; set; }
        public string? Background_Color { get; set; }
        public string? Icon_Url { get; set; }
        public string? Icon_Url_Large { get; set; }
        public List<Description>? Descriptions { get; set; }
        public int? Tradable { get; set; }
        public string? Name { get; set; }
        public string? Name_Color { get; set; }
        public string? Type { get; set; }
        public string? Market_Name { get; set; }
        public string? Market_Hash_Name { get; set; }
        public int? Commodity { get; set; }
        public int? Market_Tradable_Restriction { get; set; }
        public int? Market_Marketable_Restriction { get; set; }
        public int? Marketable { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<Action>? Actions { get; set; }
        public List<OwnerDescription>? Owner_Descriptions { get; set; }
        public string? Value { get; set; }
    }
}
