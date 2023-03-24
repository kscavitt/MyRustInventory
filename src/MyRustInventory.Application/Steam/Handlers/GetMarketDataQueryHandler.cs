using MediatR;
using MyRustInventory.Application.Common.Interfaces;
using MyRustInventory.Application.Steam.Queries.GetMarketData;

using MyRustInventory.Domain.Common;

namespace MyRustInventory.Application.Steam.Handlers;

public class GetMarketDataQueryHandler : IRequestHandler<GetMarketDataQuery, MarketDataResponse>
{
    private readonly ISteamClient _steamClient;
    public GetMarketDataQueryHandler(ISteamClient steamClient)
    {
        _steamClient = steamClient;
    }

    public async Task<MarketDataResponse> Handle(GetMarketDataQuery request, CancellationToken cancellationToken)
    {
        if(request.MarketHashName == null)
            return new MarketDataResponse();

        return await _steamClient.GetMarketData(request.MarketHashName, request.AssetId ?? "0", request.Currency ?? 1); 
    }

}
