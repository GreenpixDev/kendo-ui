# Маримекко для Kendo UI на ASP.NET MVC 5

Часть модуля «Аналитические отчёты» АИС ГП: диаграмма Маримекко по приложению А
(`brief_appendix_marimekko.md` в репозитории концепта). Конструктор форм, публикация и личные представления сюда не входят.

## Состав

| Путь | Назначение |
|---|---|
| `Reports.Web/Scripts/marimekko.js` | Диаграмма на Kendo Drawing API (canvas, переключается на svg опцией `type`) |
| `Reports.Web/Controllers/ReportDataController.cs` | `GET /ReportData/List`, `GET /ReportData/Marimekko?reportId=&filter=` |
| `Reports.Web/Services/` | `IReportDataService`, демо-данные в памяти, разбор фильтра по белому списку полей |
| `Reports.Web/Auth/` | `ICurrentUser`, заглушка входа `/Dev/Login`, `[ReportUserAuthorize]` (401/403 для API) |
| `Reports.Web/Views/Marimekko/Index.cshtml` | Страница: выбор отчёта, переключатель «График / Таблица» (Kendo Grid по той же выборке) |
| `tests/marimekko-harness.html` | Стенд без IIS: наборы MM-01…MM-13 и автопроверки геометрии |

## Запуск

Проект: .NET Framework 4.8, `Microsoft.AspNet.Mvc` 5.2.9, PackageReference. Нужен IIS Express (в Rider/Visual Studio это обычная конфигурация запуска).
Тестовый вход: `/Dev/Login` (включается `DevAuth:Enabled` в `Web.config`).

Docker (нужны Windows-контейнеры: в Docker Desktop выберите «Switch to Windows containers»):

```bash
docker compose up --build
# http://localhost:8080/Dev/Login
```

В Rider конфигурация «Reports.Web (Docker Compose)» лежит в `.run/` и появляется в списке запуска сама (нужен подключённый Docker: Settings → Build, Execution, Deployment → Docker, подключение с именем «Docker»).

Образы на Windows Server Core большие (несколько ГБ), первая сборка долгая. Запуск в контейнере не проверялся.

Стенд без IIS:

```bash
node tests/serve.js
# http://localhost:8765/tests/marimekko-harness.html
```

## Что нужно знать

- Kendo UI подключается с CDN, версия закреплена (2024.3.1015), без `Telerik.UI.for.AspNet.Mvc`. Без лицензионного ключа Kendo выводит предупреждение и водяной знак на Grid: положите свой `kendo-ui-license.js` в `Scripts/`.
- Версия .NET Framework — 4.8 (в АИС ГП 4.5). Пакет `Microsoft.AspNet.Mvc` 5.2.9 содержит сборки для net45, поэтому для 4.5 достаточно изменить `TargetFrameworkVersion` в `.csproj` и два атрибута `targetFramework` в `Web.config`; на 4.5 эта сборка не проверялась.
- Заглушка входа включена настройкой `DevAuth:Enabled`; в рабочей конфигурации её нужно выключить (`Web.Release.config` делает это при публикации в Release).
- Фильтр отчёта: `column=a|b;segment=c|d`; значения, содержащие `|` или `;`, в этом простом формате не поддерживаются.
- `JsonResult` в MVC 5 не меняет регистр свойств C#, поэтому контроллер отдаёт анонимные объекты в camelCase: клиент читает `column / segment / value`.
- В `Web.config` задано `fileEncoding="utf-8"`: без него Razor читает `.cshtml` без BOM как ANSI.
- Больше 8 сегментов: цвета палитры повторяются, различать сегменты нужно по легенде и подсказке.
- Реальный слой данных (`GROUP BY` по EF-модели АИС ГП) подключается реализацией `IReportDataService` вместо `DemoReportDataService`.
