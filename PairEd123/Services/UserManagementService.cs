using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class UserManagementService
    {
        private readonly DatabaseService _db;
        public UserManagementService(DatabaseService db) => _db = db;

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT UserId, Email, DisplayName, IsTutor, Role
                                  FROM users ORDER BY DisplayName;";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Email = reader.GetString("Email"),
                    DisplayName = reader.GetString("DisplayName"),
                    IsTutor = !reader.IsDBNull(reader.GetOrdinal("IsTutor")) && reader.GetBoolean("IsTutor"),
                    Role = reader.GetString("Role")
                });
            }
            return users;
        }

        public async Task<bool> UpdateRoleAsync(int userId, string newRole)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "UPDATE users SET Role = @role WHERE UserId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@role", newRole);
            cmd.Parameters.AddWithValue("@id", userId);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
