# TaskFlow API

Небольшой REST API для управления задачами на ASP.NET Core и .NET 10. Данные хранятся только в памяти процесса и сбрасываются при перезапуске приложения.

## Требования

- .NET SDK 10

## Запуск

```bash
dotnet restore
dotnet run --project TaskFlow.Api --urls http://localhost:5000
```

После запуска откройте Swagger UI: <http://localhost:5000/swagger>.

## Endpoints

| Метод | Путь | Назначение |
|---|---|---|
| `GET` | `/api/tasks` | Получить все задачи |
| `GET` | `/api/tasks/{id}` | Получить задачу по GUID |
| `POST` | `/api/tasks` | Создать задачу |
| `PATCH` | `/api/tasks/{id}/status` | Изменить статус задачи |

Пример создания:

```bash
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Подготовить релиз","description":"Составить release notes"}'
```

Пример смены статуса:

```bash
curl -X PATCH http://localhost:5000/api/tasks/<GUID>/status \
  -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}'
```

Допустимые статусы: `New`, `InProgress`, `Completed`. Название обязательно и ограничено 100 символами. Некорректные данные возвращают `400 Bad Request`, задача с отсутствующим ID — `404 Not Found`.

## Проверка

```bash
dotnet restore
dotnet build
dotnet test
```
