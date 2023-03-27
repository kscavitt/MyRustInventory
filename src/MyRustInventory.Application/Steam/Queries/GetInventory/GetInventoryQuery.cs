using MediatR;
using MyRustInventory.Domain.Common;

namespace MyRustInventory.Application.Steam.Queries.GetInventory;
public record GetInventoryQuery() : IRequest<List<RustItemDto>>
{
    public string? SteamId { get; set; }
}
