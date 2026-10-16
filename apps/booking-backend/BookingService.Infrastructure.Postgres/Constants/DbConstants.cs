namespace BookingService.Infrastructure.Postgres.Constants
{
    internal static class DbConstants
    {
        public const int MAX_LOW_TEXT_LENGTH = 100;
        public const int MAX_MEDIUM_TEXT_LENGTH = 500;
        public const int MAX_HIGH_TEXT_LENGTH = 2000;
        public const int MAX_EMAIL_LENGTH = 256;
        public const int MAX_PHONE_LENGTH = 20;
        public const int MAX_URL_LENGTH = 512;
        public const int MAX_PASSWORD_HASH_LENGTH = 512;
    }
}
