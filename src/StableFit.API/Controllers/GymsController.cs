using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Gyms.Queries.GetNearbyGyms;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/gyms")]
[Authorize]
public sealed class GymsController : ControllerBase
{
    private readonly ISender _sender;

    public GymsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] int radius = 2000,
        CancellationToken ct = default)
    {
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            return BadRequest("Invalid coordinates.");

        if (radius is < 100 or > 10000)
            return BadRequest("Radius must be between 100 and 10000 metres.");

        var query = new GetNearbyGymsQuery
        {
            Latitude = lat,
            Longitude = lng,
            RadiusMeters = radius
        };

        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}
