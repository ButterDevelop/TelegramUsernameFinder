using Microsoft.Extensions.Configuration;

namespace TelegramUsernameFinder
{
    public class Config
    {
        private static readonly IConfiguration _configuration;

        static Config()
        {
            // Билдер конфигураций
            var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
        }

        // MONGO PART
        public static string MONGO_CONNECTION_STRING => _configuration["ConnectionStrings:MongoConnectionString"] ?? "";
        public static string MONGO_DATABASE_NAME     => _configuration["ConnectionStrings:MongoDatabaseName"] ?? "";
        public static string MONGO_USERNAME_PATH     => _configuration["MongoTableNames:MONGO_USERNAME_PATH"] ?? "";
    }
}
