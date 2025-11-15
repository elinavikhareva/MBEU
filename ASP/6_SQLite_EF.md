# SQLite + EF Core

---
## Основновная часть

## 1. Что такое DI (внедрение зависимостей)

**Задача:** перестать создавать нужные объекты руками в каждом месте и поручить это «умной кладовщице» — **контейнеру DI**.
Ты один раз **описываешь правила**, а дальше фреймворк сам подставляет нужные экземпляры в конструкторы классов.

---

## 1.1 Проблема без DI (почему «болит»)

Без DI всё делаешь вручную: в контроллере создаёшь сервис, в сервисе — репозиторий, в репозитории — контекст БД. В итоге:
- код связанный «в клее» `new ...()`;
- трудно тестировать (ничего не подменить);
- дублирование настроек (строки подключения и т.п.);
- сложно менять реализацию (надо лезть в десятки файлов).

Пример «как делать **не** надо»:
```csharp
public class DiaryController : Controller
{
    public IActionResult List()
    {
        var db = new AppDbContext(...);              // руками
        var repo = new DiaryRepository(db);          // руками
        var service = new DiaryService(repo);        // руками
        var items = service.List();
        return View(items);
    }
}
```

---

## 1.2 Идея DI за 20 секунд

- **Dependency (зависимость)** — это то, без чего класс не может работать (например, сервису нужен репозиторий).
- **Container (контейнер DI)** — «кладовщица», знает *как создавать* зависимости.
- **Registration (регистрация)** — правило: «если попросят **этот интерфейс**, выдай **такую реализацию**».
- **Resolution (разрешение)** — процесс выдачи готового экземпляра по запросу.

**Итог:** классы **не знают**, *как* создать зависимости — они просто **объявляют**, *какие* зависимости им нужны в конструкторе.

---

## 1.3 Конструкторная инъекция (наш стандарт)

Мы будем использовать **инъекцию через конструктор**: нужные объекты приходят параметрами конструктора. Важно понимать класс сам ничего не создаёт (new), а просто просит нужные объекты в конструкторе. Созданием занимается DI-контейнер(magic box).

### **Плохо создавать зависимости руками**

Пример

```
public class DiaryService
{
    private readonly DiaryRepository _repo;

    public DiaryService()
    {
        // ❌ ПЛОХО: жёсткая привязка к конкретному классу и конкретному способу создания
        _repo = new DiaryRepository(new AppDbContext(...));
    }

    public List<DiaryEntry> List() => _repo.GetAll();
}

```

Минусы такого подхода:
- Нельзя легко подменить DiaryRepository на фейковый для тестов.
- Если поменяется конструктор DiaryRepository или AppDbContext, придётся залезать сюда.
- Код сервиса знает слишком много деталей.

### Хорошо — зависимость приходит в конструктор

1. Выделяем интерфейс для репозитория:

```
public interface IDiaryRepository
{
    List<DiaryEntry> GetAll();
    // ... другие методы
}

```

2. Реализация интерфейса репозитория:

```csharp
public class DiaryRepository : IDiaryRepository
{
    private readonly AppDbContext _db;

    public DiaryRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<DiaryEntry> GetAll()
    {
        return _db.DiaryEntries
            .OrderByDescending(e => e.CreatedUtc)
            .ToList();
    }
}
```

3. Сервис просит IDiaryRepository в конструкторе:

```
public interface IDiaryService
{
    List<DiaryEntry> List();
}

public class DiaryService : IDiaryService
{
    private readonly IDiaryRepository _repo;

    // ✅ Хорошо: сервис не знает, КАК создан репозиторий, ему дали готовый
    public DiaryService(IDiaryRepository repo)
    {
        _repo = repo;
    }

    public List<DiaryEntry> List() => _repo.GetAll();
}

```

4. Контроллер просит `IDiaryService`:

```
 public class DiaryController : Controller
{
    private readonly IDiaryService _service;

    public DiaryController(IDiaryService service)
    {
        _service = service;
    }

    public IActionResult List()
    {
        var items = _service.List();
        return View(items);
    }
}

```

### Как DI-контейнер "склеивает" всё вместе

В Program.cs ты пишешь:

```
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=App_Data/app.db"));

builder.Services.AddScoped<IDiaryRepository, DiaryRepository>();
builder.Services.AddScoped<IDiaryService, DiaryService>();
builder.Services.AddControllersWithViews();

```

Что это значит:
- Если кому-то нужен `AppDbContext` → создай `AppDbContext` с такой строкой подключения.
- Если нужен `IDiaryRepository` → выдай `DiaryRepository`.
- Если нужен `IDiaryService` → выдай `DiaryService`.

Когда приходит HTTP-запрос:
1. Контейнер видит, что надо создать `DiaryController`.
2. Смотрит в конструктор: `DiaryController(IDiaryService service)`.
3. Ищет правило для `IDiaryService` → находит `DiaryService`.
4. Чтобы создать `DiaryService`, ему нужен `IDiaryRepository` → ищет правило, создаёт `DiaryRepository`.
5. Чтобы создать `DiaryRepository`, нужен `AppDbContext` → создаёт его.
6. Складывает всё, отдаёт готовый `DiaryController`, у которого внутри есть `_service`, внутри `_repo`, внутри `_db`.

