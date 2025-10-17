# MVC-приложение с использованием Razor

## 1. Что такое MVC — простыми словами

**MVC** — это способ "разложить по полочкам" всё, что происходит в веб-приложении.  
Название расшифровывается как:  
**Model (Модель)**, **View (Представление)**, **Controller (Контроллер)**.  

Представь, что сайт — это как **пиццерия с доставкой** 🍕  

- **Пользователь (браузер)** — клиент, который делает заказ.  
- **Контроллер** — оператор, который принимает заказ по телефону.  
- **Модель** — сама пицца: рецепт, ингредиенты и цена.  
- **Хранилище (База данных)** — кухня, где всё готовится.  
- **View (Представление)** — упаковка и внешний вид пиццы, то, как она выглядит при доставке клиенту.  

---

### Как это работает по шагам

1. **Клиент (браузер)** заходит на сайт и просит: “Покажи список пицц”.  
2. **Контроллер** получает этот запрос и говорит модели: “Дай список всех пицц”.  
3. **Модель** берёт данные из базы (кухни) и отдаёт контроллеру.  
4. **Контроллер** передаёт эти данные в **View (представление)**.  
5. **View** оформляет эти данные в красивую HTML-страницу и возвращает пользователю.  

📦 Поток выглядит так:  
**Браузер → Контроллер → Модель (через базу данных) → Контроллер → View → HTML → Браузер**

---

### Почему MVC — это удобно

- 🔹 **Порядок в коде** — каждый отвечает за своё:  
  модель — за данные, контроллер — за логику, view — за внешний вид.  
- 🔹 **Проще чинить** — если ошибка в отображении, идём во View; если ошибка в логике — в контроллер.  
- 🔹 **Легче развивать** — можно менять дизайн без трогания логики, и наоборот.  
- 🔹 **Понятно другим** — почти все .NET-разработчики знают MVC, это стандарт.  

---

## 2. Что такое Razor в MVC

**Razor** — это язык, который позволяет **смешивать C# и HTML** в одном файле.  
Он нужен, чтобы **показать данные на странице** — например, имя пользователя, список товаров или сообщение об ошибке.  

Razor-файлы имеют расширение **`.cshtml`** и используются внутри **Views (представлений)**.  
Главная фишка Razor — символ **`@`**, который показывает: “сейчас начинается код на C#”.

---

### 💡 Как это работает

Razor берёт HTML-шаблон и вставляет в него кусочки C#-кода.  
Когда сервер обрабатывает страницу, он **выполняет код**, подставляет значения и **отправляет уже готовый HTML** в браузер.

**Пример идеи:**
- Было: “@Model.Name”
- Стало (в браузере): “Пицца Маргарита”

---

### 🔧 Что можно делать с Razor

- Писать C# прямо внутри HTML.  
- Выводить данные из модели: `@Model.Name`, `@Model.Price`.  
- Использовать **условия** (`if`, `else`).  
- Делать **циклы** (`foreach`, `for`).  
- Использовать **Tag Helpers** — специальные теги, которые помогают работать с контроллерами и маршрутами.  
- Управлять заголовками, layout’ами, подключениями скриптов.


---

## 3. Что такое Razor Pages

**Razor Pages** — это способ создавать веб-страницы без контроллеров.  
Каждая страница сама себе «контроллер»: у неё есть **HTML с Razor-кодом** и рядом — **C#-файл с логикой**.  

---

### 🧩 Как устроено

Каждая страница состоит из двух файлов:

