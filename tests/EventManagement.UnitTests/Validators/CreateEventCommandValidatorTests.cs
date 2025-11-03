using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.Validators;
using FluentAssertions;

namespace EventManagement.UnitTests.Validators;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Valid Event Name",
            "Valid description",
            "Valid location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_InvalidName_ShouldFail(string name)
    {
        // Arrange
        var command = new CreateEventCommand(
            name,
            "Valid description",
            "Valid location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 201);
        var command = new CreateEventCommand(
            longName,
            "Valid description",
            "Valid location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(command.Name) && 
            e.ErrorMessage.Contains("200 characters"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_InvalidDescription_ShouldFail(string description)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Valid name",
            description,
            "Valid location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }

    [Fact]
    public void Validate_StartTimeInPast_ShouldFail()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Valid name",
            "Valid description",
            "Valid location",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(command.StartTime) && 
            e.ErrorMessage.Contains("future"));
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_ShouldFail()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(2);
        var endTime = DateTime.UtcNow.AddDays(1);

        var command = new CreateEventCommand(
            "Valid name",
            "Valid description",
            "Valid location",
            startTime,
            endTime);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("End time must be after start time"));
    }
}
