using AuctionApp.Data;
using Microsoft.EntityFrameworkCore;

public static class TestDbContextFactory
{
    //private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Database=AuctionAppTestDb;Trusted_Connection=True;MultipleActiveResultSets=true";

    public static AppDbContext Create()
    {
        var dbName = $"AuctionAppTestDb_{Guid.NewGuid()}";
        var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
