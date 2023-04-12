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

var builder = WebApplication.CreateBuilder(appArgs);
var configuration = builder.Configuration;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSingleton<BlockHub>();
builder.Services.AddSingleton<NetworkClient>(networkClient);
if (type == "miner") {
    Miner node = new Miner(loggerFactory.CreateLogger<Miner>(), networkClient);
    builder.Services.AddSingleton<AbstractNode>(node);
    builder.Services.AddSingleton<Miner>(node);
    builder.Services.AddSingleton<MinerService>();
    builder.Services.AddHostedService<MinerService>(s => s.GetRequiredService<MinerService>());
} else {
    FullNode node = new FullNode(loggerFactory.CreateLogger<FullNode>(), networkClient);
    builder.Services.AddSingleton<AbstractNode>(node);
    builder.Services.AddSingleton<FullNode>(node);
}
var app = builder.Build();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<BlockHub>("/block-hub"); // add this line
});
if (type == "node") {
    app.UseCors(
        options => options.WithOrigins($"http://{address}:{frontendPort}").AllowAnyMethod().AllowAnyHeader()
    );
}
app.Run();