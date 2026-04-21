using MediatR;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.UserProfiles.Commands.CreateUserProfile;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Application.UserProfiles.Queries.GetUserProfileById;
using StableFit.Application.UserProfiles.Queries.GetAllUserProfiles;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/user-profiles")]
public class UserProfilesController : ControllerBase
{
    private readonly ISender _sender;
    public UserProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserProfile([FromBody] CreateUserProfileRequest request, CancellationToken ct)
    {
        var command = new CreateUserProfileCommand(request.Username, request.Name, request.Email);
        var dto = await _sender.Send(command, ct);
        return Created($"/api/user-profiles/{dto.Id}", dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserProfileById(Guid id, CancellationToken ct)
    {
        var dto = await _sender.Send(new GetUserProfileByIdQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserProfiles(CancellationToken ct)
    {
        var dtos = await _sender.Send(new GetAllUserProfilesQuery(), ct);
        return Ok(dtos);
    }
}
