using DCR;

namespace Tests;

public static class TestHelper
{
    // private static string _address = "192.168.0.132";
    public static void RunNode(int port, CancellationToken token)
    {
        string[] args = { $"{port}" };
        var minerTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Program.Main(args);

                // Delay before retrying
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }, token);

    }
}