Ни один из классов **не знает**, как именно создаётся зависимость — он только **декларирует потребность**.

### Несколько зависимостей в конструкторе

Можно просить сколько угодно зависимостей:

```
public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private readonly IClock _clock;
    private readonly ISmtpClient _smtp;

    public MailService(
        ILogger<MailService> logger,
        IClock clock,
        ISmtpClient smtp)
    {
        _logger = logger;
        _clock  = clock;
        _smtp   = smtp;
    }

    public Task SendAsync(string to, string body)
    {
        _logger.LogInformation("Отправляем письмо в {Time}", _clock.UtcNow);
        return _smtp.SendAsync(to, body);
    }
}

```

---

## 1.4 Жизненные циклы (lifetime) — кем и как часто «выдавать»

- **Singleton** — один объект на всё приложение.
  Подходит для **чистой логики без состояния**, кэшей, конфигураций, «часы»/рандомайзеры и т.п.
```
builder.Services.AddSingleton<ISystemClock, SystemClock>();
```

- **Scoped** — один объект на **один HTTP-запрос**.
  Стандарт для **DbContext, репозиториев, сервисов** в веб-приложении.

```
builder.Services.AddScoped<IDiaryRepository, DiaryRepository>();
```

- **Transient** — новый объект **каждый раз**, когда его запрашивают.
  Для лёгких «одноразовых» объектов (но не для DbContext).

```
builder.Services.AddTransient<TransientIdProvider>();
```

Живой пример с Guid — увидеть разницу глазами

```
public interface IIdProvider
{
    Guid Id { get; }
}

public class SingletonIdProvider : IIdProvider
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class ScopedIdProvider : IIdProvider
{
    public Guid Id { get; } = Guid.NewGuid();
}

public class TransientIdProvider : IIdProvider
{
    public Guid Id { get; } = Guid.NewGuid();
}

```
Регистрация
```
builder.Services.AddSingleton<SingletonIdProvider>();
builder.Services.AddScoped<ScopedIdProvider>();
builder.Services.AddTransient<TransientIdProvider>();

```

Контроллер:
```
public class DemoController : Controller
{
    private readonly SingletonIdProvider _s1;
    private readonly SingletonIdProvider _s2;
    private readonly ScopedIdProvider _sc1;
    private readonly ScopedIdProvider _sc2;
    private readonly TransientIdProvider _t1;
    private readonly TransientIdProvider _t2;

    public DemoController(
        SingletonIdProvider s1,
        SingletonIdProvider s2,
        ScopedIdProvider sc1,
        ScopedIdProvider sc2,
        TransientIdProvider t1,
        TransientIdProvider t2)
    {
        _s1 = s1; _s2 = s2;
        _sc1 = sc1; _sc2 = sc2;
        _t1 = t1; _t2 = t2;
    }

    public string Index()
    {
        return $"""
        Singleton: {_s1.Id} | {_s2.Id}
        Scoped:    {_sc1.Id} | {_sc2.Id}
        Transient: {_t1.Id} | {_t2.Id}
        """;
    }
}

```

**Правила безопасности:**
- **DbContext** — всегда **Scoped**.
- **Репозитории и сервисы**, которые используют DbContext, — тоже **Scoped**.
- **Singleton** **не должен** брать **Scoped** в конструкторе (иначе хранит ссылку на «чужой запрос» → ошибки).

Мини-таблица «можно/нельзя» (кто может зависеть от кого):

| Кто зависит ↓ \ От кого → | Singleton | Scoped | Transient |
|---------------------------|:---------:|:------:|:---------:|
| **Singleton**             |    ✓      |   ✗    |     ✓     |
| **Scoped**                |    ✓      |   ✓    |     ✓     |
| **Transient**             |    ✓      |   ✓    |     ✓     |

---

## 1.5 DI в ASP.NET Core — как это работает на практике

1) **Описываем правила** (в `Program.cs`): что выдавать по запросу интерфейса и с каким lifetime.
2) **Просим зависимости** в конструкторах (контроллеры/сервисы/репозитории).
3) **Контейнер** сам создаёт и передаёт готовые объекты при каждом запросе.

---

## 2. Rider: как подготовить проект и EF Core

1) Создай проект: **ASP.NET Core Web App (Model–View–Controller)**.
2) Открой Tool Window **NuGet**.
- Открой окно пакетов:
   - Либо **View → Tool Windows → NuGet**,
   - Либо иконка «коробочка/пакет» на правой боковой панели.
- В верхней части окна NuGet **сними галочку** `Include prerelease`.
   Это важно: так ты не увидишь нестабильные версии, которые часто не подходят по фреймворку.

**Почему снимаем:** предрелизы (preview/rc) часто собраны под будущие фреймворки (например, EF Core 10.x под `net10.0`). Нам для `net9.0`/`net8.0` нужны **стабильные** версии (EF Core 9.x).

