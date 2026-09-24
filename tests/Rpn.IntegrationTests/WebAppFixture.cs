using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rpn.Data;

namespace Rpn.IntegrationTests;

/// <summary>
/// Hosts the real application in memory, against a SQLite database that lives
/// only for the lifetime of the test run.
///
/// The point of an integration test is that everything below the entry point is
/// real: real routing, real filters, real services, real Entity Framework. The
/// only thing replaced here is where the data lands, because a test that writes
/// to rpn.db would leave rows behind for the next one.
/// </summary>
public class WebAppFixture : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development, so the two seeded accounts exist here as well.
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // An in-memory SQLite database stays alive exactly as long as the
            // connection does, so the fixture holds one open for the run.
            var registration = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (registration is not null) services.Remove(registration);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection?.Dispose();
    }
}