1. `Index.cshtml` — это **разметка** (HTML + Razor).  
2. `Index.cshtml.cs` — это **код-бихайнд** (C#), где обрабатываются запросы.

---

### ⚙️ Как это работает

1. Пользователь открывает страницу (например `/Products`).  
2. Razor Page автоматически вызывает метод `OnGet()` в файле `Products.cshtml.cs`.  
3. Этот метод получает данные и передаёт их в страницу Razor (`Products.cshtml`).  
4. Если пользователь отправляет форму — вызывается `OnPost()`.

---

### 🧠 Когда удобно использовать Razor Pages

- Маленькие сайты, лендинги, страницы с формами.
- Приложения, где нет чётких API и сложной архитектуры.
- Быстрое создание CRUD-страниц (создание, редактирование, удаление записей).  

---

## 4. Что такое Blazor (для кругозора)

**Blazor** — это современный способ создавать веб-интерфейсы **на C# вместо JavaScript**.  
Он позволяет писать весь код приложения (и серверный, и клиентский) **на одном языке — C#**.  

Blazor не использует MVC. Это другой подход, где всё приложение состоит из **компонентов** — маленьких кусочков интерфейса, которые можно многократно использовать.

---

### ⚙️ Как работает Blazor

Blazor бывает двух видов:

| Вид | Где выполняется код | Особенности |
|---|-------|------|
| **Blazor WebAssembly** | Прямо в браузере (через технологию WebAssembly) | Работает без сервера, всё загружается один раз и выполняется локально. |
| **Blazor Server** | На сервере, а браузер только отображает результат | Быстрее запускается, но требует постоянного соединения с сервером (SignalR). |

---

### 🧩 Что такое компонент

Компонент — это кусочек страницы (например, карточка товара, меню, форма), написанный на Razor + C#.  
Каждый компонент — это файл с расширением `.razor`.

Когда страница открывается, Blazor вставляет это как HTML, но управляет логикой уже на C#.

---

### 💬 Почему мы упоминаем Blazor, но не используем

В этом курсе мы не изучаем Blazor, потому что он ближе к современным клиентским SPA (вроде React или Angular).
Наша цель — понять основу MVC и Razor, чтобы уметь строить архитектуру и backend, а потом уже легко перейти к Blazor при желании.

Важно:
Blazor — это не «дополнение» к MVC, а альтернативный способ построения UI в .NET.

---

## 5. Почему «MVC + Razor в MVC» — проще для обучения и ближе к enterprise/API

- **Ментально проще:** MVC насильно делит код на «данные/логика/вид», упрощая понимание потока запроса.
- **Похоже на Web API:** контроллеры и маршруты те же; отличие — View vs `return Ok(...)`. Переход от MVC к API минимален.
- **Упражнение на интерфейсы и DI:** контроллер зависит от абстракции (`IProductStore`), реализация подменяется без изменения контроллера — ровно как в «взрослых» сервисах.
- **Масштабируемость и тесты:** отделение представления от логики облегчает тестирование и рефакторинг.

---

## 6. Как создать MVC‑проект в JetBrains Rider (.NET 9)

1. **File → New… → ASP.NET Core Web App (MVC)**.  
2. Нажмите **Create** → **Build** → **Run (▶)**.  
3. Если Rider спросит браузер — выберите Chromium‑based (Chrome/Edge).

---

## 7. Структура проекта и назначение ключевых файлов

```
 WebApplication/
    ├─ WebApplication4.sln                  ← решение (открываем его в Rider)
    └─ WebApplication4/                     ← сам проект (один из проектов в решении)
       ├─ WebApplication4.csproj            ← файл проекта (.NET настройки)
       ├─ Program.cs                        ← запуск приложения и настройка маршрутов/пайплайна
       ├─ Controllers/                      ← контроллеры (C# классы)
       │   └─ HomeController.cs
       ├─ Models/                           ← модели (классы данных/DTO/VM)
       │   └─ ErrorViewModel.cs
       ├─ Views/                            ← Razor-представления (шаблоны страниц)
       │   ├─ _ViewImports.cshtml
       │   ├─ _ViewStart.cshtml
       │   ├─ Shared/                       ← общие части (шаблоны макета и т.п.)
       │   │   └─ _Layout.cshtml
       │   └─ Home/                         ← папка с представлениями для HomeController
       │       ├─ Index.cshtml
       │       └─ Privacy.cshtml
       ├─ wwwroot/                          ← статические файлы (CSS, JS, картинки, шрифты)
       │   ├─ css/site.css
       │   ├─ js/site.js
       │   └─ lib/                          ← сторонние библиотеки (bootstrap, jquery и т.п.)
       ├─ appsettings.json                  ← общие настройки приложения
       ├─ appsettings.Development.json      ← настройки для окружения Development
       ├─ Properties/
       │   └─ launchSettings.json           ← как запускать проект локально (URL, профиль)
```

---

### Коротко, что это за файлы

-   **`*.sln`** - «папка-ярлык» для Rider/Visual Studio, в ней ссылки
    на проекты.\
-   **`*.csproj`** - «паспорт» проекта: таргет-фреймворк, включённые
    фичи, NuGet-пакеты.\
-   **`Program.cs`** - точка входа. Настраиваем сервисы, middleware и
    маршруты.\
-   **`Controllers/`** - обработчики запросов (классы Controller).\
-   **`Models/`** - классы данных, которые передаём между контроллером
    и представлением.\
-   **`Views/`** - Razor-страницы (HTML + C#).\
-   **`wwwroot/`** - обычные статические файлы, которые отдаются как
    есть.\
-   **`appsettings*.json`** - конфиги. Можно складывать строки
    подключения, ключи, флаги.\
-   **`launchSettings.json`** - как запускать локально (адрес, HTTPS,
    переменные окружения).

---

### Что происходит при запуске (`Program.cs`)

Проект использует **минимальную хостинг-модель** (современный стиль .NET
8/9):

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1) Регистрируем сервисы для MVC:
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2) Настраиваем конвейер (middleware):
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // страница ошибок в проде
    app.UseHsts();                           // HSTS для безопасности
}

