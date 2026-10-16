# Домашние работы по методам искусственного интеллекта

Репозиторий содержит результаты домашних заданий №1–4 и исходный код учебных приложений. Материалы каждой работы размещены отдельно; Frontend сервиса BookingService находится в `apps/booking-frontend/`.

## Содержание

| Работа | Тема | Материалы |
|---|---|---|
| №1 | Сравнение AI-инструментов на примере TaskFlow | [Отчёт](homeworks/hw-01-taskflow/hw1_report.md), [исходники четырёх вариантов](homeworks/hw-01-taskflow/projects/), [скриншоты](homeworks/hw-01-taskflow/evidence/) |
| №2 | AI Rules, RTCF-промпты и эксперименты с Codex | [Анализ проекта](homeworks/hw-02-ai-rules/hw_ai_rules.md), [правила](homeworks/hw-02-ai-rules/rules.md), [шаблоны](homeworks/hw-02-ai-rules/prompt_templates.md), [методология](homeworks/hw-02-ai-rules/prompt_methodology.md), [сравнение](homeworks/hw-02-ai-rules/testing.md) |
| №3 | UI-концепции и техническое задание BookingService | [Описание продукта](homeworks/hw-03-ui-spec/project_description.md), [три UI-концепции](homeworks/hw-03-ui-spec/ui_concepts/ui_description.md), [User Stories](homeworks/hw-03-ui-spec/user_stories.md), [техническое задание](homeworks/hw-03-ui-spec/technical_specification.md) |
| №4 | Разработка Frontend с помощью Codex | [Frontend и инструкция запуска](apps/booking-frontend/README.md), [отчёт о разработке](apps/booking-frontend/development_report.md), [промпты и скриншоты](apps/booking-frontend/docs/homework-4/) |
| №5 | Backend, PostgreSQL и end-to-end интеграция | [документация Backend и проверок](homeworks/hw-05-integration/backend_documentation.md), [процесс работы с Codex](homeworks/hw-05-integration/ai_workflow.md), [исходники Backend](apps/booking-backend/) |

## Структура репозитория

```text
AI_Coursework/
├── apps/
│   ├── booking-frontend/          # Next.js, React, TypeScript, Mock/API
│   └── booking-backend/           # ASP.NET Core/.NET 10, EF Core, PostgreSQL
├── homeworks/
│   ├── hw-01-taskflow/            # Отчёт, исходники, доказательства
│   ├── hw-02-ai-rules/            # Правила, промпты, сравнение, доказательства
│   ├── hw-03-ui-spec/             # Макеты, истории, критерии, ТЗ
│   └── hw-05-integration/         # Backend/API/DB документация и AI workflow
├── .gitignore
└── README.md
```

## Запуск Frontend для ДЗ №4

Необходимы совместимая версия Node.js и npm. Из корня репозитория в PowerShell:

```powershell
cd apps/booking-frontend
npm ci
$env:NEXT_PUBLIC_DATA_MODE = "mock"
npm run dev
```

Приложение доступно по адресу `http://localhost:3000` (если порт свободен). В режиме `mock` реальные запросы на создание бронирований в Backend не выполняются.

Проверки:

```powershell
npm run lint
npx tsc --noEmit
npm test
npm run build
```

Остальные детали запуска и результаты разработки приведены в [README Frontend](apps/booking-frontend/README.md) и [отчёте ДЗ №4](apps/booking-frontend/development_report.md).
