# TaskFlow API

REST API на ASP.NET Core / .NET 10 для управления задачами. Данные хранятся в памяти и теряются при остановке приложения.

## Запуск

Требуется .NET SDK 10.

```powershell
dotnet restore TaskFlow.slnx
dotnet run --project TaskFlow.Api --launch-profile http
```

Откройте Swagger UI: **http://localhost:5005/swagger**. Описание OpenAPI доступно по адресу **http://localhost:5005/swagger/v1/swagger.json**. В Swagger UI можно отправлять запросы через кнопку **Try it out**.

## Маршруты

| Метод | Адрес | Результат |
| --- | --- | --- |
| `GET` | `/api/tasks` | Все задачи (`200`) |
| `GET` | `/api/tasks/{id}` | Задача (`200`), отсутствует (`404`) |
| `POST` | `/api/tasks` | Новая задача (`201`), неверные данные (`400`) |
| `PATCH` | `/api/tasks/{id}/status` | Изменённая задача (`200`), неверные данные (`400`), отсутствует (`404`) |

`POST` принимает JSON вида:

```json
{"title":"Prepare demo","description":"Check API"}
```

`title` обязателен и не может быть длиннее 100 символов. `description` можно пропустить. Новый объект получает статус `New`, GUID и время создания в UTC. Для `PATCH` передайте один из статусов `New`, `InProgress`, `Completed`:

```json
{"status":"Completed"}
```

Для проверки без браузера есть файл `TaskFlow.Api/TaskFlow.Api.http`. После создания задачи скопируйте её `id` в переменную `taskId` для запросов по ID.

## Сборка и тесты

```powershell
dotnet restore TaskFlow.slnx
dotnet build TaskFlow.slnx --no-restore
dotnet test TaskFlow.slnx --no-build
```
