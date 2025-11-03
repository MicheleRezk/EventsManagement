using EventManagement.Application.Features.Events.Commands;
using EventManagement.Application.Features.Events.Validators;
using FluentAssertions;

namespace EventManagement.UnitTests.Validators;

public class RegisterForEventCommandValidatorTests
{
    private readonly RegisterForEventCommandValidator _validator;

    public RegisterForEventCommandValidatorTests()
    {
        _validator = new RegisterForEventCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterForEventCommand(
            Guid.NewGuid(),
            "Michele George",
            "+1234567890",
            "michele.george@example.com");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyEventId_ShouldFail()
    {
        // Arrange
        var command = new RegisterForEventCommand(
            Guid.Empty,
            "Michele George",
            "+1234567890",
            "michele.george@example.com");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.EventId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_InvalidName_ShouldFail(string name)
    {
        // Arrange
        var command = new RegisterForEventCommand(
            Guid.NewGuid(),
            name,
            "+1234567890",
            "michele.george@example.com");

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
        var longName = new string('a', 151);
        var command = new RegisterForEventCommand(
            Guid.NewGuid(),
            longName,
            "+1234567890",
            "michele.george@example.com");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(command.Name) && 
            e.ErrorMessage.Contains("150 characters"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_InvalidPhoneNumber_ShouldFail(string phoneNumber)
    {
        // Arrange
        var command = new RegisterForEventCommand(
            Guid.NewGuid(),
            "Michele George",
            phoneNumber,
            "michele.george@example.com");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PhoneNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterForEventCommand(
            Guid.NewGuid(),
            "Michele George",
            "+1234567890",
            email);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }
}
