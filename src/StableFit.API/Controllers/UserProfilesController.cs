using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using StableFit.Application.Commands.UserProfiles.CreateUserProfile;
using StableFit.Application.DTOs.UserProfiles;
using StableFit.Application.Queries.UserProfiles.GetUserProfileById;
using StableFit.Application.Queries.UserProfiles.GetAllUserProfiles;

namespace StableFit.API.Controllers;

[ApiController]
[Route("api/user-profiles")]
public class UserProfilesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IValidator<CreateUserProfileCommand> _validator;

    public UserProfilesController(ISender sender, IValidator<CreateUserProfileCommand> validator)
    {
        _sender = sender;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserProfile([FromBody] CreateUserProfileRequest request, CancellationToken ct)
    {
        var command = new CreateUserProfileCommand(request.Username, request.Name, request.Email);

        var validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        try
        {
            var dto = await _sender.Send(command, ct);
            return Created($"/api/user-profiles/{dto.Id}", dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
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