app.UseHttpsRedirection();   // перенаправляет http → https
app.UseRouting();            // включает маршрутизацию
app.UseAuthorization();      // проверка авторизации (если будет нужна)

// 3) Раздача статики:
app.MapStaticAssets();       // wwwroot + WithStaticAssets ниже

// 4) Маршрут по умолчанию (MVC):
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();                   // запускаем веб-сервер
```
---

Прямо человечьи: 
- Мы сказали «хочу MVC» →
`AddControllersWithViews()`. 
- Мы собрали приложение →`builder.Build()`. 
- Мы включили полезные «фильтры по дороге»
(middleware): HTTPS, обработчик ошибок, маршрутизация и т.д. 
- Мы настроили **маршрут по умолчанию**: если просто открыть `/`, то это
значит `HomeController.Index`. - Мы включили раздачу статики из
`wwwroot`.
---


## 8. Жизнь одного запроса (как браузер получает HTML)

1.  Браузер открывает `https://localhost:7217/`
2.  Маршрутизация видит пустой путь и подставляет **по умолчанию** `controller=Home`, `action=Index`.
3.  Запускается `HomeController.Index()` → возвращает `View()`.
4.  MVC ищет файл представления: `Views/Home/Index.cshtml`.
5.  Представление рендерится внутри **общего макета** `Views/Shared/_Layout.cshtml`.
6.  Итоговый HTML уходит в браузер.

---

## 9. Контроллер и его действия (Actions)

Файл: `Controllers/HomeController.cs`

```csharp
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public HomeController(ILogger<HomeController> logger) { _logger = logger; }

    public IActionResult Index()   => View(); // вернёт Views/Home/Index.cshtml
    public IActionResult Privacy() => View(); // вернёт Views/Home/Privacy.cshtml

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}
```

-   **`Controller`** - базовый класс с удобными методами: `View()`,
    `Json()`, `Redirect()` и т.д.
-   **`Index/Privacy`** - обычные действия. Имена совпадают с именами
    файлов в `Views/Home/`.
-   **`Error()`** - отдаёт специальную страницу ошибок и передаёт в
    представление модель `ErrorViewModel`.
---

## 10. Представления (Views) и общие файлы Razor

### 10.1 Общие правила

-   Для `HomeController.Index()` MVC ищет `Views/Home/Index.cshtml`.
-   Для `HomeController.Privacy()` - `Views/Home/Privacy.cshtml`.
-   **Макет** (`_Layout.cshtml`) - общий шаблон страницы (шапка/меню/подвал). Конкретная страница вставляется туда через `@RenderBody()`.

