using AuctionApp;
using AuctionApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AuctionApp.Tests.TestHelpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
       
        private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AuctionAppTestDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
            
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);


                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(TestConnectionString);
                });


                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();  // Очистить базу перед тестами
                db.Database.EnsureCreated();  // Создать базу и схему
            });
        }
    }
}
