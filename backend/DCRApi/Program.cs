using System.Collections.Generic;
using DCR;

var address = "localhost";
var frontendPort = 8080;
var backendPort = 4300;
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var type = "node";

if (args.Length == 1) {
    backendPort = Convert.ToInt32(args[0]);
    type = "miner";
} else {
    if (args.Length == 2)
    {
        frontendPort = Convert.ToInt32(args[0]);
        backendPort = Convert.ToInt32(args[1]);
    }
}

var networkClient = new NetworkClient(address, backendPort);
await networkClient.DiscoverNetwork();
string[] appArgs = {$"--urls={networkClient.ClientNode.URL}"};
Settings settings = new Settings()
    {TimeToSleep = 10000, SizeOfBlocks = 10, NumberNeighbours = 5, 
     Difficulty = 3, IsEval = false};
var builder = WebApplication.CreateBuilder(appArgs);
var configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<PendingTransactionsHub>();
builder.Services.AddSingleton<NetworkClient>(networkClient);
if (type == "miner") {
    Miner node = new Miner(loggerFactory.CreateLogger<Miner>(), networkClient, settings);
    builder.Services.AddSingleton<AbstractNode>(node);
    builder.Services.AddSingleton<Miner>(node);
    builder.Services.AddSingleton<MinerService>();
    builder.Services.AddHostedService<MinerService>(s => s.GetRequiredService<MinerService>());
} else {
    FullNode node = new FullNode(loggerFactory.CreateLogger<FullNode>(), networkClient, settings);
    builder.Services.AddSingleton<AbstractNode>(node);
    builder.Services.AddSingleton<FullNode>(node);
}
var app = builder.Build();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PendingTransactionsHub>("/pending-transactions-hub");

if (type == "node") {
    app.UseCors(
        options => options.WithOrigins($"http://{address}:{frontendPort}").AllowAnyMethod().AllowAnyHeader()
    );
}
app.Run();