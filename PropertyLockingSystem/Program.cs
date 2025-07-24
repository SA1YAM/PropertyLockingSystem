using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PropertyLockingSystem.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Property}/{action=Index}/{id?}");

app.MapHub<PropertyLockHub>("/propertyLockHub");

app.Run();
