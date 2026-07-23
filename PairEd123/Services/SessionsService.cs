using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class SessionsService
    {
        private readonly DatabaseService _db;
        public SessionsService(DatabaseService db) => _db = db;

        public async Task<(bool Success, string Message)> CreateSessionAsync(
            int requestId, int tutorId, int studentId, string subject, DateTime scheduledTime)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();
                const string sql = @"INSERT INTO sessions (RequestId, TutorId, StudentId, Subject, ScheduledTime, Status)
                                      VALUES (@requestId, @tutorId, @studentId, @subject, @scheduledTime, 'Upcoming');";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@requestId", requestId);
                cmd.Parameters.AddWithValue("@tutorId", tutorId);
                cmd.Parameters.AddWithValue("@studentId", studentId);
                cmd.Parameters.AddWithValue("@subject", subject);
                cmd.Parameters.AddWithValue("@scheduledTime", scheduledTime);
                await cmd.ExecuteNonQueryAsync();
                return (true, "Session created.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        // All sessions where the given user is either the tutor or the student.
        public async Task<List<SessionDisplay>> GetSessionsForUserAsync(int userId)
        {
            var list = new List<SessionDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT s.SessionId, s.RequestId, s.TutorId, s.StudentId, s.Subject, s.ScheduledTime, s.Status, s.CancellationReason,
                                         tutorUser.DisplayName AS TutorName, studentUser.DisplayName AS StudentName
                                  FROM sessions s
                                  JOIN users tutorUser ON tutorUser.UserId = s.TutorId
                                  JOIN users studentUser ON studentUser.UserId = s.StudentId
                                  WHERE s.TutorId = @userId OR s.StudentId = @userId
                                  ORDER BY s.ScheduledTime ASC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int tutorId = reader.GetInt32("TutorId");
                bool isTutorView = tutorId == userId;
                list.Add(new SessionDisplay
                {
                    SessionId = reader.GetInt32("SessionId"),
                    RequestId = reader.GetInt32("RequestId"),
                    TutorId = tutorId,
                    StudentId = reader.GetInt32("StudentId"),
                    Subject = reader.GetString("Subject"),
                    ScheduledTime = reader.GetDateTime("ScheduledTime"),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Upcoming" : reader.GetString("Status"),
                    CancellationReason = reader.IsDBNull(reader.GetOrdinal("CancellationReason")) ? null : reader.GetString("CancellationReason"),
                    OtherPartyName = isTutorView ? reader.GetString("StudentName") : reader.GetString("TutorName"),
                    IsTutorView = isTutorView
                });
            }
            return list;
        }

        public async Task<bool> CancelSessionAsync(int sessionId, string reason)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "UPDATE sessions SET Status = 'Cancelled', CancellationReason = @reason WHERE SessionId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", sessionId);
            cmd.Parameters.AddWithValue("@reason", reason);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> CompleteSessionAsync(int sessionId)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "UPDATE sessions SET Status = 'Completed' WHERE SessionId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", sessionId);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}