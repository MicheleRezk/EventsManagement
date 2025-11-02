using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Add PostgreSQL database
var postgresServer = builder
    .AddPostgres("postgreSQLServer")
    .WithDataVolume()
    .WithPgAdmin();

var eventsDb = postgresServer.AddDatabase("eventsdb");

// Add API project
var api = builder.AddProject<Projects.EventManagement_API>("event-management-api")
    .WaitFor(postgresServer)
    .WithReference(eventsDb)
    .WithEnvironment("Security__JwtKey", builder.Configuration["Security:JwtKey"])
    .WithEnvironment("Security__JwtIssuer", builder.Configuration["Security:JwtIssuer"])
    .WithEnvironment("Security__JwtAudience", builder.Configuration["Security:JwtAudience"])
    ;

builder.Build().Run();