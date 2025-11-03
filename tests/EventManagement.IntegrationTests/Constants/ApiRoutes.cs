namespace EventManagement.IntegrationTests.Constants;

public static class ApiRoutes
{
    private const string RootApi = "/api";

    public static class Auth
    {
        public const string Register = $"{RootApi}/auth/register";
        public const string Login = $"{RootApi}/auth/login";
    }

    public static class Events
    {
        private const string Base = $"{RootApi}/events";

        public const string GetAll = Base;
        public const string Create = Base;
        public const string Creator = $"{Base}/creator";

        public static string GetById(Guid eventId) => $"{Base}/{eventId}";
        public static string Register(Guid eventId) => $"{Base}/{eventId}/register";
        public static string GetRegistrations(Guid eventId) => $"{Base}/{eventId}/registrations";
    }
}