3) Установи **одной версии**:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.Relational`
   - `Microsoft.EntityFrameworkCore.Sqlite`
   - `Microsoft.EntityFrameworkCore.Design`

> Ошибка **"supports net10.0"** значит ты взял(а) EF Core 10.x (RC) под .NET 10. Для `net9.0` ставим EF **9.x**.

> **Если вместо SQLite берёте другую БД (PostgreSQL / MySQL):**
>
> Базовый набор пакетов остаётся тем же:
>
> - `Microsoft.EntityFrameworkCore`
> - `Microsoft.EntityFrameworkCore.Relational`
> - `Microsoft.EntityFrameworkCore.Design`
>
> Меняется только **провайдер** (четвёртый пакет):
>
> - Для **SQLite**: `Microsoft.EntityFrameworkCore.Sqlite`
> - Для **PostgreSQL**: `Npgsql.EntityFrameworkCore.PostgreSQL`
> - Для **MySQL / MariaDB** (через Pomelo): `Pomelo.EntityFrameworkCore.MySql`
>
> Остальной код лекции (модели, репозитории, сервисы, контроллеры) не зависит от типа БД.


4) Внизу Rider открой **Terminal**:
```bash
dotnet new tool-manifest
dotnet tool install --global dotnet-ef --version 8.0.*
dotnet ef --version
```

**Очень важно**: перейти в папку, где лежит твой `*.csproj`.

> Ошибка **"Settings file 'DotnetToolSettings.xml' was not found in the package"** значит воспользуйся глобальным установщиком + указанием версии.
```bash
dotnet tool install --global dotnet-ef --version 8.0.*
```

**Что это делает:**
- `dotnet new tool-manifest` — создаёт файл `.config/dotnet-tools.json` в проекте (набор локальных инструментов).
- `dotnet tool install dotnet-ef` — ставит утилиту **только для этого проекта** (без админ-прав).
- `dotnet ef --version` — проверка, что команда теперь доступна.

**Если терминал пишет «No project was found» при миграциях** — значит ты не в папке с `*.csproj`. Перейди туда **или** используй флаги `--project`/`--startup-project`.

5) Создай папку **App_Data** в корне проекта — там будет `app.db` (файл базы).
**Зачем нужна:** SQLite — это файл на диске. Мы договорились хранить его в `App_Data/app.db`, чтобы не требовались админ-права и чтобы путь был одинаковый у всех.

> ⚠️ **Важно:** папка `App_Data` нужна только при работе с **SQLite**, потому что база хранится как файл (`app.db`) рядом с проектом.
>
> Если вы используете **PostgreSQL** или **MySQL/MariaDB**, база живёт в **сервере БД**, а не в файле:
> - никакой `App_Data` создавать не нужно;
> - путь к базе задаётся **строкой подключения** (Host/Server, Port, Database, Username, Password).



---

## 3. Архитектура «Модель → Репозиторий → Сервис → Контроллер»

Структура:
```
/Models
  DiaryEntry.cs
/Data
  AppDbContext.cs
/Repositories
  IDiaryRepository.cs
  DiaryRepository.cs
/Services
  IDiaryService.cs
  DiaryService.cs
/Controllers
  DiaryController.cs
Program.cs
Views/Diary/...
```

Далее — файлы с комментариями «что делает каждая строка».

---

### 3.1 Модель: что хранится в БД

`Models/DiaryEntry.cs`
```csharp
// Обычный C#‑класс — "форма" записи в дневнике.
// Каждое свойство станет столбцом таблицы DiaryEntries.
public class DiaryEntry
{
    public int Id { get; set; }              // Первичный ключ (уникальный идентификатор)
    public string Title { get; set; } = "";   // Заголовок (NOT NULL, ограничим длину)
    public string Text { get; set; } = "";    // Текст (NOT NULL)
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; // Когда создано (UTC)
}
```

---

### 3.2 Контекст БД: «ворота» к таблицам

`Data/AppDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;

// DbContext знает:
// 1) как подключаться к БД (строка подключения придёт из Program.cs);
// 2) какие таблицы есть (DbSet<...>);
// 3) какие ограничения и индексы у столбцов (OnModelCreating).
public class AppDbContext : DbContext
{
    // "Таблица" дневника: через неё читаем и пишем записи
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();

    // В конструктор прилетят настройки (options) из DI (Program.cs)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Здесь задаём ограничения и индексы для столбцов
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<DiaryEntry>(e =>
        {
            e.Property(p => p.Title).IsRequired().HasMaxLength(200); // NOT NULL + длина
            e.Property(p => p.Text).IsRequired();                    // NOT NULL
            e.HasIndex(p => p.CreatedUtc);                           // индекс по дате
        });
    }
}
```

---

### 3.3 Репозиторий: «общаемся с БД» (CRUD, без бизнес‑логики)

`Repositories/IDiaryRepository.cs`
```csharp
// Контракт: что умеет наш репозиторий.
// Только список операций. Никакой бизнес‑логики — это будет в сервисе.
public interface IDiaryRepository
{
    List<DiaryEntry> GetAll();     // Получить список записей
    DiaryEntry? GetById(int id);   // Найти запись по Id (или null)
    void Add(DiaryEntry entry);    // Добавить запись (пока без сохранения)
    void Update(DiaryEntry entry); // Обновить запись
    void Delete(int id);           // Удалить запись по Id (если есть)
    void Save();                   // Сохранить изменения в БД
}
```

`Repositories/DiaryRepository.cs`
```csharp
using Microsoft.EntityFrameworkCore;

