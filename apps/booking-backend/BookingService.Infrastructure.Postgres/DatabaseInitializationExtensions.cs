using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Postgres;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool seedData)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookingServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        if (!seedData)
            return;

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO roles (role_id, name) VALUES
                ('10000000-0000-0000-0000-000000000001', 'User'),
                ('10000000-0000-0000-0000-000000000002', 'Administrator')
            ON CONFLICT (name) DO NOTHING;

            INSERT INTO booking_statuses (booking_status_id, name) VALUES
                ('20000000-0000-0000-0000-000000000001', 'Ожидает подтверждения'),
                ('20000000-0000-0000-0000-000000000002', 'Подтверждено'),
                ('20000000-0000-0000-0000-000000000003', 'Отменено')
            ON CONFLICT (name) DO NOTHING;

            INSERT INTO object_types (object_type_id, name) VALUES
                ('30000000-0000-0000-0000-000000000001', 'Баня'),
                ('30000000-0000-0000-0000-000000000002', 'Глэмпинг')
            ON CONFLICT (name) DO NOTHING;

            INSERT INTO booking_objects (
                booking_object_id, name, description, price, capacity, image_url,
                is_active, is_deleted, object_type_id, created_at)
            VALUES
                ('40000000-0000-0000-0000-000000000001', 'Лесная баня', 'Баня у соснового леса', 4500, 6, NULL, TRUE, FALSE, '30000000-0000-0000-0000-000000000001', NOW()),
                ('40000000-0000-0000-0000-000000000002', 'Панорамный купол', 'Глэмпинг с панорамным окном', 7900, 4, NULL, TRUE, FALSE, '30000000-0000-0000-0000-000000000002', NOW()),
                ('40000000-0000-0000-0000-000000000003', 'Семейный шатёр', 'Просторный глэмпинг для семьи', 6500, 5, NULL, TRUE, FALSE, '30000000-0000-0000-0000-000000000002', NOW())
            ON CONFLICT (booking_object_id) DO NOTHING;
            """);
    }
}
