namespace TaskManagement.API.Extensions;

public static class HostingConfigurationExtensions
{
    private const string JwtConfigurationKey = "Jwt:SecretKey";
    private const string JwtEnvironmentVariable = "Jwt__SecretKey";
    private static readonly string[] PlaceholderMarkers =
    {
        "CHANGE_ME", "PLACEHOLDER", "YOUR_", "REPLACE_ME", "SET_IN_ENVIRONMENT"
    };

    public static void ValidateEnvironmentConfiguration(IConfiguration configuration, IHostEnvironment environment)
        => ValidateEnvironmentConfiguration(configuration, environment, Environment.GetEnvironmentVariable);

    public static void ValidateEnvironmentConfiguration(
        IConfiguration configuration,
        IHostEnvironment environment,
        Func<string, string?> externalSecretReader)
    {
        RequireJwtSecret(configuration, environment, externalSecretReader);
    }

    private static void RequireJwtSecret(
        IConfiguration configuration,
        IHostEnvironment environment,
        Func<string, string?> externalSecretReader)
    {
        var configuredValue = configuration[JwtConfigurationKey]?.Trim();

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            if (IsInvalidSecret(configuredValue, 32))
                throw JwtConfigurationException(environment);
            return;
        }

        var externalValue = externalSecretReader(JwtEnvironmentVariable)?.Trim();
        if (IsInvalidSecret(externalValue, 32) ||
            !string.Equals(configuredValue, externalValue, StringComparison.Ordinal))
        {
            throw JwtConfigurationException(environment);
        }
    }

    private static bool IsInvalidSecret(string? value, int minimumLength) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Length < minimumLength ||
        PlaceholderMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static InvalidOperationException JwtConfigurationException(IHostEnvironment environment) =>
        new($"Jwt:SecretKey must be supplied through external configuration and meet security requirements for {environment.EnvironmentName}.");
}
