using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace RestStop.Api.Infrastructure.Vault;

public class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly string _vaultUrl;
    private readonly string _mountPoint;
    private readonly string _token;

    public VaultConfigurationProvider(string vaultUrl, string mountPoint, string token)
    {
        _vaultUrl = vaultUrl;
        _mountPoint = mountPoint;
        _token = token;
    }

    public override void Load() => LoadAsync().GetAwaiter().GetResult();

    private async Task LoadAsync()
    {
        try
        {
            var client = new VaultClient(new VaultClientSettings(_vaultUrl, new TokenAuthMethodInfo(_token)));

            var pg = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("postgreSQL", mountPoint: _mountPoint);
            var pgData = pg.Data.Data.ToDictionary(k => k.Key.Trim(), v => v.Value);
            Data["ConnectionStrings:Default"] = pgData["connectionString"].ToString()!;

            var jwt = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("jwt", mountPoint: _mountPoint);
            var jwtData = jwt.Data.Data.ToDictionary(k => k.Key.Trim(), v => v.Value);
            Data["Jwt:Secret"] = jwtData["key"].ToString()!;

            var aws = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("aws", mountPoint: _mountPoint);
            var awsData = aws.Data.Data.ToDictionary(k => k.Key.Trim(), v => v.Value);
            Data["Aws:SesAccessKeyId"]     = awsData["sesAccessKey"].ToString()!;
            Data["Aws:SesSecretAccessKey"] = awsData["sesSecretKey"].ToString()!;
            Data["Aws:SnsAccessKeyId"]     = awsData["snsAccessKey"].ToString()!;
            Data["Aws:SnsSecretAccessKey"] = awsData["snsSecretKey"].ToString()!;
            Data["Aws:Region"]             = awsData["region"].ToString()!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Vault] Failed to load configuration from {_vaultUrl}: {ex.Message}");
            throw;
        }
    }
}
