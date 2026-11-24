using Microsoft.EntityFrameworkCore;
using TrafficLaw.API.Data;
using TrafficLaw.API.Services;

// Đọc cấu hình từ file .env trước khi tạo ứng dụng.
LoadDotEnv();

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

// Ollama chỉ được gọi qua địa chỉ loopback trên cùng máy chạy Backend.
var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"]
    ?? Environment.GetEnvironmentVariable("OLLAMA_BASE_URL")
    ?? "http://127.0.0.1:11434/";

if (!Uri.TryCreate(ollamaBaseUrl, UriKind.Absolute, out var ollamaUri) ||
    (ollamaUri.Host != "localhost" && !System.Net.IPAddress.TryParse(ollamaUri.Host, out var ip) ||
     (System.Net.IPAddress.TryParse(ollamaUri.Host, out ip) && !System.Net.IPAddress.IsLoopback(ip))))
{
    throw new InvalidOperationException(
        "OLLAMA_BASE_URL phải trỏ tới localhost hoặc địa chỉ loopback để không mở Ollama ra mạng bên ngoài.");
}

builder.Services.AddHttpClient<OllamaService>(client =>
{
    client.BaseAddress = new Uri(ollamaBaseUrl.EndsWith('/') ? ollamaBaseUrl : ollamaBaseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(90);
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

// Đọc file .env đơn giản; biến môi trường đã tồn tại sẽ được ưu tiên giữ nguyên.
static void LoadDotEnv()
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (!File.Exists(envPath))
        return;

    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var separator = line.IndexOf('=');
        if (separator <= 0)
            continue;

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim().Trim('"', '\'');
        if (!string.IsNullOrWhiteSpace(key) &&
            Environment.GetEnvironmentVariable(key) is null)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}