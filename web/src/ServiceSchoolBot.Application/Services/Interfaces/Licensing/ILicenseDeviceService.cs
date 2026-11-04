using ServiceSchoolBot.Application.Common.Model.Licensing;
namespace ServiceSchoolBot.Application.Services.Interfaces.Licensing;

public interface ILicenseDeviceService
{
    Task<DeviceChallengeResponse> CreateChallengeAsync(CreateChallengeRequest request, CancellationToken ct = default);
    Task<EntitlementResult> ActivateAsync(ActivateLicenseRequest request, string rawJsonBody, string? idempotencyKey = null, CancellationToken ct = default);
    Task<EntitlementResult> ValidateAsync(ValidateLicenseRequest request, string rawJsonBody, CancellationToken ct = default);
}