### 10.2 Что такое `_ViewStart` и `_ViewImports`

-   `Views/_ViewStart.cshtml` - один раз задаёт макет для всех
    страниц:

    ```csharp
    @{
        Layout = "_Layout";
    }
    ```

-   `Views/_ViewImports.cshtml` - «общие using и включение
    TagHelper'ов»:

    ```csharp
    @using WebApplication
    @using WebApplication.Models
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    ```

### 10.3 Пример представления

`Views/Home/Index.cshtml`:

```csharp
@{
    ViewData["Title"] = "Home Page";
}
<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <p>Learn about <a href="https://learn.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
</div>
```

`Views/Shared/_Layout.cshtml` (фрагмент):

``` html
<title>@ViewData["Title"] - WebApplication4</title>
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
...
<nav>
  <a class="navbar-brand" asp-controller="Home" asp-action="Index">WebApplication4</a>
  <a class="nav-link" asp-controller="Home" asp-action="Privacy">Privacy</a>
</nav>
<main>
  @RenderBody()
</main>
@await RenderSectionAsync("Scripts", required: false)
```

Обратите внимание на **Tag Helpers**: 
- `asp-controller="Home"` +`asp-action="Privacy"` - вместо ручной ссылки `/Home/Privacy` MVC сам соберёт правильный URL. 
- `asp-append-version="true"` - к файлу стилей добавится хеш-версия (`site.css?v=...`), чтобы браузер не кэшировал старую версию.

## 11. Модели (Models)

Здесь только `ErrorViewModel` (передаём его в представление ошибки):

```csharp
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
```

В реальном приложении тут появляются **DTO**, **ViewModel** и доменные
модели.\
Главная идея: **модель - это данные**, которые вы показываете в
представлении или получаете/отправляете в экшенах.

## 12. Статические файлы (`wwwroot`)

Папка `wwwroot/` - это **корень для статики**. Всё, что внутри,
доступно по URL.
 
Примеры: 
- `~/css/site.css` → `/css/site.css` 
- `~/js/site.js` → `/js/site.js` 
- `~/lib/bootstrap/...` →`/lib/bootstrap/...`

Проект уже настроен на Bootstrap и jQuery, поэтому стили и скрипты
подключаются в `_Layout.cshtml`.

------------------------

## 13. Конфигурация и окружения

### 13.1 `appsettings.json` и `appsettings.Development.json`

-   `appsettings.json` - базовый конфиг (уровни логирования, настройки
    и т.п.).
-   `appsettings.Development.json` - **переопределяет** базовый
    конфиг, когда `ASPNETCORE_ENVIRONMENT=Development`.

Пример из проекта:

``` json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 13.2 `launchSettings.json` (локальный запуск)

Настраивает профили запуска и окружение:

``` json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7217;http://localhost:5266",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

-   **`applicationUrl`** - порты, на которых будет доступно
    приложение.
-   **`ASPNETCORE_ENVIRONMENT`** - задаёт окружение (`Development`),
    поэтому включатся dev-настройки и страница ошибок для разработчика
    (если её подключить).

------------------------

## 14. Маршруты: как MVC понимает, куда вести запрос

В `Program.cs` задан **один конвенциональный маршрут**:

```csharp
pattern: "{controller=Home}/{action=Index}/{id?}"
```

Расшифровка: 
- Если путь пустой (`/`) → `controller=Home`, `action=Index`. 
- Если путь `/Home/Privacy` → контроллер `HomeController`, метод `Privacy()`. 
- `{id?}` - необязательный параметр, будет доступен в методе `action(int id)`.

Это как адреса на домофоне - «подъезд/квартира/этаж». Если что-то не пишем - берётся по умолчанию.

------------------------

## 15. Где здесь безопасность, ошибки и HTTPS

-   **`UseHttpsRedirection()`** - принудительный переход на защищённый
    протокол.
