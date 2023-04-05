using DCR;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;

builder.Services.AddControllers();
//builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Miner>();
builder.Services.AddHostedService<Miner>(s => s.GetRequiredService<Miner>());
// builder.Services.AddSingleton<IHostedService, Node>();
var app = builder.Build();
//app.UseSwagger();


app.UseAuthorization();

app.MapControllers();

app.UseCors(
    options => options.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader()
);

app.Run();

