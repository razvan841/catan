namespace Catan.Server.Sessions;

public enum SessionState
{
    Connected,
    Registered,
    InQueue,
    InMatch,
    Disconnected
}
