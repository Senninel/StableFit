using MediatR;
using StableFit.Application.Matching.DTOs;

namespace StableFit.Application.Matching.Queries.GetMyActiveMatch;

public record GetMyActiveMatchQuery : IRequest<ActiveMatchDto?>;
