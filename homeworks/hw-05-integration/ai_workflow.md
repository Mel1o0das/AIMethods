# ДЗ №5 — процесс работы с Codex

## Исходный промпт

Пользователь передал расширенное задание: перенести существующий .NET 10 Backend из `../FourthTask/backend/BookingService` в репозиторий, не ломать Frontend и Mock ДЗ №4, добавить PostgreSQL/Docker, проверить JWT/CRUD/CORS/end-to-end, подготовить локальную и VPS-инструкции и сохранить только реальные результаты. Отдельно запрещены выдуманные файлы/результаты, секреты в Git, разрушительные операции с БД, новый дизайн, новые фреймворки, коммит/push и самостоятельное изменение тега `hw4-complete`.

## Выполненные итерации

1. Проверены Git (`main` на теге `hw4-complete`, чистое исходное состояние), локальные `AGENTS.md`, Frontend и доступность исходного Backend.
2. Выполнен аудит сущностей, migrations, minimal API endpoints, JWT и репозиториев. Найдены два критичных разрыва: регистрация доверяла `RoleId`, чтение «моих броней» доверяло ID из URL и не требовало JWT.
3. Исходники перенесены с исключением `.git`, `.idea`, `.vs`, `bin`, `obj`, Data Protection keys, логов, `.env` и исходного локального `appsettings.json`.
4. Добавлены безопасная конфигурация, конкретный CORS, JSON challenge/forbidden/exception responses, health endpoint, автоматическое применение migrations и идемпотентный dev seed без пользовательских паролей.
5. Добавлены multi-stage Dockerfile, `compose.yaml`, `.dockerignore`, `.env.example`. PostgreSQL не публикует 5432; API port задаётся переменной.
6. Backend перестал доверять клиентским `UserId`, `StatusId` и `RoleId`; закрыты owner/admin и admin-only endpoints. Существующие маршруты сохранены.
7. Frontend API adapter согласован с фактическим массивом `error.messages`; регистрация больше не запрашивает/не отправляет выбранную клиентом роль. Mock adapter сохранён. После последующего добавления регрессионной проверки форматирования дат актуальный набор Frontend содержит 22 теста.
8. Первый host `dotnet test --no-restore` честно завершился ошибкой отсутствующего `project.assets.json`; sandbox restore упёрся в сеть. После разрешённого NuGet restore зависимости восстановлены.
9. Первая Release-сборка Docker обнаружила анализатор `CA1849` для синхронных `Run`/`CloseAndFlush`; вызовы заменены на async, повторная multi-stage сборка успешна.
10. Реальные HTTP-проверки обнаружили, что повреждённый JSON классифицировался общим handler как `500`; `BadHttpRequestException` выделен в предсказуемый JSON `400`, повторная проверка успешна.
11. PostgreSQL/API подняты отдельным Compose project `ai-coursework-hw5`; подтверждены четыре migrations, seed, registration/login, JWT ownership, create/read/cancel, конфликт `409`, `400/401/403`, освобождение availability, CORS и Serilog request logs.
12. Для полного `dotnet test` создана отдельная БД `booking_service_test`; одноразовый SDK-контейнер в Compose network выполнил 14/14 тестов без skip.

## Принятые ограничения

- На этапе первоначальной реализации ДЗ №5 VPS не использовался. В итоговой версии проекта Backend опубликован на Railway; его `/health` и публичный API проверены отдельно перед сдачей.
- Администратор не seeded с известным паролем; production admin должен создаваться отдельной доверенной процедурой вне публичной регистрации.
- `npm audit fix --force` не выполнялся из-за риска breaking changes; вывод `npm ci` о 12 уязвимостях сохранён в отчёте.
- Контейнеры других проектов, включая существующий `booking-hw2-pg`, не изменялись.
- Коммит, push и изменение тега не выполнялись.