// Тут только прямые операции с EF Core: SELECT/INSERT/UPDATE/DELETE.
public class DiaryRepository : IDiaryRepository
{
    private readonly AppDbContext _db; // контекст БД

    public DiaryRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<DiaryEntry> GetAll()
    {
        // AsNoTracking: быстрее читать списки, EF не следит за объектами
        return _db.DiaryEntries
                  .AsNoTracking()
                  .OrderByDescending(x => x.CreatedUtc)
                  .ToList();
    }

    public DiaryEntry? GetById(int id)
    {
        // Find ищет по первичному ключу
        return _db.DiaryEntries.Find(id);
    }

    public void Add(DiaryEntry entry)
    {
        _db.DiaryEntries.Add(entry);
        // Сохраняем отдельно (через Save), чтобы контролировать момент записи
    }

    public void Update(DiaryEntry entry)
    {
        _db.DiaryEntries.Update(entry);
    }

    public void Delete(int id)
    {
        var found = _db.DiaryEntries.Find(id);
        if (found != null)
            _db.DiaryEntries.Remove(found);
    }

    public void Save()
    {
        // Отправляем INSERT/UPDATE/DELETE в БД
        _db.SaveChanges();
    }
}
```

---

### 3.4 Сервис: «правила и проверки» поверх репозитория

`Services/IDiaryService.cs`
```csharp
// Контракт сервиса: что умеет "дневник" как бизнес‑логика.
public interface IDiaryService
{
    List<DiaryEntry> List();                      // Список записей
    DiaryEntry? Get(int id);                      // Одна запись
    void Create(string title, string text);       // Создать запись
    bool Edit(int id, string title, string text); // Изменить запись
    bool Delete(int id);                          // Удалить запись
}
```

`Services/DiaryService.cs`
```csharp
// Здесь простые проверки и одно сохранение за операцию.
public class DiaryService : IDiaryService
{
    private readonly IDiaryRepository _repo;

    public DiaryService(IDiaryRepository repo)
    {
        return _repo = repo;
    }

    public List<DiaryEntry> List()
    {
        return _repo.GetAll();
    }

    public DiaryEntry? Get(int id)
    {
        return _repo.GetById(id);
    }

    public void Create(string title, string text)
    {
        // Простейшая валидация: пустые строки не принимаем
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Заголовок пуст");
        if (string.IsNullOrWhiteSpace(text))  throw new ArgumentException("Текст пуст");

        var entry = new DiaryEntry
        {
            Title = title.Trim(),
            Text = text.Trim(),
            CreatedUtc = DateTime.UtcNow
        };

        _repo.Add(entry);
        _repo.Save(); // одно сохранение
    }

    public bool Edit(int id, string title, string text)
    {
        var entry = _repo.GetById(id);
        if (entry == null) return false;
        if (string.IsNullOrWhiteSpace(title)) return false;
        if (string.IsNullOrWhiteSpace(text))  return false;

        entry.Title = title.Trim();
        entry.Text = text.Trim();

        _repo.Update(entry);
        _repo.Save();
        return true;
    }

}
```

---

### 3.5 Контроллер: «склейка с веб‑страницами»

`Controllers/DiaryController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;

// Контроллер зависит ТОЛЬКО от сервиса (не от БД).
// Это упрощает тестирование и чтение кода.
public class DiaryController : Controller
{
    private readonly IDiaryService _service;

    public DiaryController(IDiaryService service)
    {
        _service = service;
    }

    // Показать список записей: GET /Diary/List
    public IActionResult List()
    {
        var items = _service.List();
        return View(items); // передаём список в представление
    }

    ....
}
```

---

### 3.6 Program.cs: настраиваем DI и EF

`Program.cs`
```csharp
using Microsoft.Data.Sqlite;           // строка подключения к SQLite
using Microsoft.EntityFrameworkCore;   // UseSqlite, DbContext и т.п.

// В этом примере ниже показано подключение **SQLite**.
// Для PostgreSQL / MySQL меняется только:
// - строка подключения,
// - и вызов UseSqlite(...) → UseNpgsql(...) или UseMySql(...).

var builder = WebApplication.CreateBuilder(args);

// 1) Готовим ПАПКУ для файла БД: <проект>/App_Data
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir); // если нет — создаст

// 2) Полный ПУТЬ к файлу app.db
var dbPath = Path.Combine(dataDir, "app.db");

// 3) Собираем строку подключения к SQLite
var csb = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,                          // куда положить файл
    Mode = SqliteOpenMode.ReadWriteCreate,        // создавай если нет
    Cache = SqliteCacheMode.Shared                // нормальный режим кэша
};

// 4) Регистрируем КОНТЕКСТ БД (один на HTTP‑запрос — Scoped)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(csb.ToString())
       .EnableSensitiveDataLogging() // учебно: логируй параметры SQL
);

