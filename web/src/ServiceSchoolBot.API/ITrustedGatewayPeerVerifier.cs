using Grpc.Core;

namespace ServiceSchoolBot.API;

public interface ITrustedGatewayPeerVerifier
{
    bool IsTrusted(ServerCallContext context);
}

public sealed class DenyAllGatewayPeerVerifier : ITrustedGatewayPeerVerifier
{
    public bool IsTrusted(ServerCallContext context) => false;
}