-   **`UseHsts()`** - браузер «запоминает», что сайт работает только
    по HTTPS (в проде).
-   **`UseExceptionHandler("/Home/Error")`** - если упадём в проде,
    покажем аккуратную страницу `/Home/Error`.

------------------------

## 16 View подрбнее

## 16.1 Где живут представления и как MVC их находит

**Правило папок:** файл представления ищется по шаблону:

    Views/{ИмяКонтроллера}/{ИмяДействия}.cshtml

Примеры: 
- `HomeController.Index()` → `Views/Home/Index.cshtml` 
- `ProductsController.Details(int id)` → `Views/Products/Details.cshtml`

**Общие файлы:** 
- `Views/Shared/_Layout.cshtml` - общий макет
(шапка/подвал/меню).\
- `Views/_ViewStart.cshtml` - выбирает макет по умолчанию:
`Layout = "_Layout";`\
- `Views/_ViewImports.cshtml` - общие `@using` и включение Tag
Helpers:\
`cshtml   @using Проект.Namespace   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`

------------------------

## 16.2 Структура обычного View-файла

Минимальный шаблон:

```cshtml
@model MyApp.Models.ProductViewModel      <!-- указываем тип модели (можно без ;) -->
@{
    ViewData["Title"] = "Страница товара"; // заголовок для <title> в макете
}

<h1>@ViewData["Title"]</h1>

<div>
    <p>Название: @Model.Name</p>          <!-- доступ к данным модели -->
    <p>Цена: @Model.Price</p>
</div>
```

**Что важно:** 
- `@model` (вверху файла) задаёт **тип модели** для этого представления. 
- `Model` (с большой буквы) - **экземпляр** этого типа, который передал контроллер через `return View(модель)`. 
- `ViewData["Title"]` - простое хранилище данных по ключу (словарь), удобно для мелочей (заголовки и т. п.).

------------------------

## 16.3 `@model`, `Model`, `ViewData`, `ViewBag`, `TempData`

### 16.3.1 `@model` и `Model`

```cshtml
@model MyApp.Models.ProductViewModel

<h2>@Model.Name</h2>
<p>Цена: @Model.Price</p>
```

-   **`@model`** - объявление типа.
-   **`Model`** - объект, переданный из контроллера:

```csharp
    public IActionResult Details(int id) {
        var vm = _service.GetProduct(id);
        return View(vm); // vm попадёт в Model в .cshtml
    }
```

### 16.3.2 `ViewData`

-   Тип: `ViewDataDictionary` (словарь `string -> object`).
-   Доступ в .cshtml: `ViewData["Title"]`, `ViewData["Message"]`.

**Пример**:

```csharp
    public IActionResult Index() {
        ViewData["Title"] = "Каталог";
        return View();
    }
```

```cshtml
    <title>@ViewData["Title"] - MyApp</title>
```

-   **Когда использовать:** быстрые простые значения, не создавать для них отдельную модель.

### 16.3.3 `ViewBag`

-   Динамическая обёртка над `ViewData`. То же самое, только через свойства:

    ```csharp
    ViewBag.Title = "Каталог";
    ```

    ```cshtml
    <title>@ViewBag.Title - MyApp</title>
    ```

-   **Рекомендация:** для учебных проектов можно, в проде лучше **предпочитать `@model` + `Model`** (типобезопасно).

### 16.3.4 `TempData`

-   Временное хранилище (живёт **один запрос вперёд** - после
    редиректа удобно показывать сообщения).

-   Пример (контроллер):

    ```csharp
    TempData["Success"] = "Товар сохранён";
    return RedirectToAction("Index");
    ```

    (в представлении Index):

    ```cshtml
    @if (TempData["Success"] is string msg) {
        <div class="alert alert-success">@msg</div>
    }
    ```

------------------------

## 16.4 Razor-синтаксис «на пальцах»

### 16.4.1 Вставки значений

```cshtml
<p>@DateTime.Now</p>
<p>@Model.Name</p>
```

