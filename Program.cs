using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuctionApp.Data;
using AuctionApp.Hubs;
using AuctionApp.Models;
using AuctionApp.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавление DbContext с SQL Server 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Добавление Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuctionApp API",
        Version = "v1",
        Description = "API для аукционного приложения"
    });
});

// Добавление SignalR
builder.Services.AddSignalR();

// Добавление Razor Pages
builder.Services.AddRazorPages();

// Добавление сервисов приложения
builder.Services.AddScoped<IAuctionService, AuctionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Включение middleware для Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuctionApp API v1");
        //options.RoutePrefix = string.Empty;  Swagger UI будет доступен по корню сайта http://localhost:<port>/
    });
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<AuctionHub>("/auctionHub");
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Auctions/Index");
    return Task.CompletedTask;
});

app.Run();

public partial class Program { }