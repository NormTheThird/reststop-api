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

            var pg = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("postgresql", mountPoint: _mountPoint);
            Data["ConnectionStrings:Default"] = pg.Data.Data["connectionString"].ToString()!;

            var jwt = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("jwt", mountPoint: _mountPoint);
            Data["Jwt:Secret"] = jwt.Data.Data["key"].ToString()!;

            var aws = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync("aws", mountPoint: _mountPoint);
            Data["Aws:SesAccessKeyId"]     = aws.Data.Data["sesAccessKey"].ToString()!;
            Data["Aws:SesSecretAccessKey"] = aws.Data.Data["sesSecretKey"].ToString()!;
            Data["Aws:SnsAccessKeyId"]     = aws.Data.Data["snsAccessKey"].ToString()!;
            Data["Aws:SnsSecretAccessKey"] = aws.Data.Data["snsSecretKey"].ToString()!;
            Data["Aws:Region"]             = aws.Data.Data["region"].ToString()!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Vault] Failed to load configuration from {_vaultUrl}: {ex.Message}");
            throw;
        }
    }
}
