var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;

builder.Services.AddControllers();
//builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();
//app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(
    options => options.WithOrigins("http://localhost:8080").AllowAnyMethod()
);

app.Run();

