using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class RequestsService
    {
        private readonly DatabaseService _db;
        public RequestsService(DatabaseService db) => _db = db;

        public async Task<(bool Success, string Message)> CreateRequestAsync(
            int tuteeId, int tutorId, string subject, DateTime preferredDate)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();
                const string sql = @"INSERT INTO requests (TuteeId, TutorId, Subject, PreferredDate)
                                      VALUES (@tuteeId, @tutorId, @subject, @preferredDate);";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tuteeId", tuteeId);
                cmd.Parameters.AddWithValue("@tutorId", tutorId);
                cmd.Parameters.AddWithValue("@subject", subject);
                cmd.Parameters.AddWithValue("@preferredDate", preferredDate);
                await cmd.ExecuteNonQueryAsync();
                return (true, "Request sent.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        // Requests I sent (I am the tutee). Shows the tutor's name.
        public async Task<List<RequestDisplay>> GetSentRequestsAsync(int tuteeId)
        {
            var list = new List<RequestDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT r.RequestId, r.TuteeId, r.TutorId, r.Subject, r.PreferredDate, r.Status, r.RejectionReason,
                                         u.DisplayName AS OtherPartyName
                                  FROM requests r
                                  JOIN users u ON u.UserId = r.TutorId
                                  WHERE r.TuteeId = @tuteeId
                                  ORDER BY r.RequestId ASC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tuteeId", tuteeId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(ReadRequestDisplay(reader));
            return list;
        }

        // Requests sent to me (I am the tutor). Shows the student's name.
        public async Task<List<RequestDisplay>> GetIncomingRequestsAsync(int tutorId)
        {
            var list = new List<RequestDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT r.RequestId, r.TuteeId, r.TutorId, r.Subject, r.PreferredDate, r.Status, r.RejectionReason,
                                 u.DisplayName AS OtherPartyName
                          FROM requests r
                          JOIN users u ON u.UserId = r.TuteeId
                          WHERE r.TutorId = @tutorId AND r.Status = 'Pending'
                          ORDER BY r.RequestId ASC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tutorId", tutorId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(ReadRequestDisplay(reader));
            return list;
        }

        public async Task<bool> AcceptRequestAsync(int requestId)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "UPDATE requests SET Status = 'Accepted', RejectionReason = NULL WHERE RequestId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", requestId);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeclineRequestAsync(int requestId, string reason)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = "UPDATE requests SET Status = 'Declined', RejectionReason = @reason WHERE RequestId = @id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", requestId);
            cmd.Parameters.AddWithValue("@reason", reason);
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        private static RequestDisplay ReadRequestDisplay(MySqlDataReader reader) => new()
        {
            RequestId = reader.GetInt32("RequestId"),
            TuteeId = reader.GetInt32("TuteeId"),
            TutorId = reader.GetInt32("TutorId"),
            Subject = reader.GetString("Subject"),
            PreferredDate = reader.GetDateTime("PreferredDate"),
            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Pending" : reader.GetString("Status"),
            RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason")) ? null : reader.GetString("RejectionReason"),
            OtherPartyName = reader.GetString("OtherPartyName")
        };
    }
}