using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<CurrencyService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["CurrencyApi:BaseUrl"]
        ?? "https://open.er-api.com/v6/latest/");

    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddScoped<FileValidationService>();
builder.Services.AddScoped<ContractWorkflowService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// Creates the local SQL Server database from the EF Core model for this prototype.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
