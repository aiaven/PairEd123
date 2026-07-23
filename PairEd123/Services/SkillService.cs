using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class SkillsService
    {
        private readonly DatabaseService _db;
        public SkillsService(DatabaseService db) => _db = db;

        // ---- Catalog (admin-managed) ----
        public async Task<List<Skill>> GetCatalogAsync()
        {
            var list = new List<Skill>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "SELECT SkillId, SkillName FROM skills ORDER BY SkillName;";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(new Skill { SkillId = reader.GetInt32("SkillId"), SkillName = reader.GetString("SkillName") });
            return list;
        }

        public async Task<(bool Success, string Message)> AddToCatalogAsync(string skillName)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();
                const string sql = "INSERT INTO skills (SkillName) VALUES (@name);";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", skillName.Trim());
                await cmd.ExecuteNonQueryAsync();
                return (true, "Skill added.");
            }
            catch (MySqlException ex) when (ex.Number == 1062) // unique constraint violation
            {
                return (false, "That skill already exists.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFromCatalogAsync(int skillId)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "DELETE FROM skills WHERE SkillId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", skillId);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---- Tutor's own assigned skills ----
        public async Task<List<UserSkill>> GetSkillsForTutorAsync(int userId)
        {
            var list = new List<UserSkill>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "SELECT SkillId, UserId, SkillName FROM userskills WHERE UserId = @userId ORDER BY SkillName;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(new UserSkill
                {
                    SkillId = reader.GetInt32("SkillId"),
                    UserId = reader.GetInt32("UserId"),
                    SkillName = reader.GetString("SkillName")
                });
            return list;
        }

        public async Task<(bool Success, string Message)> AddSkillToTutorAsync(int userId, string skillName)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();

                const string checkSql = "SELECT COUNT(*) FROM userskills WHERE UserId = @userId AND SkillName = @name;";
                await using (var checkCmd = new MySqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@userId", userId);
                    checkCmd.Parameters.AddWithValue("@name", skillName);
                    var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                    if (count > 0)
                        return (false, "You already added this skill.");
                }

                const string insertSql = "INSERT INTO userskills (UserId, SkillName) VALUES (@userId, @name);";
                await using var insertCmd = new MySqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@userId", userId);
                insertCmd.Parameters.AddWithValue("@name", skillName);
                await insertCmd.ExecuteNonQueryAsync();
                return (true, "Skill added.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        public async Task<bool> RemoveSkillFromTutorAsync(int userSkillRowId)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "DELETE FROM userskills WHERE SkillId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userSkillRowId);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}