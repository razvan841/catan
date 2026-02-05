using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Catan.Client.Networking;
using Catan.Client.State;
using Catan.Client.Commands;
using Catan.Shared.Enums;
using Catan.Shared.Networking.Messages;

namespace Catan.Client.UI;

public partial class MainWindow : Window
{
    private TcpClientConnection? _connection;
    private readonly MatchmakingClient _matchmakingClient = new();
    private MatchFoundWindow? _matchDialog;
    private readonly ClientSession _session = new();
    private readonly ClientSender _sender = new();
    private readonly ServerMessageRouter _router = new();
    private CommandDispatcher _commandDispatcher;

    public MainWindow()
    {
        InitializeComponent();
        InitializeProfileView();

        _commandDispatcher = new CommandDispatcher(new ICommandHandler[]
        {
            new HelpCommand(this),
            new ClearCommand(this),
            new PingCommand(_sender),
            new QueueCommand(_sender, _session, this),
            new DequeueCommand(_sender, _session, this),
            new EloCommand(_sender, _session),
            new LeaderboardCommand(_sender),
            new PlayerInfoCommand(_sender, this),
            new WhisperCommand(_sender, this),
            new FriendRequestCommand(_sender, this),
            new GroupMessageCommand(_sender, this),
            new CreateGameCommand(_sender, this),
            new AcceptFriendCommand(_sender, this, _session),
            new RejectFriendCommand(_sender, this, _session),
            new BlockUserCommand(_sender, this),
            new UnblockUserCommand(_sender, this),
            new UnfriendCommand(_sender, this),
            new GetFriendsCommand(_sender, this),
        });

        new ServerMessageHandlers(this).Register(_router);

        UpdateUi();
        _ = TryConnectAsync();
    }


    private async Task TryConnectAsync()
    {
        try
        {
            _connection = new TcpClientConnection("127.0.0.1", 5000);
            _sender.Attach(_connection);
            _matchmakingClient.Attach(_connection);
            _session.UiState = ClientUiState.Auth;

            _ = Task.Run(ListenAsync);
            Log("Connected to server.");
        }
        catch
        {
            _session.UiState = ClientUiState.Disconnected;
            Log("Failed to connect to server.");
        }

        UpdateUi();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (_connection != null)
            {
                var msg = await _connection.ReceiveAsync();
                Dispatcher.UIThread.Post(() => _router.Route(msg));
            }
        }
        catch
        {
            Dispatcher.UIThread.Post(() =>
            {
                _session.UiState = ClientUiState.Disconnected;
                UpdateUi();
                Log("Disconnected from server.");
            });
        }
    }

    private void InitializeProfileView()
    {
        ProfileView.BackRequested += () =>
        {
            _session.UiState = ClientUiState.InLobby;
            UpdateUi();
        };
    }

    private async Task RequestProfileDataAsync()
    {
        if(_connection == null)
            return;
        await _connection.SendAsync(new ClientMessage
        {
            Type = MessageType.ProfileRequest
        });
    }


}
