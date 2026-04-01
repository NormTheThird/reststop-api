namespace RestStop.Api.Infrastructure.Vault;

public static class VaultConfigurationExtensions
{
    public static void AddVaultConfiguration(this WebApplicationBuilder builder)
    {
        var vaultUrl = builder.Configuration["Vault:Url"]
            ?? "http://vault.local:30420";

        var mountPoint = builder.Configuration["Vault:MountPoint"]
            ?? "reststop";

        var token = Environment.GetEnvironmentVariable("VAULT_TOKEN")
            ?? throw new InvalidOperationException(
                "VAULT_TOKEN environment variable is not set. " +
                "Set it in your shell before running the application.");

        ((IConfigurationBuilder)builder.Configuration).Add(new VaultConfigurationSource(vaultUrl, mountPoint, token));
    }
}

file sealed class VaultConfigurationSource : IConfigurationSource
{
    private readonly string _vaultUrl;
    private readonly string _mountPoint;
    private readonly string _token;

    public VaultConfigurationSource(string vaultUrl, string mountPoint, string token)
    {
        _vaultUrl = vaultUrl;
        _mountPoint = mountPoint;
        _token = token;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new VaultConfigurationProvider(_vaultUrl, _mountPoint, _token);
}
