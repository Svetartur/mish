using ASP_P42.Data;
using ASP_P42.Services.Hash;
using ASP_P42.Services.Kdf;
using ASP_P42.Services.Time;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHash();
builder.Services.AddKdf();
builder.Services.AddTime();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LocalDB")
    )
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
