using Microsoft.EntityFrameworkCore;
using Pronia.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer("server=VICTUS\\SQLEXPRESS; database=ProniaDB;trusted_connection=true; integrated security=true; TrustServerCertificate=true;");
});

var app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=home}/{action=index}/{id?}"
    );


app.Run();