### 16.4.2 Условия и циклы

```cshtml
@if (Model.IsAvailable)
{
    <span>В наличии</span>
}
else
{
    <span>Нет на складе</span>
}

<ul>
@foreach (var item in Model.Items)
{
    <li>@item.Title</li>
}
</ul>
```

### 16.4.3 Кодовые блоки

```cshtml
@{
    var total = Model.Items.Sum(i => i.Price);
}
<p>Сумма: @total</p>
```

### 16.4.4 Экранирование `@`

```cshtml
<p>Собачка: @@</p> <!-- выведет символ @ -->
```

------------------------

## 16.5 Макеты (Layout), секции и частичные представления

### 5.1 Макет `_Layout.cshtml`

Главный каркас страницы:

```cshtml
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>@ViewData["Title"] - MyApp</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    @RenderSection("Head", required: false) <!-- опциональная секция -->
</head>
<body>
    <header>…меню…</header>
    <main class="container">
        @RenderBody() <!-- сюда подставляется содержимое конкретной страницы -->
    </main>
    <footer>© @DateTime.Now.Year</footer>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @RenderSection("Scripts", required: false)
</body>
</html>
```

### 16.5.2 Секция в дочернем представлении

```cshtml
@section Scripts {
    <script>console.log('только для этой страницы')</script>
}
```

### 16.5.3 Частичные представления (Partial Views)

-   Файлы-шаблоны для повторных кусков UI, например `Views/Shared/_ProductCard.cshtml`.
-   Использование:

    ```cshtml
    @await Html.PartialAsync("_ProductCard", product)
    ```

### 16.5.4 View Components (продвинутый вариант частичных)

-   Микроконтроллер + мини-представление в одном компоненте. Хорошо для виджетов (меню, корзина).\
-   В учебном курсе достаточно понимать что они есть.

------------------------

## 16.6 Tag Helpers - правильный способ писать HTML для MVC

**Почему Tag Helpers лучше:** они генерируют правильные ссылки/атрибуты
«по правилам маршрутизации» и дружат с моделью и валидацией.

### 16.6.1 Ссылки

```cshtml
<a asp-controller="Products" asp-action="Details" asp-route-id="@item.Id">Подробнее</a>
```

Генерирует корректный URL для `ProductsController.Details(int id)`.

### 16.6.2 Формы

```cshtml
<form asp-controller="Products" asp-action="Create" method="post">
    <input asp-for="Name" class="form-control" />
    <input asp-for="Price" class="form-control" />
    <button type="submit" class="btn btn-primary">Сохранить</button>
</form>
```

-   `asp-for` - привязывает поле к свойству модели (`@model ProductCreateDto`).
-   Автоматически добавляет **anti-forgery token** (если используется `<form asp-…method="post">`). Это глубокая тема про безопасность приложений, можно почитать отдельно, однако мы не будет затрагивать в рамках курса.

### 16.6.3 Поля ввода и подписи

```cshtml
<label asp-for="Name" class="form-label"></label>
<input asp-for="Name" class="form-control" />
<span asp-validation-for="Name" class="text-danger"></span>
```

### 16.6.4 Списки/выпадающие

```cshtml
<select asp-for="CategoryId" asp-items="Model.Categories" class="form-select"></select>
```

Где в модели:
`public List<SelectListItem> Categories { get; set; }`

### 16.6.5 Валидация на клиенте

**Клиентская валидация** — это когда ошибки проверяются в браузере, до отправки формы на сервер.

В макете **добавьте скрипты** (если используете клиентскую валидацию):

```cshtml
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
```

И используйте `asp-validation-summary`, `asp-validation-for` в форме.

В рамках курса - не будет трогать.

------------------------

## 16.7 GET vs POST: как представления работают с действиями контроллера

### 16.7.1 Паттерн «Показать форму (GET) → Принять форму (POST) → Redirect»

Контроллер:

