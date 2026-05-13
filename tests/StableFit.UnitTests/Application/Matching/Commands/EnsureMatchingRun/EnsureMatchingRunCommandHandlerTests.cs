using FluentAssertions;
using NSubstitute;
using StableFit.Application.Matching.Commands.EnsureMatchingRun;
using StableFit.Application.Matching.Interfaces;

namespace StableFit.UnitTests.Application.Matching.Commands.EnsureMatchingRun;

public class EnsureMatchingRunCommandHandlerTests
{
    private readonly IMatchingRunBuilder _builderMock;
    private readonly EnsureMatchingRunCommandHandler _handler;

    public EnsureMatchingRunCommandHandlerTests()
    {
        _builderMock = Substitute.For<IMatchingRunBuilder>();
        _handler = new EnsureMatchingRunCommandHandler(_builderMock);
    }

    [Fact]
    public async Task Handle_AlwaysDelegate_ToBuilder()
    {
        // Arrange
        var expectedRunId = Guid.NewGuid();
        _builderMock.EnsureRunAndMaterializeRecommendationsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedRunId);

        var command = new EnsureMatchingRunCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedRunId);
        await _builderMock.Received(1).EnsureRunAndMaterializeRecommendationsAsync(CancellationToken.None);
    }
}
