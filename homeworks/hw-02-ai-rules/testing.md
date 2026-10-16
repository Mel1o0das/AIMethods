# Домашнее задание № 2. Тестирование системы AI-правил

## 1. Цель и условия эксперимента

Для проверки влияния проектных инструкций `AGENTS.md` в **Codex** использован Backend существующего BookingService (ASP.NET Core/.NET 10, PostgreSQL). Проводились два отдельных запуска одинаковой по постановке задачи — добавление защищённого маршрута `GET /api/bookings/{id}`, который должен возвращать бронирование только его владельцу или администратору.

Для анализа представлены два архива реального кода: `Booking_without_rules.zip` и `Booking_with_rules2.zip`. Исходные файлы сопоставлены непосредственно. Без учёта `bin`, `obj`, `.idea` и служебных каталогов в архиве без правил находится **100 файлов**, с правилами — **101 файл**; дополнительный файл — `AGENTS.md`. Из 100 общих файлов только **два отличаются по содержимому**, остальные совпадают. Оба набора включают один и тот же ранее созданный `GetActiveCount.cs`; он не влияет на сравнение этой задачи.

Совпадение исходных файлов установлено при проверке архивов, но исходная переписка Codex с точным текстом каждого отправленного промпта и параметры выбранной модели не приложены. Поэтому эксперимент показывает реальные отличия результатов, но не является строго контролируемым измерением эффекта только от правил.

## 2. Постановка задачи и критерии сравнения

Оба варианта должны добавить защищённый API-метод получения одного бронирования, используя существующую архитектуру `IEndpoint → handler → IBookingRepository → EF Core`, а также разграничение прав владельца и администратора.

Ожидаемые коды: `200` при разрешённом доступе, `401` без авторизации, `403` при чужой записи и `404` при отсутствии бронирования. В сравнении рассматривались маршрут, получение пользователя из JWT, проверка владельца и роли, формат ошибок, слои проекта, масштаб изменений, сборка и замечания Codex о рисках.

Это **восстановленная формулировка задачи** из диалога и итоговых ответов, а не дословная копия обоих промптов. Исходные ответы агентов помещены в `evidence/`.

## 3. Сырой исходный код без правил

Полный фактически полученный файл `BookingService.Application/Features/Bookings/GetById.cs`:

```csharp
using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings/{id:guid}", async Task<EndpointResult<BookingsDto>> (
            Guid id,
            ClaimsPrincipal principal,
            [FromServices] GetBookingByIdHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class GetBookingByIdHandler
{
    private readonly ILogger<GetBookingByIdHandler> _logger;
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdHandler(
        ILogger<GetBookingByIdHandler> logger,
        IBookingRepository bookingRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingsDto, Error>> Handle(
        Guid id,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userIdString = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
            return Error.Authentication("auth.invalid_token", "Недействительный токен");

        var bookingResult = await _bookingRepository.GetByIdAsync(id, cancellationToken);
        if (bookingResult.IsFailure)
            return bookingResult.Error;

        var booking = bookingResult.Value;
        if (booking.UserId != userId && !IsAdmin(principal))
            return Error.Authorization("booking.access.denied", "Нет доступа к бронированию");

        _logger.LogInformation("Get booking {Id} by user {UserId}", id, userId);

        return booking;
    }

    private static bool IsAdmin(ClaimsPrincipal principal)
    {
        var roleName = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return roleName.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || roleName.Contains("админ", StringComparison.OrdinalIgnoreCase);
    }
}
```

Дополнительные три исходных файла варианта «без правил» находятся в `comparison/without_rules/` с сохранением их путей внутри проекта.

## 4. Сырой исходный код с правилами

Полный фактически полученный файл `BookingService.Application/Features/Bookings/GetById.cs`:

