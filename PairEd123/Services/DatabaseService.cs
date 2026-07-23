using MySqlConnector;

namespace PairEd123.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = BuildConnectionString();
        }

        private static string BuildConnectionString()
        {
            string server = DeviceInfo.Platform == DevicePlatform.Android
                ? "10.0.2.2"
                : "localhost";

            return $"Server={server};Database=paired_db;User ID=root;Password=paireduser;Connection Timeout=10;";
        }

        public async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}