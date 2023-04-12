using NUnit.Framework;

namespace Tests;

public class FullNodeTests
{

    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task Simple_Test()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        TestHelper.RunNode(4300, cts.Token);

        await Task.Delay(TimeSpan.FromSeconds(20));

        TestHelper.RunNode(4301, cts.Token);

        await Task.Delay(TimeSpan.FromSeconds(20));

        cts.Cancel();

        await Task.WhenAll();
        cts.Cancel();
    }
}