```csharp
using System.Security.Claims;
using BookingService.Application.EndpointsSettings;
using BookingService.Domain.Shared;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Features.Bookings;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/bookings/{id:guid}", async Task<EndpointResult<BookingsDto>> (
            Guid id,
            ClaimsPrincipal principal,
            [FromServices] GetBookingByIdHandler handler,
            CancellationToken cancellationToken = default) =>
                await handler.Handle(id, principal, cancellationToken))
            .RequireAuthorization();
    }
}

public sealed class GetBookingByIdHandler
{
    private readonly ILogger<GetBookingByIdHandler> _logger;
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdHandler(
        ILogger<GetBookingByIdHandler> logger,
        IBookingRepository bookingRepository)
    {
        _logger = logger;
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingsDto, Error>> Handle(
        Guid id,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userIdString = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
            return Error.Authentication("auth.invalid_token", "Invalid token");

        var bookingResult = await _bookingRepository.GetByIdAsync(id, cancellationToken);
        if (bookingResult.IsFailure)
            return bookingResult.Error;

        var booking = bookingResult.Value;
        if (booking.UserId != userId && !IsAdminUser(principal))
            return Error.Authorization("booking.access_denied", "Access to the booking is forbidden");

        _logger.LogInformation("Get booking {Id} for user {UserId}", id, userId);

        return booking;
    }

    private static bool IsAdminUser(ClaimsPrincipal principal)
    {
        var roleName = principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return roleName.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || roleName.Contains("админ", StringComparison.OrdinalIgnoreCase);
    }
}
```

Дополнительные три исходных файла варианта «с правилами» находятся в `comparison/with_rules/`; рядом приложен точный diff `comparison/actual_diff.patch`. Использовавшийся файл инструкций совпадает с переданным `AGENTS.md`.

## 5. Пофайловое сравнение фактических результатов

| Файл | Без правил | С правилами | Вывод по реальному коду |
|---|---|---|---|
| `Features/Bookings/GetById.cs` | Endpoint, handler, JWT, владелец/администратор | Те же механизмы | Различаются сообщения об ошибках, строковый код `403`, имя вспомогательного метода и сообщение лога |
| `Features/Bookings/IBookingRepository.cs` | `GetByIdAsync(Guid, CancellationToken)` | То же самое | **Файлы идентичны** |
| `Repositories/BookingRepository.cs` | `AsNoTracking()`, выборка по `BookingId`, `NotFound` | Та же логика | Отличается только текст типа сущности в ошибке `NotFound` (`бронирование` / `booking`) |
| `Application/DependencyInjectionExtensions.cs` | `AddScoped<GetBookingByIdHandler>()` | То же самое | **Файлы идентичны** |

Критические для безопасности места у двух реализаций **совпадают**:

- Маршрут в обоих случаях **`/bookings/{id:guid}`**, в обоих вызывается `.RequireAuthorization()`. С учётом существующей группы `/api` полный путь — `GET /api/bookings/{id:guid}`.
- В обоих вариантах выполняется `FindFirstValue("sub")` с резервным `ClaimTypes.NameIdentifier`, затем `Guid.TryParse`.
- Обе версии получают бронирование через `IBookingRepository.GetByIdAsync(id, cancellationToken)` и сравнивают `booking.UserId` с пользователем из JWT.
- Обе версии разрешают обращение владельцу либо пользователю, которого функция `IsAdmin/IsAdminUser` распознаёт по роли.
- Обе версии используют `EndpointResult<BookingsDto>` и стандартную модель ошибок `Result<T, Error>`.

**Проверкой исходников опровергнуто ранее предполагаемое различие `{id}` против `{id:guid}`:** в архиве «без правил» ограничение `:guid` действительно присутствует. Отсутствие этого уточнения в текстовом ответе агента нельзя было считать отсутствием в его коде.

### Выявленные технические ограничения обеих версий

В обоих файлах административная роль распознаётся через `roleName.Contains("admin", ...)` либо `Contains("админ", ...)`. Такое сопоставление подстроки может ошибочно признать привилегированной постороннюю роль, содержащую эти символы. Для реального сервиса безопаснее проверять точное разрешённое значение роли или явную политику авторизации. Это **общая проблема обеих версий**, а не преимущество одного варианта.

Формат ошибок также не полностью единообразен: без правил часть новых сообщений написана на русском, с правилами — на английском. Различается строковый код `booking.access.denied` / `booking.access_denied`; при наличии клиентов, учитывающих код ошибки, это может быть существенным отличием совместимости. Само по себе наличие английских сообщений не означает улучшения качества.

