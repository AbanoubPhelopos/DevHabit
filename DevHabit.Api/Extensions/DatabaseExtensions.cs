namespace DevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();

            app.Logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while migrating the database");
            throw;
        }

    }
}
