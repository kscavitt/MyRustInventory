﻿using MediatR;
using MyRustInventory.Application.Common.Interfaces;
using MyRustInventory.Application.Steam.Queries.GetInventory;
using MyRustInventory.Domain.Common;

namespace MyRustInventory.Application.Steam.Handlers;

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, List<RustItemDto>>
{
    private readonly ISteamClient _steamClient;

    public GetInventoryQueryHandler(ISteamClient steamClient)
    {
        _steamClient = steamClient ?? throw new ArgumentNullException(nameof(steamClient));
    }

    public async Task<List<RustItemDto>> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        if(request.SteamId == null)
            // return an objec with an empty list.
            return  Enumerable.Empty<RustItemDto>().ToList();

        return await _steamClient.GetInventory(request.SteamId);
    }
}