// --- Варианты для других БД ---
//
// 4а) Если вместо SQLite используется **PostgreSQL**:
//
// var pgConnectionString =
//     "Host=localhost;Port=5432;Database=diary_db;Username=postgres;Password=YourPassword;";
//
// builder.Services.AddDbContext<AppDbContext>(opt =>
//     opt.UseNpgsql(pgConnectionString)
//        .EnableSensitiveDataLogging()
// );
//
// 4б) Если используется **MySQL / MariaDB** (через Pomelo.EntityFrameworkCore.MySql):
//
// var mySqlConnectionString =
//     "Server=localhost;Port=3306;Database=diary_db;User=root;Password=YourPassword;Charset=utf8mb4;";
//
// builder.Services.AddDbContext<AppDbContext>(opt =>
//     opt.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString))
//        .EnableSensitiveDataLogging()
// );
//
// ► Эти варианты **взаимозаменяемы** с блоком UseSqlite(...) выше — оставляем только один нужный вариант.


// 5) Регистрируем РЕПОЗИТОРИЙ и СЕРВИС (тоже Scoped)
builder.Services.AddScoped<IDiaryRepository, DiaryRepository>();
builder.Services.AddScoped<IDiaryService, DiaryService>();

// 6) Подключаем MVC (контроллеры + представления)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 7) При первом запуске — создадим БД по модели (если миграции пока не делаем)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();                      // создаст таблицы, если их нет
    db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;"); // включим внешние ключи
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");// режим журнала побыстрее
}

> ⚠️ **Про разные базы:**
>
> - Вызов `db.Database.EnsureCreated()` и команды `PRAGMA ...` относятся именно к **SQLite**
>   (файл на диске, свои настройки журнала и внешних ключей).
> - Для **PostgreSQL** и **MySQL/MariaDB**:
>   - `PRAGMA ...` **не пишем вообще** — это специфично для SQLite;
>   - вместо `EnsureCreated()` в реальных проектах почти всегда используют миграции:
>
>   ```csharp
>   db.Database.Migrate(); // применить все миграции к PostgreSQL / MySQL
>   ```
>
>   В учебном режиме можно временно оставить `EnsureCreated()`, но для «взрослых» БД лучше сразу приучать студентов к `Migrate()`.


app.MapDefaultControllerRoute(); // /{controller=Home}/{action=Index}/{id?}
app.Run();
```

---

## 4. Коротко про SQL (ровно то, что нужно здесь)

- **Таблица** — как Excel‑лист: колонки и строки.
- **Строка** — одна запись.
- **Первичный ключ** — уникальный Id.
- **Индекс** — ускоряет поиск/сортировку по колонке.

Мини‑команды:
```sql
-- Создание таблицы
CREATE TABLE DiaryEntries (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Title TEXT NOT NULL,
  Text  TEXT NOT NULL,
  CreatedUtc TEXT NOT NULL
);

-- Вставка в таблицу
INSERT INTO DiaryEntries (Title, Text, CreatedUtc)
VALUES ('Первая', 'Привет', '2025-11-07T10:00:00.0000000Z');

-- Выборка из таблицы
SELECT * FROM DiaryEntries ORDER BY CreatedUtc DESC;

-- Обновление данных в таблице
UPDATE DiaryEntries SET Title='Новое' WHERE Id=1;

-- Удаление строки из таблицы
DELETE FROM DiaryEntries WHERE Id=1;

-- Создание индекса
CREATE INDEX IX_DiaryEntries_CreatedUtc ON DiaryEntries(CreatedUtc);
```

---

## 5. Миграции: что это и какой SQL они запускают

- Миграции — это **C#-файлы** с «инструкциями», как **создать/обновить** схему БД.
- Внутри есть два метода: **`Up`** (вперёд) и **`Down`** (откат).
- EF хранит «снимок модели» (`ModelSnapshot`), чтобы понимать, что изменилось.

### Команды (в Rider → Terminal)
```bash
dotnet ef migrations add InitialCreate  # создать миграцию из текущих моделей
dotnet ef database update               # применить миграцию (создать/изменить БД)
dotnet ef migrations script             # вывести реальный SQL всех миграций
```

После `dotnet ef migrations` должен создаться файл миграции.


### Что внутри миграции (C#)

> Этот файл создается через команду. **НЕ РУКАМИ**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "DiaryEntries",
        columns: table => new
        {
            Id = table.Column<int>()
                .Annotation("Sqlite:Autoincrement", true),
            Title = table.Column<string>(maxLength: 200, nullable: false),
            Text = table.Column<string>(nullable: false),
            CreatedUtc = table.Column<DateTime>(nullable: false)
        },
        constraints: table => { table.PrimaryKey("PK_DiaryEntries", x => x.Id); });

    migrationBuilder.CreateIndex(
        name: "IX_DiaryEntries_CreatedUtc",
        table: "DiaryEntries",
        column: "CreatedUtc");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "DiaryEntries");
}
```

