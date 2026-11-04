using System.Globalization;
using Grpc.Core;

namespace ServiceSchoolBot.API.GrpcServices;

// The caller as authenticated by the Gateway, the only authenticator. Trusted because the transport peer is.
// AdminId marks SchoolBright central staff (TAdmin); DeviceEntitlement is the desktop's license entitlement.
public sealed record GatewayIdentity(
    int SchoolId,
    int UserId,
    string? UserType,
    string? AdminId,
    bool IsSchoolAdmin,
    int? EmployeeId,
    string? DeviceEntitlement)
{
    public const string Student = "student";
    public const string Employee = "employee";

    // Employees are looked up by their employee id when the legacy web token supplied one.
    public int ProfileId => UserType == Employee && EmployeeId is { } employee ? employee : UserId;

    // Null when the call carries no identity (device licensing) or the identity is incomplete.
    public static GatewayIdentity? From(Metadata headers)
    {
        string? Value(string key) => headers.GetValue(key) is { Length: > 0 } value ? value : null;

        if (!int.TryParse(Value("x-school-id"), NumberStyles.None, CultureInfo.InvariantCulture, out var schoolId) ||
            schoolId <= 0 ||
            !int.TryParse(Value("x-user-id"), NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
            return null;

        var userType = Value("x-user-type");
        if (userType is not (null or Student or Employee)) return null;
        int? employeeId = int.TryParse(Value("x-employee-id"), NumberStyles.None, CultureInfo.InvariantCulture,
            out var parsed) && parsed > 0 ? parsed : null;
        return new GatewayIdentity(schoolId, userId, userType, Value("x-admin-id"), Value("x-is-admin") == "true",
            employeeId, Value("x-device-entitlement"));
    }
}
