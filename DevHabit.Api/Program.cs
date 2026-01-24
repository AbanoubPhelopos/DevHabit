using Npgsql;
using OpenTelemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database"),
            npgsqlOptions 
                => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName,Schemas.Application))
        .UseSnakeCaseNamingConvention());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql())
    .WithMetrics(matrics => matrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.AddOtlpExporter();
});


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
