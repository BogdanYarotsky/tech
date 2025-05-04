
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;

public static class Migrator
{
    internal static void EnsureDatabaseInGoodShape(string connectionString, CancellationToken token)
    {
        using var provider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(Migrator).Assembly).For.All())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider();

        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}