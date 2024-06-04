using SeikenServer.Network.Utils;

namespace SeikenServer.Network.Models;

public readonly struct AuthenticateResult
{
    public readonly EConnectionResult ConnectionResult;
    public readonly string Message;

    public AuthenticateResult(EConnectionResult connectionResult, string message)
    {
        ConnectionResult = connectionResult;
        Message = message;
    }
}