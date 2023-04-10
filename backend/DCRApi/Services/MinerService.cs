using Microsoft.Extensions.Options;

namespace DCR;

public class MinerService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly AbstractNode _miner;
    

    public MinerService(ILogger<MinerService> logger, AbstractNode miner)
    {
        _logger = logger;
        _miner = miner;
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Node");
        await base.StartAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
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
        }
    }
}