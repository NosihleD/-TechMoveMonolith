using Microsoft.EntityFrameworkCore;
using TechMove.Api.Data;
using TechMove.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContractRepository, ContractRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

bool isDocker = app.Environment.IsEnvironment("Docker");

if (app.Environment.IsDevelopment() || isDocker)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!isDocker)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// The API owns database initialization.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await db.Database.EnsureCreatedAsync();
}

app.Run();

public partial class Program { }
