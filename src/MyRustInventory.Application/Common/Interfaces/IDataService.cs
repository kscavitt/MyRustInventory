using MyRustInventory.Core.Models;

namespace MyRustInventory.Core.Interfaces
{
    internal interface IDataService
    {
        string? Get(string key);
        void Upsert(string key, List<RustItemDto> InventoryItems);
        void Upsert(string key, List<RustItemDto> InventoryItems, TimeSpan expiration);
        void Remove(string key);    
    }
}
