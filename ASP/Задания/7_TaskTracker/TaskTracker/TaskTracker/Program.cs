using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем папку для базы
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "tasks.db");
var connectionString = $"Data Source={dbPath}";

// 2. Регистрируем DbContext с SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 3. Подключаем MVC (контроллеры + представления)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Применяем миграции при старте (если вы их создадите)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // На первой версии можно использовать EnsureCreated, 
    // но лучше сразу Migrate, чтобы потом работать с миграциями.
    // db.Database.Migrate();
}

// 5. Стандартный пайплайн
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 6. Роут по умолчанию — на наши задачи
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();