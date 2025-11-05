using System.Net;
using System.Text;
using System.Text.Json;
using EventManagement.Application.Features.Events.DTOs;
using EventManagement.IntegrationTests.Constants;
using EventManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace EventManagement.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class EventsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EventsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GivenEventsExist_WhenGettingAllEvents_ThenShouldReturnEventsList()
    {
        // Arrange
        await TestUtility.CreateAuthenticatedUserAsync(_client, "creator1@example.com", "Creator1", "password123");

        var createEventDto = new CreateEventDto(
            "Public Event",
            "Public event description",
            "Public location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        await _client.PostAsync(ApiRoutes.Events.Create, content);
        
        _client.ClearAuthorizationHeader();

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.GetAll);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<IEnumerable<EventDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        events.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GivenAuthenticatedUser_WhenCreatingEvent_ThenShouldCreateEventSuccessfully()
    {
        // Arrange
        await TestUtility.CreateAuthenticatedUserAsync(_client, "creator2@example.com", "Event Creator", "password123");

        var createEventDto = new CreateEventDto(
            "Test Conference",
            "Annual tech conference",
            "Berlin, Germany",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Events.Create, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.Should().NotBeNull();
        result!.Name.Should().Be(createEventDto.Name);
        result.Description.Should().Be(createEventDto.Description);
        result.Location.Should().Be(createEventDto.Location);;
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GivenUnauthenticatedUser_WhenCreatingEvent_ThenShouldReturnUnauthorized()
    {
        // Arrange
        var createEventDto = new CreateEventDto(
            "Unauthorized Event",
            "This should fail",
            "Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Events.Create, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GivenAuthenticatedUserWithEvents_WhenGettingCreatedEvents_ThenShouldReturnUserEvents()
    {
        // Arrange
        await TestUtility.CreateAuthenticatedUserAsync(_client, "eventcreator@example.com", "Event Creator", "password123");

        // Create an event
        var createEventDto = new CreateEventDto(
            "My Event",
            "Event created by me",
            "My Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        await _client.PostAsync(ApiRoutes.Events.Create, content);

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.Creator);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var events = JsonSerializer.Deserialize<IEnumerable<EventDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        events.Should().NotBeNull();
        events!.Should().HaveCount(1);
        var eventDto = events.First();
        eventDto.Name.Should().Be(createEventDto.Name);
        eventDto.Description.Should().Be(createEventDto.Description);
        eventDto.Location.Should().Be(createEventDto.Location);
        eventDto.StartTime.Should().BeCloseTo(createEventDto.StartTime, TimeSpan.FromMilliseconds(1));
        eventDto.EndTime.Should().BeCloseTo(createEventDto.EndTime, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task GivenExistingEvent_WhenRegisteringWithValidData_ThenShouldCreateRegistration()
    {
        // Arrange
        await TestUtility.CreateAuthenticatedUserAsync(_client, "creator4@example.com", "Creator 4", "password123");

        var createEventDto = new CreateEventDto(
            "Registration Event",
            "Event for registration testing",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var eventContent = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        var eventResponse = await _client.PostAsync(ApiRoutes.Events.Create, eventContent);
        var eventResponseContent = await eventResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(eventResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        _client.ClearAuthorizationHeader();

        // Register for event
        var registerDto = new RegisterForEventDto("Michele George", "+1234567890", "michele.george@gmail.com");
        var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(ApiRoutes.Events.Register(createdEvent!.Id), registerContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RegistrationDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        result.Should().NotBeNull();
        result!.Name.Should().Be(registerDto.Name);;
        result.Email.Should().Be(registerDto.Email);
        result.EventId.Should().Be(createdEvent.Id);
    }

    [Fact]
    public async Task GivenEventCreator_WhenGettingEventRegistrations_ThenShouldReturnRegistrations()
    {
        // Arrange - Create event and registration
        var token = await TestUtility.CreateAuthenticatedUserAsync(_client, "creator5@example.com", "Creator 5", "password123");

        // Create event
        var createEventDto = new CreateEventDto(
            "Event with Registrations",
            "Event description",
            "Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var eventContent = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        var eventResponse = await _client.PostAsync(ApiRoutes.Events.Create, eventContent);
        var eventResponseContent = await eventResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(eventResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Clear auth and register using particpant user
        _client.ClearAuthorizationHeader();
        
        var registerDto = new RegisterForEventDto("Michele George", "+0987654321", "michele.george@example.com");
        var registerContent = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        await _client.PostAsync(ApiRoutes.Events.Register(createdEvent!.Id), registerContent);

        // Re-authenticate as creator
        _client.AddJwt(token);

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.GetRegistrations(createdEvent.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var registrations = JsonSerializer.Deserialize<IEnumerable<RegistrationDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        registrations.Should().NotBeNull();
        registrations!.Should().HaveCount(1);
        registrations!.First().Name.Should().Be(registerDto.Name);
    }

    [Fact]
    public async Task GivenExistingEventId_WhenGettingEventById_ThenShouldReturnEventDto()
    {
        // Arrange - Create an event first
        await TestUtility.CreateAuthenticatedUserAsync(_client, "creator7@example.com", "Creator 7", "password123");

        var createEventDto = new CreateEventDto(
            "Tech Meetup",
            "Monthly tech meetup for developers",
            "Tech Hub Berlin",
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(3));

        var eventContent = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync(ApiRoutes.Events.Create, eventContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(createResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        _client.ClearAuthorizationHeader();

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.GetById(createdEvent!.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var eventDto = JsonSerializer.Deserialize<EventDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        eventDto.Should().NotBeNull();
        eventDto!.Id.Should().Be(createdEvent.Id);
        eventDto.Name.Should().Be(createEventDto.Name);
        eventDto.Description.Should().Be(createEventDto.Description);
        eventDto.Location.Should().Be(createEventDto.Location);
        eventDto.StartTime.Should().BeCloseTo(createEventDto.StartTime, TimeSpan.FromMilliseconds(1));
        eventDto.EndTime.Should().BeCloseTo(createEventDto.EndTime, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task GivenNonExistingEventId_WhenGettingEventById_ThenShouldReturnNotFound()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.GetById(nonExistingEventId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenNonEventCreator_WhenGettingEventRegistrations_ThenShouldReturnUnauthorized()
    {
        //Arrange
        await TestUtility.CreateAuthenticatedUserAsync(_client, "creator9@example.com", "Creator 9", "password123");

        var createEventDto = new CreateEventDto(
            "Private Event",
            "Event description",
            "Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2));

        var eventContent = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        var eventResponse = await _client.PostAsync(ApiRoutes.Events.Create, eventContent);
        var eventResponseContent = await eventResponse.Content.ReadAsStringAsync();
        var createdEvent = JsonSerializer.Deserialize<EventDto>(eventResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Authenticate as a different user
        _client.ClearAuthorizationHeader();
        await TestUtility.CreateAuthenticatedUserAsync(_client, "other@example.com", "Other User", "password123");

        // Act
        var response = await _client.GetAsync(ApiRoutes.Events.GetRegistrations(createdEvent!.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
