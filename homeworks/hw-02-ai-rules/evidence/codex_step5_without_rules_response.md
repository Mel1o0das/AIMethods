# Ответ Codex: без AGENTS.md — GET /api/bookings/{id}

> Это перенесённый текст ответа агента, предоставленный для сравнения. Абсолютные ссылки на локальный компьютер заменены относительными путями к проекту. **Полный код изменённых файлов и diff в ответе отсутствуют.**

Добавлен защищённый маршрут `GET /api/bookings/{id}`.

Правила доступа:

- без JWT — `401 Unauthorized`;
- владелец бронирования — `200 OK`;
- администратор — `200 OK`;
- другой пользователь — `403 Forbidden`;
- отсутствующее бронирование — `404 Not Found`.

Изменённые файлы:

- `BookingService.Application/Features/Bookings/GetById.cs`;
- `BookingService.Application/Features/Bookings/IBookingRepository.cs`;
- `BookingService.Application/DependencyInjectionExtensions.cs`;
- `BookingService.Infrastructure.Postgres/Repositories/BookingRepository.cs`.

Проверка:

```text
dotnet build BookingService.sln -p:NuGetAudit=false

Сборка успешно завершена.
Предупреждений: 0
Ошибок: 0
```

Флаг отключает только NuGet-аудит существующей уязвимой зависимости `Microsoft.OpenApi 2.4.1`; код проекта собран полностью. Схема базы данных не изменялась.

**В представленном ответе нет кода `GetById.cs`, списка точных diff-изменений и сведений о запуске автоматических тестов.**