```csharp
public class ProductsController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        var vm = new ProductCreateVm
        {
            Categories = _catalog.GetCategoriesSelectList()
        };
        return View(vm); // Views/Products/Create.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductCreateVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = _catalog.GetCategoriesSelectList(); // заново заполнить списки!
            return View(vm); // показать ошибки на той же форме
        }

        _catalog.CreateProduct(vm);
        TempData["Success"] = "Товар создан";
        return RedirectToAction("Index");
    }
}
```

View `Create.cshtml`:

```cshtml
@model ProductCreateVm
@{
    ViewData["Title"] = "Создание товара";
}
<h1>@ViewData["Title"]</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="Name" class="form-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Price" class="form-label"></label>
        <input asp-for="Price" class="form-control" />
        <span asp-validation-for="Price" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="CategoryId" class="form-label"></label>
        <select asp-for="CategoryId" asp-items="Model.Categories" class="form-select"></select>
        <span asp-validation-for="CategoryId" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Сохранить</button>
</form>
```

**Что важно понять:** 
- **GET**-действие отдаёт **пустую форму** (или форму с начальными данными).
- **POST**-действие получает заполненные данные (**model binding** по именам полей), проверяет `ModelState`.
- Если есть ошибки - **возвращаем View с той же моделью**, чтобы
показать ошибки.\
- Если всё ок - **сохраняем** и делаем **Redirect** (чтобы избежать повторной отправки при обновлении страницы).

### 16.7.2 Пример формы для поиска (GET)

```cshtml
<form asp-action="Index" method="get">
    <input type="text" name="q" value="@Model.Query" />
    <button type="submit">Найти</button>
</form>
```

Контроллер:

```csharp
public IActionResult Index(string? q)
{
    var items = _repo.Search(q);
    return View(new SearchVm { Query = q, Items = items });
}
```

-   Для поисков/фильтров чаще используют **GET**, чтобы параметры были в адресной строке и работали закладки.

------------------------

## 16.8) Валидация моделей и отображение ошибок

Модель:

```csharp
public class ProductCreateVm
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
```

View (фрагмент - уже был выше):

```cshtml
<div asp-validation-summary="ModelOnly" class="text-danger"></div>
<label asp-for="Name"></label>
<input asp-for="Name" />
<span asp-validation-for="Name" class="text-danger"></span>
```

-   Атрибуты (`[Required]`, `[Range]`, `[StringLength]`) проверяются **на сервере** всегда.
-   Если подключены скрипты - будет и **клиентская** валидация (моментально подсветит поле).

------------------------

## 16.9 Практические советы и «правильный стиль» для Views

1.  **Строго типизируйте представления** через `@model`. Меньше ошибок, лучше автодополнение.
2.  **Меньше логики в `.cshtml`** - максимум условия и простые циклы. Вся тяжёлая логика - в контроллер/сервис.
3.  **Используйте Tag Helpers** вместо «ручных» ссылок/форм - меньше ошибок с маршрутами и валидацией.
4.  **Единый макет** и **частичные представления** для повторяющихся блоков (карточки, меню).
5.  **Возврат после POST - только Redirect** (PRG-паттерн: Post/Redirect/Get).
6.  **Не доверяйте клиентской валидации**: она удобна, но сервер должен проверять всё заново.
7.  **Имена полей формы должны совпадать с именами свойств модели** - тогда биндится «само».
8.  **Соблюдайте папочную структуру**: `Views/{Controller}/{Action}.cshtml`. Так проще жить всем.

------------------------

## 16.10 Частые ошибки новичков

-   Забыли `@model` → `Model` пустой/тип `dynamic` → ошибок не видно до рантайма.
-   Сломали пути статики → нет стилей/скриптов. Всю статику храните в `wwwroot`.\
-   Не вернули списки (SelectList) при `ModelState.IsValid == false` → выпадающие пустые при повторном показе формы.\
-   Пишут `<form method="post">` без `asp-action`/`asp-controller` и без анти-фрода - ловят 400/валидация не работает.\
-   Выводят ошибки только через `asp-validation-summary`, но забывают `asp-validation-for` у полей - пользователю трудно понять, что не так.

------------------------
