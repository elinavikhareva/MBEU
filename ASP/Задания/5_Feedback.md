# 🧩 Практическое задание

## Тема: Мини-сайт с формой обратной связи (MVC + Razor)

---

### 🎯 Цель

Закрепить базовые знания о **MVC**, **Razor-представлениях**, **формах** и **Model Binding**.
Понять, как данные переходят: **View → Controller → View**.

---

## 🧱 Часть 1. Болванка

Создай новый проект:

> File → New Project → ASP.NET Core Web App (Model-View-Controller)

Дай имя: `MiniFeedbackApp`

---

### 1. Модель (`Models/FeedbackModel.cs`)

```csharp
// Модель — это просто класс с данными.
// Эти данные будут приходить из формы и уходить обратно во View.

public class FeedbackModel
{
    // Имя пользователя
    public string UserName { get; set; }

    // Текст сообщения
    public string Message { get; set; }
}
```

---

### 2. Контроллер (`Controllers/FeedbackController.cs`)

```csharp
using Microsoft.AspNetCore.Mvc;
using MiniFeedbackApp.Models;

namespace MiniFeedbackApp.Controllers;

// Контроллер получает запросы от браузера.
// Через атрибуты [HttpGet] и [HttpPost] мы разделяем показ формы и приём данных.
public class FeedbackController : Controller
{
    // GET-запрос просто показывает страницу с формой
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // POST-запрос обрабатывает отправленную форму
    [HttpPost]
    public IActionResult Index(FeedbackModel model)
    {
        // ViewData — временное хранилище данных для View
        ViewData["Result"] = $"Спасибо, {model.UserName}! Ваше сообщение: {model.Message}";
        return View();
    }
}
```

---

### 3. Представление (`Views/Feedback/Index.cshtml`)

```cshtml
@model MiniFeedbackApp.Models.FeedbackModel

<!-- Заголовок страницы -->
<h2>Обратная связь</h2>

<!-- Форма отправляется методом POST к действию Index контроллера Feedback -->
<form asp-action="Index" method="post">
    <div>
        <label asp-for="UserName">Ваше имя:</label>
        <input asp-for="UserName" class="form-control" />
    </div>

    <div>
        <label asp-for="Message">Сообщение:</label>
        <textarea asp-for="Message" class="form-control"></textarea>
    </div>

    <button type="submit" class="btn btn-primary">Отправить</button>
</form>

<!-- Если сервер сохранил результат, показываем сообщение -->
@if (ViewData["Result"] != null)
{
    <p class="alert alert-success">@ViewData["Result"]</p>
}
```

---

### 4. Что происходит при запуске

1. Открывается `/Feedback/Index` — метод `[HttpGet]`, показывается форма.
2. Пользователь вводит данные и жмёт кнопку.
3. Отправляется `POST /Feedback/Index` с полями формы.
4. Контроллер получает модель `FeedbackModel`.
5. Выводит сообщение “Спасибо, {имя}!”.

---

## 🚀 Часть 2. Задание на доработку

Теперь сделай **реальное улучшение** этой болванки.
Нужно превратить простой “Feedback” в мини-сайт с регистрацией и списком отзывов.

---

### 🔧 Что нужно сделать

1. **Добавь страницу регистрации пользователя** (`Register`):
   - Модель: `RegisterModel` с полями `Login`, `Password`, `Email`.
   - Добавь атрибуты валидации (`[Required]`, `[EmailAddress]`, `[StringLength]`).
   - Если `ModelState.IsValid == false`, показать ошибки через `asp-validation-for`.
   - После успешной регистрации — `TempData["Success"] = "Регистрация прошла успешно!"` и `RedirectToAction("Register")`.

2. **Добавь список отправленных сообщений**:
   - При каждом POST в FeedbackController добавляй сообщение в `static List<FeedbackModel>`.
   - Сделай новую страницу `/Feedback/List`, где выводятся все сообщения (имя + текст).

3. **Используй частичное представление** `_FeedbackForm.cshtml`:
   - Вынеси форму отправки в `Views/Shared/_FeedbackForm.cshtml`.
   - Подключи её в `Views/Feedback/Index.cshtml` через
     `@await Html.PartialAsync("_FeedbackForm", new FeedbackModel())`.

4. **Добавь загрузку файла** (доп. задача ★):
   - Форма с `IFormFile Photo` и `enctype="multipart/form-data"`.
   - Сохраняй файл в `/wwwroot/uploads/`.
   - Покажи сообщение “Файл успешно загружен”.

---

### 💡 Подсказки

- Используй примеры из разделов **“Формы и обработка данных”** и **“Razor-формы с Tag Helpers”**.
- Не забывай про `[ValidateAntiForgeryToken]` при POST.
- Для списков отзывов можно просто хранить данные в `static List<>` — база не нужна.
- Используй Bootstrap-классы для красоты (`btn`, `form-control`, `alert-success`).

---

## ✅ Критерии оценки (10 баллов)

| Критерий | Баллы | Проверка |
|-----------|--------|----------|
| Проект запускается без ошибок | 1 | Проверка запуска |
| Реализована форма регистрации с валидацией | 2 | Ошибки выводятся под полями |
| После успешной отправки выполняется Redirect (PRG) | 1 | Повторная отправка невозможна |
| Реализован список отзывов (`/Feedback/List`) | 2 | Все сообщения выводятся |
| Использовано частичное представление `_FeedbackForm` | 1 | Форма повторно используется |
| Корректно применены `TempData`, `ViewData`, `ModelState` | 2 | Проверка работы сообщений и ошибок |

---

**Максимум: 10 баллов.**
