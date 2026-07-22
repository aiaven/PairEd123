using MySqlConnector;
using PairedAPI.Data;
using PairedAPI.Models;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace PairedAPI.Services
{
    public class AuthService
    {
        private readonly Database _db;

        public AuthService(Database db)
        {
            _db = db;
        }

        // ✅ Register new user
        public async Task<bool> RegisterUser(User user)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // ✅ Hash the password before saving
            var hasher = new PasswordHasher<User>();
            string hashedPassword = hasher.HashPassword(user, user.PasswordHash);

            var cmd = new MySqlCommand(
                @"INSERT INTO Users (Email, PasswordHash, DisplayName, Role, IsTutor, Availability) 
          VALUES (@Email, @PasswordHash, @DisplayName, @Role, @IsTutor, @Availability)", conn);

            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
            cmd.Parameters.AddWithValue("@DisplayName", user.DisplayName);
            cmd.Parameters.AddWithValue("@Role", string.IsNullOrEmpty(user.Role) ? "Student" : user.Role);
            cmd.Parameters.AddWithValue("@IsTutor", user.IsTutor);
            cmd.Parameters.AddWithValue("@Availability", user.Availability);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateProfile(int userId, string displayName, string availability)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "UPDATE Users SET DisplayName=@DisplayName, Availability=@Availability WHERE UserId=@UserId", conn);

            cmd.Parameters.AddWithValue("@DisplayName", displayName);
            cmd.Parameters.AddWithValue("@Availability", availability);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> OptInAsTutor(int userId, bool isTutor)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "UPDATE Users SET IsTutor=@IsTutor WHERE UserId=@UserId", conn);

            cmd.Parameters.AddWithValue("@IsTutor", isTutor);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<string>> GetSkills(int userId)
        {
            var skills = new List<string>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT SkillName FROM UserSkills WHERE UserId=@UserId", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                skills.Add(reader.GetString("SkillName"));
            }
            return skills;
        }


        public async Task<bool> RemoveSkill(int userId, string skillName)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "DELETE FROM UserSkills WHERE UserId=@UserId AND SkillName=@SkillName", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SkillName", skillName);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }


        public async Task<bool> AddSkill(int userId, string skillName)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "INSERT INTO UserSkills (UserId, SkillName) VALUES (@UserId, @SkillName)", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@SkillName", skillName);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }


        // ✅ Login existing user
        public async Task<User?> LoginUser(string email, string passwordHash)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT UserId, Email, DisplayName, Role, IsTutor, Availability FROM Users WHERE Email=@Email AND PasswordHash=@PasswordHash",
                conn);

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Email = reader.GetString("Email"),
                    DisplayName = reader.GetString("DisplayName"),
                    Role = reader.GetString("Role"),
                    IsTutor = reader.GetBoolean("IsTutor"),
                    Availability = reader.IsDBNull("Availability") ? null : reader.GetString("Availability"),
                    PasswordHash = string.Empty // 🚫 don’t expose password
                };
            }
            return null;
        }
    }
}
