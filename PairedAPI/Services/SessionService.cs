using PairedAPI.Models;
using MySqlConnector;

namespace PairedAPI.Services
{
    public class SessionService
    {
        private readonly string _connectionString;

        public SessionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> CreateSession(Session session)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                @"INSERT INTO Sessions (TutorId, StudentId, Subject, ScheduledTime, Status) 
                  VALUES (@TutorId, @StudentId, @Subject, @ScheduledTime, @Status)", conn);

            cmd.Parameters.AddWithValue("@TutorId", session.TutorId);
            cmd.Parameters.AddWithValue("@StudentId", session.StudentId);
            cmd.Parameters.AddWithValue("@Subject", session.Subject);
            cmd.Parameters.AddWithValue("@ScheduledTime", session.ScheduledTime);
            cmd.Parameters.AddWithValue("@Status", session.Status ?? "Pending");

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateSessionStatus(int sessionId, string status)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "UPDATE Sessions SET Status=@Status WHERE SessionId=@SessionId", conn);

            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<Session>> GetSessions(int userId)
        {
            var sessions = new List<Session>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT * FROM Sessions WHERE TutorId=@UserId OR StudentId=@UserId", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sessions.Add(new Session
                {
                    SessionId = reader.GetInt32("SessionId"),
                    TutorId = reader.GetInt32("TutorId"),
                    StudentId = reader.GetInt32("StudentId"),
                    Subject = reader.GetString("Subject"),
                    ScheduledTime = reader.GetDateTime("ScheduledTime"),
                    Status = reader.GetString("Status")
                });
            }
            return sessions;
        }
    }
}
