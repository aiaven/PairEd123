using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class AuthService
    {
        private readonly DatabaseService _db;

        public AuthService(DatabaseService db) => _db = db;

        public async Task<(bool Success, string Message)> RegisterAsync(
            string email, string password, string displayName, bool isTutor)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();

                const string checkSql = "SELECT COUNT(*) FROM users WHERE Email = @email;";
                await using (var checkCmd = new MySqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@email", email);
                    var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                    if (count > 0)
                        return (false, "Email already in use.");
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                const string insertSql = @"
                    INSERT INTO users (Email, PasswordHash, DisplayName, IsTutor, Role)
                    VALUES (@email, @passwordHash, @displayName, @isTutor, @role);";

                await using var insertCmd = new MySqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@email", email);
                insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
                insertCmd.Parameters.AddWithValue("@displayName", displayName);
                insertCmd.Parameters.AddWithValue("@isTutor", isTutor);
                insertCmd.Parameters.AddWithValue("@role", isTutor ? "Tutor" : "Student");

                await insertCmd.ExecuteNonQueryAsync();
                return (true, "Registration successful.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            await using var conn = await _db.GetOpenConnectionAsync();

            const string sql = @"SELECT UserId, Email, PasswordHash, DisplayName, ProfilePicture, IsTutor, Availability, Bio, Role
                                  FROM users WHERE Email = @email;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            string storedHash = reader.GetString("PasswordHash");
            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                return null;

            return MapUser(reader, storedHash);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            await using var conn = await _db.GetOpenConnectionAsync();

            const string sql = @"SELECT UserId, Email, PasswordHash, DisplayName, ProfilePicture, IsTutor, Availability, Bio, Role
                                  FROM users WHERE UserId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return MapUser(reader, reader.GetString("PasswordHash"));
        }

        private static User MapUser(MySqlDataReader reader, string passwordHash) => new()
        {
            UserId = reader.GetInt32("UserId"),
            Email = reader.GetString("Email"),
            PasswordHash = passwordHash,
            DisplayName = reader.GetString("DisplayName"),
            ProfilePicture = reader.IsDBNull(reader.GetOrdinal("ProfilePicture")) ? null : reader.GetString("ProfilePicture"),
            IsTutor = !reader.IsDBNull(reader.GetOrdinal("IsTutor")) && reader.GetBoolean("IsTutor"),
            Availability = reader.IsDBNull(reader.GetOrdinal("Availability")) ? null : reader.GetString("Availability"),
            Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString("Bio"),
            Role = reader.GetString("Role")
        };

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            int userId, string displayName, string? profilePicturePath, bool isTutor, string? availability, string? bio)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();

                const string sql = @"
                    UPDATE users
                    SET DisplayName = @displayName, ProfilePicture = @profilePicture, IsTutor = @isTutor,
                        Role = @role, Availability = @availability, Bio = @bio
                    WHERE UserId = @userId;";

                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@displayName", displayName);
                cmd.Parameters.AddWithValue("@profilePicture", (object?)profilePicturePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@isTutor", isTutor);
                cmd.Parameters.AddWithValue("@role", isTutor ? "Tutor" : "Student");
                cmd.Parameters.AddWithValue("@availability", (object?)availability ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bio", (object?)bio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@userId", userId);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0 ? (true, "Profile updated.") : (false, "No matching user found.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }
    }
}