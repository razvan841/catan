using System.Threading.Tasks;

namespace Catan.Client.Commands;

public interface ICommandHandler
{
    string[] Aliases { get; }
    Task Execute(string[] args);
}
