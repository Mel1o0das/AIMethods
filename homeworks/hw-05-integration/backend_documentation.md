# ДЗ №5 — интеграция Backend, PostgreSQL и Frontend

## Результат

В репозиторий перенесён существующий `BookingService` на ASP.NET Core/.NET 10 без переписывания архитектуры. Backend находится в `apps/booking-backend`, Frontend — в `apps/booking-frontend`. Локально подтверждена цепочка PostgreSQL → EF Core migrations → API → JWT → создание, чтение и отмена брони → освобождение интервала.

Mock-режим ДЗ №4 сохранён. Выбор источника данных выполняется через `NEXT_PUBLIC_DATA_MODE=mock|api`.

## Архитектура

| Проект | Ответственность |
|---|---|
| `BookingService.Domain` | сущности, value objects, типизированные идентификаторы и доменные ошибки |
| `BookingService.Application` | minimal API endpoints, сценарии, DTO и интерфейсы репозиториев |
| `BookingService.Infrastructure.Postgres` | EF Core, Npgsql, конфигурации, миграции, репозитории, безопасный dev seed |
| `BookingService.Core` | composition root, JWT, CORS, Serilog, middleware, Swagger и запуск |
| `BookingService.Tests` | unit- и PostgreSQL-интеграционные тесты |

## Сущности и связи

| Сущность | Основные поля | Связи |
|---|---|---|
| `Role` | `RoleId`, `Name` | 1:N с `User` |
| `User` | email, password hash, ФИО, телефон, `RoleId` | N:1 с `Role`, 1:N с `Booking` |
| `ObjectType` | `ObjectTypeId`, `Name` | 1:N с `BookingObject`; связь объекта допускает `NULL` |
| `BookingObject` | название, описание, цена, вместимость, image URL, active/deleted | N:1 с `ObjectType`, 1:N с `Booking` |
| `BookingStatus` | `BookingStatusId`, `Name` | 1:N с `Booking` |
| `Booking` | владелец, объект, начало/конец, гости, комментарий, статус | N:1 с `User`, `BookingObject`, `BookingStatus` |

В БД есть уникальные индексы для email, телефона и имён справочников; внешние ключи используют `Restrict`. Миграция валидации добавляет `start_date_time < end_date_time` и `guests_count > 0`. Пересечение интервалов проверяется внутри транзакции после `FOR UPDATE` выбранного объекта.

## Реальные миграции

1. `20260502150732_Initial`
2. `20260511164628_AddSoftDeleteToBookingObject`
3. `20260518182717_AddObjectTypes`
4. `20261015162933_AddBookingValidationConstraints`

Новые дублирующие миграции не создавались. При старте API вызывается `Database.MigrateAsync()`. Локальная проверка таблицы `__EFMigrationsHistory` подтвердила применение всех четырёх миграций.

## Инфраструктурный выбор

`compose.yaml` запускает два сервиса:

- `db` — закреплённый major-образ `postgres:17`, persistent volume и `pg_isready` healthcheck; порт 5432 на host не публикуется;
- `api` — многоэтапная сборка на официальных .NET 10 SDK/runtime images, непривилегированный пользователь `app`, публикация только `${API_PORT}:8080`.

Отдельные volumes сохраняют БД, изображения и Data Protection keys. Значения БД, JWT, CORS и внешнего URL изображений поступают из `.env`; в Git есть только `.env.example` с заменяемыми значениями.

Dev seed включается через `SEED_DATA_ENABLED=true`. Он идемпотентно добавляет роли `User`/`Administrator`, три статуса, два типа и три объекта. Пароли не зашиты: обычный пользователь создаётся публичным endpoint регистрации, пароль хешируется BCrypt. Администратор автоматически не создаётся.

## API и доступ

Все маршруты имеют префикс `/api`. Успех и ошибки возвращаются JSON envelope `{ result, error, isError, timeGenerated }`.

| Метод и маршрут | Доступ | Операция |
|---|---|---|
| `POST /auth/register` | public | регистрация с серверным назначением роли `User` |
| `POST /auth/login` | public | JWT login |
| `GET /auth/me` | JWT | профиль текущего пользователя |
| `GET /booking-objects` | public | пагинированный каталог/поиск/фильтры |
| `GET /booking-objects/{id}` | public | объект по ID |
| `GET /object-types` | public | типы объектов |
| `GET /booking-statuses` | public | статусы |
| `GET /availability`, `GET /availability/range` | public | занятые интервалы |
| `POST /bookings` | JWT | CREATE брони |
| `GET /bookings/my/{id}` | владелец или admin | READ своих броней |
| `PUT /bookings/{id}/cancel` | владелец или admin | UPDATE через отмену |
| `GET /bookings`, `PUT /bookings/{id}/confirm` | admin | список и подтверждение |
| CRUD справочников/объектов и `GET /users` | admin для изменяющих/закрытых операций | администрирование |

При создании брони backend игнорирует присланные `UserId` и `StatusId`: владелец берётся из JWT `sub`, начальный статус ищется по имени в БД. Регистрация сохраняет совместимость старого DTO, но игнорирует клиентский `RoleId` и назначает найденную в БД роль `User`.

Отмена разрешена владельцу либо роли `Administrator`. Чтение `/bookings/my/{id}` проверяет соответствие URL и JWT. Общий список, подтверждение, изменение объектов/справочников и список пользователей требуют административной роли.

## Ошибки и логирование

Доменные ошибки отображаются в `400/401/403/404/409/500`. JWT challenge/forbidden и необработанные ошибки также возвращают JSON, а не HTML. Повреждённое JSON-тело проверено отдельно: `400`, код `request.invalid_json`.

