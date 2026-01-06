using Catan.Shared.Networking.Dtos.Server;

namespace Catan.Client.UI;

public partial class MainWindow
{
    public void OnLoginResponse(LoginResponseDto dto)
    {
        if (dto.Success)
        {
            _session.UiState = ClientUiState.InLobby;
            UpdateUi();
            Log("Login successful.");
        }
        else
        {
            ShowErrorPopup("Login Failed", dto.Message);
        }
    }

    public void OnRegisterResponse(RegisterResponseDto dto)
    {
        if (dto.Success)
        {
            _session.UiState = ClientUiState.InLobby;
            UpdateUi();
            Log("Registration successful.");
        }
        else
        {
            ShowErrorPopup("Registration Failed", dto.Message);
        }
    }
}
