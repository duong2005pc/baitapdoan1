using Microsoft.EntityFrameworkCore;
using TrafficLaw.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var connStr = builder.Configuration.GetConnectionString("SqlServerConnection");
        options.UseSqlServer(connStr);
    }
    else
    {
        var connStr = builder.Configuration.GetConnectionString("SqliteConnection");
        options.UseSqlite(connStr);
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "processed", "traffic_laws.json");
        if (File.Exists(jsonPath))
        {
            await DataSeeder.SeedFromJsonAsync(context, jsonPath);
            Console.WriteLine("--> Đã đồng bộ dữ liệu luật giao thông từ JSON vào CSDL thành công.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Lỗi khởi tạo CSDL: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();