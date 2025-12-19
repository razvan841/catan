using Catan.Server.Networking.Tcp;

var server = new TcpServer(port: 5000);
await server.StartAsync();