### Какой SQL выполнится в SQLite (примерно)
```sql
CREATE TABLE "DiaryEntries" (
  "Id" INTEGER NOT NULL CONSTRAINT "PK_DiaryEntries" PRIMARY KEY AUTOINCREMENT,
  "Title" TEXT NOT NULL,
  "Text" TEXT NOT NULL,
  "CreatedUtc" TEXT NOT NULL
);
CREATE INDEX "IX_DiaryEntries_CreatedUtc" ON "DiaryEntries" ("CreatedUtc");
```

Если добавить новое свойство:
```csharp
public bool IsPinned { get; set; }
```
Миграция создаст:
```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsPinned",
    table: "DiaryEntries",
    type: "INTEGER",      // bool в SQLite хранится как 0/1
    nullable: false,
    defaultValue: false);
```
SQL будет таким:
```sql
ALTER TABLE "DiaryEntries" ADD COLUMN "IsPinned" INTEGER NOT NULL DEFAULT 0;
```

> В SQLite сложные изменения столбцов иногда делаются так: создать временную таблицу → скопировать данные → удалить старую → переименовать. Это нормально.

### Типовые ошибки и быстрые исправления

1) **«Build failed»**
   → Проект не компилируется. Исправь ошибки компиляции, потом снова `dotnet ef ...`.

2) **«Unable to create an object of type 'AppDbContext'»**
   → CLI не смог создать контекст.
   ✔ Проверь, что `AddDbContext<AppDbContext>(...)` есть в `Program.cs`.
   ✔ Добавь **фабрику дизайна** (`IDesignTimeDbContextFactory<AppDbContext>`) — см. пункт 0.3.

3) **«No DbContext named 'AppDbContext'»**
   → В проекте несколько контекстов или другой неймспейс.
   ✔ Укажи явное имя:
   ```bash
   dotnet ef migrations add InitialCreate --context AppDbContext
   ```

4) **Неправильные версии пакетов («supports net10.0»)**
   → Ты поставил EF Core 10.x (предрелиз) под `net10`.
   ✔ Сними *Include prerelease*, поставь **EF Core 9.x** для `net9.0`.

5) **Смешал `EnsureCreated()` и миграции**
   → Удали `EnsureCreated()`; используй `db.Database.Migrate()`.

6) **Команда запущена не из той папки**
   → Делай `dotnet ef ...` **в папке с `.csproj`** или укажи `--project`.

7) **Файл БД не там, где ждёшь**
   → Проверь путь в `Program.cs` (`App_Data/app.db`).
   ✔ Открой лог старта — там видно реальную строку подключения.

8) **«Database is locked» в SQLite**
   → Закрой все соединения (окна Database в Rider), повтори команду.

---

## 6. Как «создать базу прямо сейчас» (если миграции страшно)

В `Program.cs` у нас уже есть:
```csharp
db.Database.EnsureCreated();
```
Это создаст таблицы **по текущей модели**. Позже **перейдёшь на миграции** — это правильнее для командной работы и обновлений.


> 🧩 **Зависимость от типа БД:**
>
> - Для **SQLite** такой «быстрый старт» через `EnsureCreated()` вполне окей на учебных проектах.
> - Для **PostgreSQL** и **MySQL/MariaDB** `EnsureCreated()` тоже работает, но:
>   - в реальных системах для серверных БД почти всегда используют **миграции**,
>   - то есть вместо `EnsureCreated()` в `Program.cs` вызывают `db.Database.Migrate();` Но чаще накатывают миграции через терминал.
>     и управляют изменениями схемы таблиц через `dotnet ef migrations ...`

---

## Дополнительная информация


## 1. Модификаторы доступа (кто кого видит)

Модификаторы доступа отвечают на вопрос: **кто может пользоваться чем** в твоём коде.

- **Класс** — это как «коробка».
- **Методы/свойства/поля класса** — это «вещи внутри коробки».
- Модификаторы говорят, кому можно заглядывать в коробку и трогать вещи.

### Зачем они нужны

- Скрыть внутренности (инкапсуляция), чтобы их не сломали.
- Разделить «публичный интерфейс» (что можно трогать) и «внутреннюю кухню».
- Сделать код понятнее: видно, что для внешнего мира, а что — только для себя.

### Модификаторы для КЛАССОВ (верхний уровень, файл .cs)

Доступно **3** варианта:

- `public` — класс видят **все проекты**, если они **ссылаются** на твой проект.
  *Метафора:* поставили коробку на главную полку магазина — все видят.
- `internal` — класс виден **только внутри этого проекта**. *(Это значение по умолчанию, если ничего не писать.)*
  *Метафора:* коробка стоит в подсобке магазина — видят только сотрудники (код внутри проекта).
