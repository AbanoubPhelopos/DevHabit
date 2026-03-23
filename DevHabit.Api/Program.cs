using DevHabit.Api.Controllers;
using DevHabit.Application.Contracts.Repositories;
using DevHabit.Application.Contracts.UnitOfWork;
using DevHabit.Application.Interfaces;
using DevHabit.Application.Services;
using DevHabit.Application.Services.Abstractions;
using DevHabit.Infrastructure.Database;
using DevHabit.Infrastructure.Repositories;
using DevHabit.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlSerializerFormatters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options
    => options.UseNpgsql(builder.Configuration.GetConnectionString("Database"),
            npgsqlOptions
                => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
        .UseSnakeCaseNamingConvention());

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<RequestScopedService>();
builder.Services.AddScoped<IHabitService, HabitService>();
builder.Services.AddScoped<ITagService, TagService>();

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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
