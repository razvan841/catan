using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DockerServerFixture : IAsyncLifetime
{
    private const string ServerUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        RunDockerCompose("up -d --build");
        await WaitForServerAsync();
    }

    public Task DisposeAsync()
    {
        RunDockerCompose("down");
        return Task.CompletedTask;
    }

    private static void RunDockerCompose(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose {arguments}",
                WorkingDirectory = GetRepoRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception("Docker Compose failed to run.");
        }
    }

    private static async Task WaitForServerAsync()
    {
        using var client = new HttpClient();
        var timeout = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < timeout)
        {
            try
            {
                var response = await client.GetAsync($"{ServerUrl}/health");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Server not ready yet
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Server did not become healthy in time.");
    }

    private static string GetRepoRoot()
    {
        // tests/bin/Debug/netX.Y → go up 4 levels
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
    }
}
