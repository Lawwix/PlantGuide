using Microsoft.EntityFrameworkCore;
using PlantGuide.Data;

var builder = WebApplication.CreateBuilder(args);

// ������ ����������� ��� SQLite � ���� � ����� �������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=plants.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// ��������� ��������� MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// �������������� ���������� �������� � ������������� ����
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Initialize(db);
}

// ������������ HTTP-���������
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
