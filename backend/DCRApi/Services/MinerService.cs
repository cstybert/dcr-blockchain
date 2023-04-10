namespace DCR;

public class MinerService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly Miner _miner;

    public MinerService(ILogger<MinerService> logger, Miner miner)
    {
        _logger = logger;
        _miner = miner;
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _miner.NetworkClient.DiscoverNetwork();
        // ResyncBlockchain();
        // GoOnline();
        _logger.LogInformation("Starting Node");
        await base.StartAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _miner.Blockchain.Save();
        await _miner.NetworkClient.DisconnectFromNetwork();
        _logger.LogInformation($"Stopping Node");
        await base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Task task = new Task(new System.Action(_miner.Mine));
            task.Start();
            await task;
            Thread.Sleep(_miner.Settings.TimeToSleep); // For testing, a block is added every 15 seconds
        }
    }
}