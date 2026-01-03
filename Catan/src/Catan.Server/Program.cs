using System;
using System.Threading.Tasks;
using Catan.Server;
using Catan.Server.Networking.Tcp;

class Program
{
    static async Task Main()
    {
        // Db.Initialize();

        Console.WriteLine("Database initialized! Starting TCP server...");

        var server = new TcpServer(5000);
        await server.StartAsync();
    }
}
