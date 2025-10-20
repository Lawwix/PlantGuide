using Microsoft.EntityFrameworkCore;
using PlantGuide.Data;

var builder = WebApplication.CreateBuilder(args);

// строка подключения для SQLite — файл в корне проекта
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=plants.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Добавляем поддержку MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Автоматическое применение миграций и инициализация базы
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Initialize(db);
}

// Конфигурация HTTP-конвейера
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Plants}/{action=Index}/{id?}");

app.Run();