Serilog пишет старт, HTTP method/path/status/duration и значимые операции (`login`, создание/отмена брони). Пароли и JWT не логируются. В локальных логах подтверждены ответы `200`, `400`, `401`, `403` и `409`.

## CORS и конфигурация

Всегда разрешён `http://localhost:3000`; дополнительный frontend origin задаётся `Cors__AllowedOrigins__0`/`FRONTEND_ORIGIN`. Используются конкретные origins, `AllowAnyOrigin` отсутствует. Credentials не включены: JWT передаётся в `Authorization` header.

Основные переменные:

```text
ConnectionStrings__BookingServiceDb
JwtSettings__SecretKey
JwtSettings__Issuer
JwtSettings__Audience
Cors__AllowedOrigins__0
ImagesPath
ImagesPublicUrl
SeedData__Enabled
```

## Локальный запуск

Из `apps/booking-backend`:

```powershell
Copy-Item .env.example .env
# Заменить change-me значения и задать случайный JWT key длиной 32+ символа.
docker compose up --build -d
docker compose ps
docker compose logs --tail 100 api
```

Frontend в другом терминале:

```powershell
cd apps/booking-frontend
npm ci
$env:NEXT_PUBLIC_DATA_MODE = "api"
$env:NEXT_PUBLIC_API_BASE_URL = "http://localhost:5260/api"
npm run dev
```

Для возврата к ДЗ №4 достаточно `NEXT_PUBLIC_DATA_MODE=mock`; HTTP-запросы реального API в этом режиме блокируются адаптером.

## Проверенные HTTP-примеры

Регистрация (присланный административный `roleId` специально проверяет, что сервер ему не доверяет):

```bash
curl -i -X POST http://localhost:5260/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"hw5.user@example.test","password":"Homework5Test!","lastName":"Test","firstName":"Homework","patronymic":"User","phone":"+79990001122","roleId":"10000000-0000-0000-0000-000000000002"}'
```

Фактический результат: `200`, ID `4adf6823-86b1-4362-9b2f-58306776ecff`; `/auth/me` вернул роль `10000000-0000-0000-0000-000000000001` (`User`).

После login токен передаётся как `Authorization: Bearer <TOKEN>`:

```bash
curl -i http://localhost:5260/api/booking-objects?pageSize=10
curl -i -X POST http://localhost:5260/api/bookings -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" -d @booking.json
curl -i -X PUT http://localhost:5260/api/bookings/<BOOKING_ID>/cancel -H "Authorization: Bearer <TOKEN>"
```

Фактически создана бронь `3adbcec5-a3d4-4215-8034-491a0ada472a`, затем она появилась в «Моих бронированиях», была отменена, а запрос availability вернул 0 занятых интервалов для этого диапазона.

Проверенные отрицательные ответы:

- без токена: `401`, `auth.required`;
- пароль короче 8 символов: `400`, `value.is.invalid`;
- чтение чужих броней: `403`, `booking.read.forbidden`;
- повтор того же интервала: `409`, `booking.interval.conflict`;
- повреждённый JSON: `400`, `request.invalid_json`.

CORS preflight с origin `http://localhost:3000` вернул `204` и `Access-Control-Allow-Origin: http://localhost:3000`.

## Фактические проверки 16.10.2026

| Команда/проверка | Результат |
|---|---|
| `dotnet restore BookingService.sln` | успешно |
| `dotnet test BookingService.sln --no-restore` на host | 6 passed, 8 skipped без test DB |
| полный test suite в одноразовом SDK-контейнере с `booking_service_test` | 14 passed, 0 failed, 0 skipped |
| `npm run lint` | успешно, 0 ошибок |
| `npx tsc --noEmit` | успешно, 0 ошибок |
| `npm test` | 7 файлов, 21 тест — все passed |
| `npm run build`, `NEXT_PUBLIC_DATA_MODE=mock` | успешно, 9 routes |
| `npm run build`, `NEXT_PUBLIC_DATA_MODE=api` | успешно, 9 routes |
| `docker compose config` | успешно; DB без host port |
| `docker compose up --build -d` | PostgreSQL healthy, API запущен на `5260` |
| `/health` | `200` |
| migrations/seed/API/JWT/CRUD/CORS | проверены реальными запросами, результаты приведены выше |

`npm ci` сообщил 12 известных уязвимостей в дереве зависимостей (1 low, 1 moderate, 8 high, 2 critical). Автоматический `npm audit fix --force` не применялся, поскольку он способен внести несовместимые обновления и выходит за рамки минимальной интеграции.

## VPS

Удалённый деплой не выполнялся: доступа к VPS, домена и подтверждённого публичного адреса в этой итерации нет. Воспроизводимая последовательность для VPS:

```bash
git clone <repository-url> AI_Coursework
cd AI_Coursework/apps/booking-backend
cp .env.example .env
# Заполнить .env уникальными production secrets, доменом Frontend и API port.
docker compose config
docker compose up --build -d
docker compose ps
docker compose logs --tail 100 api
curl -i http://127.0.0.1:${API_PORT}/health
```

При обновлении: сделать backup PostgreSQL volume/`pg_dump`, получить новую версию исходников и снова выполнить `docker compose up --build -d`. API применит существующие EF migrations до начала обслуживания запросов. Команды `down -v` и `database drop` в инструкции намеренно отсутствуют. Reverse proxy/TLS и firewall настраиваются отдельно под реальный домен; существующие VPN/другие сервисы трогать не нужно.

