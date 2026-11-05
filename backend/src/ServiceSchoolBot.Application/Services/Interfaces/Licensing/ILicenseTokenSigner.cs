using System.Text.Json;

namespace ServiceSchoolBot.Application.Services.Interfaces.Licensing;

public interface ILicenseTokenSigner
{
    string KeyId { get; }
    string Sign(string typ, object payload);
    bool Verify(string token, out JsonDocument? header, out JsonDocument? payload);
}
