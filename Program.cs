using Microsoft.EntityFrameworkCore;
using NpgsqlCommandRaceSample;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build()
    .GetConnectionString("DbStorage");
builder.Services.AddDbContext<MyDbContext>(builder =>
{
    builder.UseNpgsql(connectionString,
        options => options.EnableRetryOnFailure());
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(cfg =>
    {
        var npgSqlMeter = new NpgSqlMeter();

        cfg.AddAspNetCoreInstrumentation()
            .AddMeter(NpgSqlMeter.Name)
            .AddInstrumentation(() => npgSqlMeter)
            .AddView("http.server.*", MetricStreamConfiguration.Drop);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();