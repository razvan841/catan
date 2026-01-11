using System;
using System.Reactive;
using Avalonia.Threading;
using ReactiveUI;

namespace Catan.Client.UI;

public class MatchFoundViewModel : ReactiveObject
{
    private int _secondsLeft = 10;
    private readonly DispatcherTimer _timer;

    public string CountdownText => $"Accept within {_secondsLeft} seconds";

    public ReactiveCommand<Unit, Unit> AcceptCommand { get; }
    public ReactiveCommand<Unit, Unit> DeclineCommand { get; }

    public MatchFoundViewModel()
    {
        AcceptCommand = ReactiveCommand.Create(Stop);
        DeclineCommand = ReactiveCommand.Create(Stop);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _timer.Tick += (_, _) =>
        {
            _secondsLeft--;
            this.RaisePropertyChanged(nameof(CountdownText));

            if (_secondsLeft <= 0)
            {
                _timer.Stop();
                DeclineCommand.Execute().Subscribe();
            }
        };

        _timer.Start();
    }

    private void Stop() => _timer.Stop();
}
