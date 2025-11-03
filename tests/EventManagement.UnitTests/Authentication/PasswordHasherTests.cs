using EventManagement.Infrastructure.Services;
using FluentAssertions;

namespace EventManagement.UnitTests.Authentication;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ValidPassword_ShouldReturnHashedValue()
    {
        // Arrange
        const string password = "testPassword123";

        // Act
        var hashedPassword = _passwordHasher.Hash(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "testPassword123";
        var hashedPassword = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string originalPassword = "testPassword123";
        const string wrongPassword = "wrongPassword123";
        var hashedPassword = _passwordHasher.Hash(originalPassword);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }
}
