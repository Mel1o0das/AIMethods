# TaskFlow API

Небольшой REST API для управления задачами. Данные хранятся в потокобезопасной памяти процесса (без PostgreSQL и без авторизации).

## Требования

- .NET SDK 10

## Запуск

Из корня репозитория:

```bash
dotnet restore
dotnet run --project TaskFlow.Api
```

Приложение слушает:

- http://localhost:5133
- https://localhost:7086

Swagger UI: [http://localhost:5133/swagger](http://localhost:5133/swagger)

OpenAPI-спецификация: [http://localhost:5133/swagger/v1/swagger.json](http://localhost:5133/swagger/v1/swagger.json)

## Endpoints

| Метод | URL | Описание | Коды ответа |
| --- | --- | --- | --- |
| GET | `/api/tasks` | Список всех задач | 200 |
| GET | `/api/tasks/{id}` | Задача по ID | 200, 404 |
| POST | `/api/tasks` | Создание задачи | 201, 400 |
| PATCH | `/api/tasks/{id}/status` | Смена статуса | 200, 400, 404 |

Статусы: `New`, `InProgress`, `Completed`.

Валидация: `title` обязателен, максимум 100 символов.

## Примеры запросов

Создание задачи:

```bash
curl -X POST http://localhost:5133/api/tasks ^
  -H "Content-Type: application/json" ^
  -d "{\"title\":\"Prepare TaskFlow demo\",\"description\":\"Create REST API endpoints\"}"
```

Получение всех задач:

```bash
curl http://localhost:5133/api/tasks
```

Смена статуса (подставьте `id` из ответа POST):

```bash
curl -X PATCH http://localhost:5133/api/tasks/<id>/status ^
  -H "Content-Type: application/json" ^
  -d "{\"status\":\"InProgress\"}"
```

Несуществующая задача вернёт `404`. Пустое название — `400`.

## Тесты

```bash
dotnet test
```

Покрыто:

- успешное создание задачи;
- ошибка при пустом названии;
- успешное изменение статуса.
