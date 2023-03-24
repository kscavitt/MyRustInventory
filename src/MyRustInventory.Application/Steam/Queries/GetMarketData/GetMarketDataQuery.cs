using MediatR;
using MyRustInventory.Domain.Common;


namespace MyRustInventory.Application.Steam.Queries.GetMarketData;

public record GetMarketDataQuery() : IRequest<MarketDataResponse>
{
    public string? MarketHashName { get; set; }
    public int? Currency { get; set; }
    public string? AssetId { get; set; }
}

