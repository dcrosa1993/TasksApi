using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TasksApi.Api.Infrastructure.Persistence;

namespace TasksApi.Tests.Infrastructure;

public sealed class TasksApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production database configuration.
            var dbContextOptionsDescriptor = services.SingleOrDefault(
                descriptor =>
                    descriptor.ServiceType ==
                    typeof(IDbContextOptionsConfiguration<AppDbContext>)
            );

            if (dbContextOptionsDescriptor is not null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            // Replace the connection with an open in-memory SQLite connection.
            var dbConnectionDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbConnection)
            );

            if (dbConnectionDescriptor is not null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var connection =
                    serviceProvider.GetRequiredService<DbConnection>();

                options.UseSqlite(connection);
            });
        });

        builder.UseEnvironment("Testing");
    }
}