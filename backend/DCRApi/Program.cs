using DCR;

var address = "localhost";
var frontendPort = 8080;
var backendPort = 4300;
if (args.Length == 2) {
    frontendPort = Convert.ToInt32(args[0]);
    backendPort = Convert.ToInt32(args[1]);
}
var client = new NetworkClient(address, backendPort);
string[] appArgs = {$"--urls={client.ClientNode.URL}"};
var builder = WebApplication.CreateBuilder(appArgs);
var configuration = builder.Configuration;
builder.Services.Configure<MinerSettings>(configuration.GetSection(nameof(MinerSettings)));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<NetworkClient>(client);
builder.Services.AddSingleton<Miner>();
builder.Services.AddHostedService<Miner>(s => s.GetRequiredService<Miner>());

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.UseCors(
    options => options.WithOrigins($"http://{address}:{frontendPort}").AllowAnyMethod().AllowAnyHeader()
);
app.Run();