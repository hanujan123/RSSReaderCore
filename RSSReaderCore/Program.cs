using Microsoft.EntityFrameworkCore;
using RSSReaderCore.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options => 
		options.UseSqlServer(builder.Configuration.GetConnectionString("FeedsDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
		app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Feeds}/{action=Index}/{id?}");

app.Run();
