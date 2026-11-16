using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TasksApi.Api.Infrastructure.Persistence;

namespace TasksApi.Tests.Infrastructure;

public sealed class TestApi : IDisposable
{
    private readonly TasksApiFactory _factory;

    public HttpClient Client { get; }

    private TestApi(TasksApiFactory factory, HttpClient client)
    {
        _factory = factory;
        Client = client;
    }

    public static async Task<TestApi> CreateAsync()
    {
        var factory = new TasksApiFactory();

        try
        {
            var client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();

            var dbContext =
                scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.MigrateAsync();

            return new TestApi(factory, client);
        }
        catch
        {
            factory.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}