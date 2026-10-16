namespace BookingService.Domain.Shared
{
    public static class GeneralError
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            string label = name ?? "значение";
            return Error.Validation("value.is.invalid", $"{label} недействительно", name);
        }

        public static Error NotFound(Guid? id = null, string? name = null)
        {
            string forId = id == null ? string.Empty : $" по Id '{id}'";
            return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
        }

        public static Error ValueIsRequred(string? name = null)
        {
            string label = name == null ? string.Empty : " " + name + " ";
            return Error.Validation("value.is.requered", $"Поле{label}обязательно");
        }

        public static Error AlreadyExist()
        {
            return Error.Validation("record.already.exist", $"Запись уже существует");
        }

        public static Error Failure(string? message = null)
        {
            return Error.Validation("server.failure", message ?? "Серверная ошибка");
        }
    }
}