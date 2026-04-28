using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.UserProfiles.Commands.CreateUserProfile;
using StableFit.Application.UserProfiles.Commands.UpdateMyUserProfile;
using StableFit.Application.UserProfiles.DTOs;
using StableFit.Application.UserProfiles.Queries.GetAllUserProfiles;
using StableFit.Application.UserProfiles.Queries.GetMyUserProfile;
using StableFit.Application.UserProfiles.Queries.GetUserProfileById;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/user-profiles")]
public sealed class UserProfilesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly StableFit.Application.Interfaces.ICurrentUserService _currentUser;

    public UserProfilesController(ISender sender, StableFit.Application.Interfaces.ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUserProfile([FromBody] CreateUserProfileRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId 
            ?? throw new StableFit.Application.Exceptions.UnauthorizedException();

        var command = new CreateUserProfileCommand(userId, request.Username, request.Name, request.Email);
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

    /// <summary>Returns the authenticated user's own profile.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var dto = await _sender.Send(new GetMyUserProfileQuery(), ct);
        return Ok(dto);
    }

    /// <summary>Updates the authenticated user's own profile. Returns 204 on success.</summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyUserProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateMyUserProfileCommand(
            request.Bio, request.Goal, request.ScheduleDays, request.AgeYears, request.WeightKg);

        await _sender.Send(command, ct);
        return NoContent();
    }
}