При проверке кода установлены `.RequireAuthorization()` и условия возврата ошибок. Однако **реальные HTTP-проверки статусов 200/401/403/404 для нового маршрута не выполнялись**. Их корректность в отчётах Codex заявлена и согласуется с читаемыми ветками кода, но динамически не подтверждена.

## 6. Сравнение ответов Codex и результаты сборки

| Критерий | Без правил | С правилами |
|---|---|---|
| Защита маршрута JWT | Да, в исходниках | Да, в исходниках |
| Проверка владельца и администратора | Да | Да |
| Ограничение `{id:guid}` | **Да** | **Да** |
| `AsNoTracking()` | Да | Да |
| `CancellationToken` | Да | Да |
| Затронуто файлов по задаче | 4 | 4 |
| Заявленная сборка с `NuGetAudit=false` | Успешно, 0 ошибок и предупреждений | Успешно, 0 ошибок и предупреждений |
| Обычная сборка | Присутствует существующая проблема `NU1903` | Сообщена ошибка `NU1903` |
| Отдельные тесты нового endpoint | Не запускались / не представлены | Не запускались / не представлены |
| Замечания о соседних незащищённых маршрутах | Не отражены в ответе | Явно перечислены в ответе агента |

Оба агента указали на исходную проблему `Microsoft.OpenApi 2.4.1` (`NU1903`). Успешная сборка через `dotnet build BookingService.sln -p:NuGetAudit=false` подтверждается **только их отчётами**: в этой проверке команда самостоятельно не выполнялась. Флаг `NuGetAudit=false` отключает аудит NuGet, но не исправляет уязвимость.

Текстовые ответы агента: `evidence/codex_step5_without_rules_response.md` и `evidence/codex_step5_with_rules_response.md`. В ответе с правилами Codex дополнительно указал существующие незащищённые операции других маршрутов. Это наблюдаемое отличие **анализа и отчётности**, которое соответствует проектному правилу R15, хотя по одному эксперименту нельзя установить причинную связь однозначно.

## 7. Краткий вывод

Обе версии Codex решили небольшую задачу по одной и той же архитектурной схеме: создали endpoint и handler, расширили репозиторий и зарегистрировали обработчик, предусмотрели JWT и проверку доступа. Сравнение непосредственно исходных файлов **не выявило существенного улучшения безопасности или архитектуры от добавления `AGENTS.md`**. Положительный эффект заметен прежде всего в ответе агента с правилами: он отдельно сообщил об уязвимых соседних маршрутах и ограничил правки рамками задачи. Реализованный код при этом практически совпал.

Таким образом, опыт подтвердил, что правила Codex действительно доступны и полезны как формализованный контрольный список проекта, но в данном сравнении не доказано повышение качества самого сгенерированного кода. Такой результат необходимо учитывать при оценке работы: для доказательства устойчивого преимущества потребовались бы более сложная задача, повторные запуски с одинаковыми параметрами и проверка HTTP-тестами. Отдельные восемь PostgreSQL-тестов, пройденные ранее в шаге 4, относятся **к другой задаче** — защите конкурентного бронирования — и не подтверждают работу этого нового GET-маршрута.

## 8. Доказательные материалы

- `comparison/without_rules/` — **четыре оригинальных `.cs` файла** без правил;
- `comparison/with_rules/` — **четыре оригинальных `.cs` файла** с правилами;
- `comparison/actual_diff.patch` — реальный diff двух вариантов;
- `comparison/sha256.csv` — контрольные суммы каждого файла;
- `evidence/codex_step5_without_rules_response.md`, `evidence/codex_step5_with_rules_response.md` — ответы Codex;
- `AGENTS.md`, `rules.md` — проектные правила, применявшиеся в одном варианте;
- `evidence/codex_agents_rules.png` — подтверждение загрузки правил в Codex.

Сведения о сборке и заявленном поведении зафиксированы по ответам Codex; исходники проверены прямым чтением двух переданных архивов. Секреты из конфигураций Backend в комплект не включены.
