using DCR;

var address = "localhost";
var port = 4300;
if (args.Length == 2) {
    address = args[0];
    port = Convert.ToInt32(args[1]);
}
var client = new NetworkClient(address, port);
string[] appArgs = {$"--urls={client.ClientNode.URL}"};
var builder = WebApplication.CreateBuilder(appArgs);
var Configuration = builder.Configuration;

builder.Services.AddControllers();
//builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<NetworkClient>(client);
builder.Services.AddSingleton<Miner>();
builder.Services.AddHostedService<Miner>(s => s.GetRequiredService<Miner>());

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.UseCors(
    options => options.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader()
);
app.Run();