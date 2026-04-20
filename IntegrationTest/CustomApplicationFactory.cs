using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using vizsgaController.Persistence;

namespace IntegrationTesting
{
    public class CustomApplicationFactory
     : WebApplicationFactory<vizsgaController.Program>
    {
        private SqliteConnection _connection = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Kiszedsz MINDENT, amit az AddDbContextPool (Npgsql) felrakhat
                services.RemoveAll<DbContextOptions<NewsDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<NewsDbContext>();

                services.RemoveAll<IDbContextPool<NewsDbContext>>();
                services.RemoveAll<IScopedDbContextLease<NewsDbContext>>();
                services.RemoveAll<IDbContextFactory<NewsDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<NewsDbContext>>();

                // Sqlite in-memory (maradjon nyitva)
                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();

                // Tesztben ne poolozz
                services.AddDbContext<NewsDbContext>(o => o.UseSqlite(_connection));

                // DB létrehozás
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
                db.Database.EnsureCreated();
                DbSeeder.Seed(db);

                // A TestServer HTTP-n fut, így a Secure cookie nem kerül mentésre
                services.PostConfigure<CookieAuthenticationOptions>(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                    });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _connection?.Dispose();
        }
    }
}
