using TechMove.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<CurrencyService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["CurrencyApi:BaseUrl"]
        ?? "https://open.er-api.com/v6/latest/");

    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("TechMoveApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]
        ?? "http://localhost:5004/");

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<FileValidationService>();
builder.Services.AddScoped<FileValidationService>();
builder.Services.AddScoped<ContractWorkflowService>();

var app = builder.Build();

bool isDocker = app.Environment.IsEnvironment("Docker");

if (!app.Environment.IsDevelopment() && !isDocker)
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!isDocker)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