- `file` — класс виден **только в этом .cs-файле** (C# 11).
  *Метафора:* маленькая личная коробочка на твоём столе — никто, кроме этого файла, не видит.

### Пример
```csharp
// PublicThing.cs (Проект: Lib)
public class PublicThing {}   // виден другим проектам, которые ссылаются на Lib

// InternalThing.cs (Проект: Lib)
class InternalThing {}        // это internal по умолчанию — виден ТОЛЬКО в Lib

// Helpers.cs (Проект: Lib)
file class FileOnlyHelper {}  // виден только в этом файле Helpers.cs

```

### Для членов класса (методы/свойства/поля)
- **public** — всем можно.
- **private** — только этому классу.
- **protected** — этому классу и его наследникам.
- **internal** — всем классам **текущего проекта**.
- **protected internal** — наследникам **или** этому проекту.
- **private protected** — только наследникам **в этом же проекте**.

### 1.3 Важное правило: «член не может быть доступнее, чем класс»

Если класс `internal`, то даже `public` метод **не будет виден** из другого проекта, потому что сам класс спрятан.

```csharp
// Проект: Lib
internal class Hidden
{
    public void Hello() {} // формально public, но снаружи проекта НЕ видно,
                           // потому что класс internal
}

```

### Примеры «что можно / что нельзя»

### Сценарий: два проекта
- **Lib** — библиотека (class library).
- **App** — приложение, которое **ссылается** на Lib.

```csharp
// Проект Lib
public class A
{
    public    int X = 1;
    internal  int Y = 2;
    protected int Z = 3;
    private   int T = 4;
}

internal class B {}  // виден только в Lib

```

```
// Проект App (ссылается на Lib)
var a = new A();
_ = a.X;  // OK: public видно всем
// _ = a.Y;  // ОШИБКА: Y internal — виден только внутри Lib
// _ = a.Z;  // ОШИБКА: Z protected — App не наследник A
// _ = a.T;  // ОШИБКА: T private — видно только внутри A

// new B(); // ОШИБКА: класс B internal — не виден в App

```

### `protected` — доступно наследнику
```csharp
// Проект App (ссылается на Lib)
public class AChild : A
{
    void Test()
    {
        _ = X; // OK (public)
        _ = Z; // OK (protected): мы наследник A
        // _ = Y; // ОШИБКА: internal из Lib, а мы в другом проекте
        // _ = T; // ОШИБКА: private
    }
}
```

---

### Разница: `protected internal` vs `private protected`

- **`protected internal`** = **protected ИЛИ internal**
  → Доступ либо как наследник (даже из другого проекта), либо как любой код в этом проекте.
- **`private protected`** = **protected И internal** (ОБА сразу)
  → Доступ только если **наследник** и **в этом же проекте**.

| Ситуация                                       | protected | internal | protected internal | private protected |
|-----------------------------------------------|:---------:|:--------:|:------------------:|:-----------------:|
| Тот же класс                                   |     ✓     |    ✓     |         ✓          |         ✓         |
| Другой класс, тот же проект, **не** наследник  |     ✗     |    ✓     |         ✓          |         ✗         |
| Наследник в том же проекте                     |     ✓     |    ✓     |         ✓          |         ✓         |
| Наследник в другом проекте                     |     ✓     |    ✗     |         ✓          |         ✗         |
| Не наследник, другой проект                    |     ✗     |    ✗     |         ✗          |         ✗         |

---

### Вложенные классы (класс внутри класса)

По умолчанию они **`private`** — видны только внутри внешнего класса.

```csharp
public class Outer
{
    class Inner { }            // private по умолчанию — виден только в Outer
    public class PubInner { }  // снаружи виден, если виден сам Outer (он public)
}
```

---

### Набор примеров на все модификаторы

```csharp
public class Demo
{
    public    int A = 1; // доступен всем, кто видит Demo
    internal  int B = 2; // доступен только в текущем проекте
    protected int C = 3; // доступен в этом классе и наследниках
    private   int D = 4; // доступен только внутри Demo

    protected internal int E = 5; // наследники (везде) ИЛИ любой код в этом проекте
    private protected  int F = 6; // только наследники И только внутри этого проекта

    public void Show()
    {
        // внутри класса доступно всё:
        _ = A; _ = B; _ = C; _ = D; _ = E; _ = F;
    }
}
```

---

# 2. Область видимости (scope)

**Scope (область видимости)** отвечает на вопрос: *«Где моё имя (переменная/параметр/метод) видно?»*

---

### Базовые правила

- Имя, объявленное **внутри фигурных скобок `{ ... }`**, видно **только внутри** этих скобок.
- Параметры метода видны **только** внутри этого метода.
- Нельзя объявлять **две переменные с одинаковым именем** в одной и той же области (будет конфликт).
- Вложенный блок может **перекрыть** внешнее имя (тенирование) — так лучше **не делать**.

```csharp
int x = 10;           // x видна от этой строки до конца текущего блока

{
    int y = 20;       // y видна только здесь, внутри фигурных скобок
}

// Console.WriteLine(y); // ОШИБКА: y "умер" за пределами блока
```

---

### Где начинаются и заканчиваются области

| Конструкция                      | Где объявил имя                       | Где оно видно                                  |
|----------------------------------|----------------------------------------|------------------------------------------------|
| Метод                            | Параметры                              | Только внутри тела метода                      |
| Блок `{ ... }`                   | Переменная внутри блока                | Только внутри этого блока                      |
| `for (int i = 0; ...)`           | `i` в заголовке `for`                  | Только внутри `for` (включая тело цикла)       |
| `if (...) { } else { }`          | Переменная в `if`-блоке                | Только в соответствующем блоке                 |
| `switch ... case ...`            | Имена в `case` (с фигурными скобками)  | Только в данном `case`-блоке                   |
| Класс (поле/свойство/метод)      | Члены класса                           | Видны по модификаторам (`public`, `private` …) |
| Файл/пространство имён           | Типы/классы                            | По модификаторам (`public/internal/file`)      |

---

### Частые мини-сценарии

### A) Переменная цикла живёт внутри цикла
```csharp
for (int i = 0; i < 3; i++)
{
    Console.WriteLine(i); // i видна тут
}

// Console.WriteLine(i); // ОШИБКА: i вне области видимости
```

### B) Тенирование (не делай так)
```csharp
int value = 10;

{
    // int value = 5; // ОШИБКА: в этой области уже есть имя value из внешней области
    int another = 5;  // норм
}
```

### C) Локальная переменная перекрывает поле (лучше назвать по-разному)
```csharp
public class Sample
{
    private int value = 100;       // поле класса

    public void Print()
    {
        int value = 5;             // локальная переменная (плохая идея: имя совпало)
        Console.WriteLine(value);  // 5 (локальная перекрыла поле)
        Console.WriteLine(this.value); // 100 (обращение к полю через this)
    }
}
```

---

### `out var` и `is`-паттерны: где живут новые имена

### `out var name` — имя видно **после** вызова
```csharp
if (int.TryParse("123", out var n))
{
    Console.WriteLine(n); // здесь n есть и присвоено (123)
}

Console.WriteLine(n);     // и здесь n уже есть (для TryParse: 123/0)
```
> Для `TryParse` значение будет присвоено всегда: `123` при успехе, `0` при неуспехе.

### `is type name` — имя видно **только там, где условие истинно**
```csharp
object obj = 42;

if (obj is int k)
{
    Console.WriteLine(k); // k есть только тут (42)
}

// Console.WriteLine(k); // ОШИБКА: k вне области (завязан на условие if)
```

---

### `switch` и область видимости

Каждому `case` лучше явно делать свой блок `{ ... }`, чтобы имена не конфликтовали.

```csharp
switch (DateTime.Now.DayOfWeek)
{
    case DayOfWeek.Monday:
    {
        int d = 1;
        Console.WriteLine(d);
        break;
    }
    case DayOfWeek.Tuesday:
    {
        int d = 2; // это НОВАЯ d, не конфликтует с d из другого case
        Console.WriteLine(d);
        break;
    }
}
```

---

### Замыкания (closures): имя «живёт дольше блока»

Лямбда **захватывает** переменные по ссылке. Поэтому после выхода из блока переменная может продолжать жить, пока жива лямбда.

Проблемный пример в цикле:
```csharp
var actions = new List<Action>();

for (int i = 0; i < 3; i++)
{
    actions.Add(() => Console.WriteLine(i));
}

// Печатает: 3, 3, 3 (НЕ 0,1,2) — лямбды видят ОДИН и тот же i, уже равный 3
foreach (var a in actions) a();
```

Правильный «фикс» — копия в новую локальную:
```csharp
var actions = new List<Action>();

for (int i = 0; i < 3; i++)
{
    int copy = i; // новая переменная с нужной областью видимости
    actions.Add(() => Console.WriteLine(copy));
}

// Печатает: 0, 1, 2 — как ожидается
foreach (var a in actions) a();
```

---

### Разница «область видимости» vs «время жизни»

- **Область видимости** — где *можно написать имя* в коде.
- **Время жизни** — когда *объект реально существует*.

Обычно локальные переменные «умирают» в конце блока, **но**:
- если их **захватило** замыкание (лямбда/анонимный метод), они могут жить дольше метода;
- поля класса живут, пока жив объект; `static`-поля — пока живо приложение.

---

### Частые ошибки и как их избежать

1) **Использую переменную вне блока**
   → Перенеси `Console.WriteLine(...)` внутрь блока или подними объявление выше.

2) **Дублирую имя во внутреннем блоке**
   → Не объявляй одно и то же имя дважды; дай другое имя (`count` → `innerCount`).

3) **Лямбды в цикле печатают одно и то же**
   → Делай копию переменной цикла внутри тела (`var j = i;`), захватывай `j`.

4) **Путаю видимость и модификаторы доступа**
   → Модификаторы (`public/private/...`) регулируют доступ **между классами/проектами**.
     Scope — видимость **внутри кода/блоков**.

---


## 3 `using` и `namespace` — это разные вещи

### 1) using‑директива (верху файла) — подключаем пространства имён
```csharp
using System;                        // Console.WriteLine без System.Console
using Microsoft.EntityFrameworkCore; // DbContext без полного имени
```

**global using (на весь проект, один файл GlobalUsings.cs):**
```csharp
global using System;
global using Microsoft.EntityFrameworkCore;
```

### 2) using‑statement (внутри метода) — «закрой за собой дверь»
```csharp
using var conn = new SqliteConnection("Data Source=:memory:");
// по выходу из метода conn.Dispose() вызовется сам
```

---