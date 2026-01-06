namespace Catan.Client.State;

public class ClientSession
{
    public string? Username { get; set; }
    public ClientUiState UiState { get; set; } = ClientUiState.Connecting;
}
