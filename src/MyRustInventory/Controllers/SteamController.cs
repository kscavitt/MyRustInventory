using Microsoft.AspNetCore.Mvc;
using MyRustInventory.Domain.Common;
using Microsoft.AspNetCore.Cors;
using MediatR;
using MyRustInventory.Application.Steam.Queries.GetInventory;
using MyRustInventory.Application.Steam.Queries.GetMarketData;

namespace MyRustInventory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SteamController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SteamController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [DisableCors]
        [HttpGet("GetInventory")]
        public async Task<RustItemsResponse> GetInventory(string steamId = "76561198012083287")
        {

            return await _mediator.Send(new GetInventoryQuery { SteamId = steamId });
        }

        [DisableCors]
        [HttpGet("GetMarketData/{mhn}/{currency}")]
        public async Task<MarketDataResponse> GetMarketData(string mhn, int currency = 1)
        {
            return await _mediator.Send( new GetMarketDataQuery { MarketHashName = mhn, Currency = currency });
        }

        [DisableCors]
        [HttpGet("GetMarketData/{mhn}/{assetId}/{currency}/")]
        public async Task<MarketDataResponse> GetMarketData(string mhn, string assetId, int currency = 1)
        {
            return await _mediator.Send(new GetMarketDataQuery { MarketHashName = mhn, Currency = currency, AssetId = assetId });
        }
    }
}
