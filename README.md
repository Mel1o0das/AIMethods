# AI_Coursework — Методы искусственного интеллекта

Учебный монорепозиторий с результатами шести домашних заданий. В первых четырёх работах последовательно прорабатывались применение AI-инструментов, правила работы с ними, UI-концепция и клиентская часть BookingService. В ДЗ №5 к приложению добавлен Backend и проведена интеграция, а в ДЗ №6 настроены автоматические проверки и production-деплой Frontend.

## Опубликованный проект

| Ресурс | Ссылка |
|---|---|
| Репозиторий | [github.com/Mel1o0das/AIMethods](https://github.com/Mel1o0das/AIMethods) |
| Frontend | [booking-service-xi.vercel.app](https://booking-service-xi.vercel.app/) |
| Backend API | [aimethods-production.up.railway.app](https://aimethods-production.up.railway.app/) |
| Health check Backend | [aimethods-production.up.railway.app/health](https://aimethods-production.up.railway.app/health) |
| GitHub Actions | [github.com/Mel1o0das/AIMethods/actions](https://github.com/Mel1o0das/AIMethods/actions) |

Корневой адрес Backend не содержит отдельной HTML-страницы: рабочие маршруты доступны под `/api`, а состояние сервиса проверяется через `/health`.

## Домашние задания

| № | Тема | Результат | Материалы |
|---|---|---|---|
| 1 | Сравнение AI-инструментов | Реализованы и сопоставлены четыре варианта TaskFlow | [Отчёт и исходники](homeworks/hw-01-taskflow/) |
| 2 | AI Rules и промпты | Подготовлены правила, RTCF-шаблоны и результаты экспериментов с Codex | [Материалы ДЗ №2](homeworks/hw-02-ai-rules/) |
| 3 | UI-концепция и техническое задание | Описаны продукт, пользовательские сценарии, три UI-концепции и требования к BookingService | [Материалы ДЗ №3](homeworks/hw-03-ui-spec/) |
| 4 | Frontend | Реализовано клиентское приложение с Mock API, авторизацией, бронированиями и административными экранами | [Frontend и отчёт](apps/booking-frontend/) |
| 5 | Backend и интеграция | Добавлены ASP.NET Core API, PostgreSQL 17, EF Core, JWT и связь Frontend с Backend | [Документация ДЗ №5](homeworks/hw-05-integration/) |
| 6 | CI/CD | Настроены GitHub Actions, проверки Frontend и Backend, Docker build и gated-деплой на Vercel | [Документация ДЗ №6](homeworks/hw-06-cicd/) |

## Возможности BookingService

- каталог объектов размещения, просмотр доступности и создание бронирования;
- регистрация и вход пользователя;
- список собственных бронирований и их отмена;
- роли `User` и `Administrator`;
- подтверждение и отмена бронирований администратором;
- управление объектами размещения;
- два режима Frontend: локальный Mock API и работа с реальным Backend.

## Технологии

**Frontend:** Next.js 16, React 19, TypeScript, Ant Design 6, ESLint, Vitest и Testing Library.

**Backend:** ASP.NET Core на .NET 10, Entity Framework Core, Npgsql, PostgreSQL 17, JWT Bearer Authentication, BCrypt и xUnit.

**Инфраструктура:** Docker, Docker Compose, GitHub Actions, Vercel для Frontend и Railway для Backend с PostgreSQL.

## Структура репозитория

```text
AI_Coursework/
├── .github/
│   └── workflows/
│       └── ci-cd.yml
├── apps/
│   ├── booking-frontend/
│   └── booking-backend/
├── homeworks/
│   ├── hw-01-taskflow/
│   ├── hw-02-ai-rules/
│   ├── hw-03-ui-spec/
│   ├── hw-05-integration/
│   └── hw-06-cicd/
├── .gitignore
└── README.md
```

Материалы ДЗ №4 находятся рядом с реализацией Frontend в `apps/booking-frontend/docs/homework-4/`.

## Запуск локально

Frontend в Mock-режиме:

```powershell
cd apps/booking-frontend
npm ci
$env:NEXT_PUBLIC_DATA_MODE = "mock"
npm run dev
```

Backend с PostgreSQL 17:

```powershell
cd apps/booking-backend
Copy-Item .env.example .env
# Заменить значения change-me в локальном .env.
docker compose up --build
```

При стандартном значении `API_PORT=5260` Backend доступен на `http://localhost:5260`, health check — на `http://localhost:5260/health`. Для подключения Frontend используются `NEXT_PUBLIC_DATA_MODE=api` и `NEXT_PUBLIC_API_BASE_URL=http://localhost:5260/api`; Mock-режим от Backend не зависит.

## Защита и конфигурация

Backend использует JWT для аутентификации. Это собственная JWT Bearer-аутентификация приложения, а не реализация OAuth2. Доступ к административным операциям ограничен ролью `Administrator`; для пользовательских бронирований дополнительно проверяется принадлежность ресурса текущему пользователю. CORS разрешает только настроенные origin, включая production-домен Frontend.

Публичные сервисы доступны по HTTPS через Vercel и Railway. Строка подключения к БД, JWT-ключ и параметры deployment не хранятся в исходном коде: Backend получает секреты из переменных окружения Railway, а workflow — из GitHub Secrets (`VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID`).

## Подтверждённые проверки

На момент итоговой проверки 16 октября 2026 года:

- Frontend: ESLint, TypeScript, production build и **22 теста**;
- Backend: Release build и **14 тестов** с PostgreSQL 17;
- Docker-образ Backend собирается;
- опубликованный Frontend отвечает HTTP 200;
- `/health` опубликованного Backend отвечает HTTP 200 и `Healthy`;
- запрос к публичному API каталога отвечает HTTP 200;
- на странице GitHub Actions отображаются успешные запуски workflow `CI and Production Deploy`, включая запуск для изменения `a400576`.

Подробные команды и границы выполненных проверок приведены в документации [ДЗ №5](homeworks/hw-05-integration/backend_documentation.md) и [ДЗ №6](homeworks/hw-06-cicd/technical_documentation.md).
