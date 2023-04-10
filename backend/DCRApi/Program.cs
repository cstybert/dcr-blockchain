using DCR;
using Microsoft.Extensions.Options; // Import the namespace for IOptions

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
string[] appArgs = {$"--urls={networkClient.ClientNode.URL}"};

var builder = WebApplication.CreateBuilder(appArgs);
var configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<NetworkClient>(networkClient);
AbstractNode node;
if (type == "miner") {
    node = new Miner(loggerFactory.CreateLogger<Miner>(), Options.Create(new BlockchainSettings()), networkClient);
    builder.Services.Configure<BlockchainSettings>(configuration.GetSection(nameof(BlockchainSettings)));
    builder.Services.AddSingleton<MinerService>();
    builder.Services.AddHostedService<MinerService>(s => s.GetRequiredService<MinerService>());
    builder.Services.AddSingleton<Miner>();
} else {
    node = new FullNode(loggerFactory.CreateLogger<FullNode>(), Options.Create(new BlockchainSettings()), networkClient);
}
builder.Services.AddSingleton<AbstractNode>(node);

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
if (type == "node") {
    app.UseCors(
        options => options.WithOrigins($"http://{address}:{frontendPort}").AllowAnyMethod().AllowAnyHeader()
    );
}
app.Run();
