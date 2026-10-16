# Домашнее задание №6. CI/CD

## Что настроено

Workflow `.github/workflows/ci-cd.yml` запускается при каждом pull request в `main` и при push в `main`.

Обязательные проверки выполняются независимо:

- `Frontend`: `npm ci`, ESLint, проверка TypeScript, 21 тест Vitest и production-сборка в режимах Mock и API;
- `Backend`: восстановление зависимостей, Release-сборка и 14 тестов на .NET 10;
- `Backend Docker image`: сборка production-образа из существующего `Dockerfile`.

Для интеграционных Backend-тестов workflow поднимает PostgreSQL 17 как service container. Используются только тестовые база и учётные данные: `booking_service_test` / `booking_test`. Строка подключения передаётся через `BOOKING_SERVICE_TEST_CONNECTION_STRING` и не относится к production.

Job `Deploy Frontend to Vercel Production` запускается только для события `push` в ветку `main` и только после успешного завершения всех трёх обязательных проверок. При ошибке любой проверки production-деплой не запускается.

## Локальная проверка

Frontend:

```powershell
Set-Location apps/booking-frontend
npm ci
npm run lint
npx --no-install tsc --noEmit
npm test
$env:NEXT_PUBLIC_DATA_MODE = "mock"
npm run build
$env:NEXT_PUBLIC_DATA_MODE = "api"
$env:NEXT_PUBLIC_API_BASE_URL = "https://api.example.invalid/api"
npm run build
Remove-Item Env:NEXT_PUBLIC_DATA_MODE
Remove-Item Env:NEXT_PUBLIC_API_BASE_URL
```

Backend с отдельным временным PostgreSQL 17:

```powershell
docker run --detach --name booking-hw6-ci-pg --publish 55433:5432 `
  --env POSTGRES_DB=booking_service_test `
  --env POSTGRES_USER=booking_test `
  --env POSTGRES_PASSWORD=ci_test_password_not_for_production `
  --health-cmd="pg_isready -U booking_test -d booking_service_test" `
  --health-interval=2s --health-timeout=5s --health-retries=15 `
  postgres:17
do {
  Start-Sleep -Seconds 2
  $health = docker inspect --format='{{.State.Health.Status}}' booking-hw6-ci-pg
} until ($health -eq "healthy")
Set-Location apps/booking-backend
$env:BOOKING_SERVICE_TEST_CONNECTION_STRING = "Host=localhost;Port=55433;Database=booking_service_test;Username=booking_test;Password=ci_test_password_not_for_production"
dotnet restore BookingService.sln
dotnet build BookingService.sln --configuration Release --no-restore
dotnet test BookingService.sln --configuration Release --no-build --no-restore --logger "console;verbosity=normal"
Remove-Item Env:BOOKING_SERVICE_TEST_CONNECTION_STRING
Set-Location ../..
docker rm --force booking-hw6-ci-pg
```

Docker-образ Backend:

```powershell
docker build --file apps/booking-backend/Dockerfile --tag booking-service-api:ci apps/booking-backend
```

## Настройка GitHub и Vercel

В GitHub в `Settings` → `Secrets and variables` → `Actions` необходимо добавить repository secrets:

- `VERCEL_TOKEN` — токен Vercel с доступом к production-проекту;
- `VERCEL_ORG_ID` — идентификатор аккаунта или команды Vercel;
- `VERCEL_PROJECT_ID` — идентификатор существующего проекта `booking-service`.

Значения `VERCEL_ORG_ID` и `VERCEL_PROJECT_ID` можно получить из `.vercel/project.json`: из корня монорепозитория выполнить `vercel link` и выбрать существующий проект `booking-service`. Файл `.vercel` уже исключён из Git и не должен коммититься.

В настройках Vercel проекта `booking-service` Root Directory должен оставаться `apps/booking-frontend`. Production-переменные Frontend, включая `NEXT_PUBLIC_DATA_MODE` и `NEXT_PUBLIC_API_BASE_URL`, настраиваются в Vercel; команда `vercel pull` получает переменные и настройки существующего проекта перед production-сборкой.

Vercel CLI запускается из корня монорепозитория. Путь `apps/booking-frontend` не передаётся повторно через `--cwd`: его применяет настройка Root Directory, полученная командой `vercel pull`. Это предотвращает удвоение пути к приложению.

```text
vercel pull --yes --environment=production ...
vercel build --prod ...
vercel deploy --prebuilt --prod ...
```

Стандартный Git-деплой Vercel пока остаётся включённым. Его следует отключать только после добавления secrets и подтверждения успешного полного GitHub Actions run. До этого момента наличие workflow означает лишь готовность конфигурации, а не подтверждённую работоспособность CI/CD.

## Результаты локальной проверки

Проверено 16 октября 2026 года:

- ESLint и TypeScript — успешно;
- Vitest — 7 файлов, 21 из 21 теста успешно;
- production-сборка Frontend — успешно в режимах Mock и API;
- .NET 10 Release build — успешно, 0 предупреждений и 0 ошибок;
- Backend — 14 из 14 тестов успешно с PostgreSQL 17;
- Docker-образ `booking-service-api:ci` — успешно собран.

Локальный `npm ci` также сообщил о 12 уязвимостях зависимостей по данным `npm audit` (1 low, 1 moderate, 8 high, 2 critical). Автоматический `npm audit fix` не выполнялся, чтобы не вносить не относящиеся к CI/CD изменения зависимостей.

Удалённый GitHub Actions run и production-деплой Vercel локальной проверкой не подтверждаются. Для их проверки необходимо сначала добавить GitHub Secrets, затем выполнить push в `main` или открыть pull request.
