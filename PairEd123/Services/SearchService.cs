using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class SearchService
    {
        private readonly DatabaseService _db;

        public SearchService(DatabaseService db) => _db = db;

        public async Task<List<TutorSearchResult>> GetAllTutorsAsync()
        {
            var results = new List<TutorSearchResult>();

            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"
                SELECT u.UserId, u.DisplayName,
                       GROUP_CONCAT(s.SkillName ORDER BY s.SkillName SEPARATOR ', ') AS Skills
                FROM users u
                LEFT JOIN userskills s ON s.UserId = u.UserId
                WHERE u.IsTutor = 1
                GROUP BY u.UserId, u.DisplayName
                ORDER BY u.DisplayName;";

            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new TutorSearchResult
                {
                    UserId = reader.GetInt32("UserId"),
                    DisplayName = reader.GetString("DisplayName"),
                    Skills = reader.IsDBNull(reader.GetOrdinal("Skills")) ? "" : reader.GetString("Skills")
                });
            }
            return results;
        }
    }
}