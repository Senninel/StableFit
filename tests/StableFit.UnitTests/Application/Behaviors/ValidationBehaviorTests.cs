using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using StableFit.Application.Behaviors;
using MediatR;

namespace StableFit.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests : IDisposable
{
    // Minimal test request/response types used across all tests
    public sealed record TestRequest : IRequest<string>;

    private readonly RequestHandlerDelegate<string> _next;

    public ValidationBehaviorTests()
    {
        _next = Substitute.For<RequestHandlerDelegate<string>>();
        _next.Invoke().Returns(Task.FromResult("ok"));
    }

    public void Dispose() { /* nothing to clean up — NSubstitute is GC-friendly */ }

    // -------------------------------------------------------------------------
    // No validators registered
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_NoValidators_When_Handle_Then_CallsNextAndReturnsResult()
    {
        // Arrange
        var sut = new ValidationBehavior<TestRequest, string>([]);

        // Act
        var result = await sut.Handle(new TestRequest(), _next, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        await _next.Received(1).Invoke();
    }

    // -------------------------------------------------------------------------
    // Valid request
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_CallsNextAndReturnsResult()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult()); // no errors

        var sut = new ValidationBehavior<TestRequest, string>([validator]);

        // Act
        var result = await sut.Handle(new TestRequest(), _next, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        await _next.Received(1).Invoke();
    }

    // -------------------------------------------------------------------------
    // Invalid request — single validator
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_InvalidRequest_When_Handle_Then_ThrowsValidationException()
    {
        // Arrange
        var failure = new ValidationFailure("SomeProperty", "SomeProperty is required.");
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure]));

        var sut = new ValidationBehavior<TestRequest, string>([validator]);

        // Act
        var act = async () => await sut.Handle(new TestRequest(), _next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _next.DidNotReceive().Invoke();
    }

    // -------------------------------------------------------------------------
    // Multiple validators — errors are aggregated
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Given_MultipleValidatorsWithErrors_When_Handle_Then_AggregatesAllErrors()
    {
        // Arrange
        var failure1 = new ValidationFailure("Field1", "Field1 error.");
        var failure2 = new ValidationFailure("Field2", "Field2 error.");

        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure1]));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2
            .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure2]));

        var sut = new ValidationBehavior<TestRequest, string>([validator1, validator2]);

        // Act
        var act = async () => await sut.Handle(new TestRequest(), _next, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCount(2);
    }
}
