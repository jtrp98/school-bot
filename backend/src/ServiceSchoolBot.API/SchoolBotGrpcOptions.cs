namespace ServiceSchoolBot.API;

public sealed class SchoolBotGrpcOptions
{
    public ISet<string> AllowedOrigins { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
