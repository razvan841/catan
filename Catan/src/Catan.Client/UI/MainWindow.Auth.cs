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
            _session.UiState = ClientUiState.Auth;
            UpdateUi();

            Log($"Login failed: {dto.Message}");
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
            _session.UiState = ClientUiState.Auth;
            UpdateUi();

            Log($"Registration failed: {dto.Message}");
            ShowErrorPopup("Registration Failed", dto.Message);
        }
    }
}
