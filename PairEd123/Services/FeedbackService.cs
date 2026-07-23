using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using PairEd123.Models;

namespace PairEd123.Services
{
    public class FeedbackService
    {
        private readonly DatabaseService _db;
        public FeedbackService(DatabaseService db) => _db = db;

        // Completed sessions for this user that don't yet have feedback authored by them.
        public async Task<List<SessionToReview>> GetSessionsToReviewAsync(int userId)
        {
            var list = new List<SessionToReview>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT s.SessionId, s.Subject, s.ScheduledTime, s.TutorId, s.StudentId,
                                         tutorUser.DisplayName AS TutorName, studentUser.DisplayName AS StudentName
                                  FROM sessions s
                                  JOIN users tutorUser ON tutorUser.UserId = s.TutorId
                                  JOIN users studentUser ON studentUser.UserId = s.StudentId
                                  WHERE (s.TutorId = @userId OR s.StudentId = @userId)
                                    AND s.Status = 'Completed'
                                    AND NOT EXISTS (
                                        SELECT 1 FROM feedback f
                                        WHERE f.SessionId = s.SessionId AND f.AuthorId = @userId
                                    )
                                  ORDER BY s.ScheduledTime DESC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int tutorId = reader.GetInt32("TutorId");
                bool isTutorView = tutorId == userId;
                list.Add(new SessionToReview
                {
                    SessionId = reader.GetInt32("SessionId"),
                    Subject = reader.GetString("Subject"),
                    ScheduledTime = reader.GetDateTime("ScheduledTime"),
                    TutorId = tutorId,
                    TuteeId = reader.GetInt32("StudentId"),
                    OtherPartyName = isTutorView ? reader.GetString("StudentName") : reader.GetString("TutorName")
                });
            }
            return list;
        }

        public async Task<(bool Success, string Message)> SubmitFeedbackAsync(
            int tutorId, int tuteeId, int sessionId, int authorId, int rating, string? comment)
        {
            try
            {
                await using var conn = await _db.GetOpenConnectionAsync();
                const string sql = @"INSERT INTO feedback (TutorId, TuteeId, SessionId, AuthorId, Rating, Comment)
                                      VALUES (@tutorId, @tuteeId, @sessionId, @authorId, @rating, @comment);";
                await using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tutorId", tutorId);
                cmd.Parameters.AddWithValue("@tuteeId", tuteeId);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);
                cmd.Parameters.AddWithValue("@authorId", authorId);
                cmd.Parameters.AddWithValue("@rating", rating);
                cmd.Parameters.AddWithValue("@comment", (object?)comment ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
                return (true, "Feedback submitted.");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return (false, "You've already left feedback for this session.");
            }
            catch (MySqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        // Feedback I wrote.
        public async Task<List<FeedbackDisplay>> GetGivenAsync(int userId)
        {
            var list = new List<FeedbackDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT f.FeedbackId, f.TutorId, f.TuteeId, f.SessionId, f.AuthorId, f.Rating, f.Comment,
                                         s.Subject,
                                         subjectUser.DisplayName AS SubjectName
                                  FROM feedback f
                                  JOIN sessions s ON s.SessionId = f.SessionId
                                  JOIN users subjectUser ON subjectUser.UserId = (CASE WHEN f.AuthorId = f.TutorId THEN f.TuteeId ELSE f.TutorId END)
                                  WHERE f.AuthorId = @userId
                                  ORDER BY f.FeedbackId DESC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(ReadFeedback(reader, "SubjectName"));
            return list;
        }

        // Feedback about me (private — only I, and Admin via a separate method, should call this).
        public async Task<List<FeedbackDisplay>> GetReceivedAsync(int userId)
        {
            var list = new List<FeedbackDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT f.FeedbackId, f.TutorId, f.TuteeId, f.SessionId, f.AuthorId, f.Rating, f.Comment,
                                         s.Subject,
                                         authorUser.DisplayName AS AuthorName
                                  FROM feedback f
                                  JOIN sessions s ON s.SessionId = f.SessionId
                                  JOIN users authorUser ON authorUser.UserId = f.AuthorId
                                  WHERE (f.TutorId = @userId OR f.TuteeId = @userId) AND f.AuthorId != @userId
                                  ORDER BY f.FeedbackId DESC;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(ReadFeedback(reader, "AuthorName"));
            return list;
        }

        // Admin: every feedback entry in the system, with both party names for searching.
        public async Task<List<FeedbackDisplay>> GetAllFeedbackAsync()
        {
            var list = new List<FeedbackDisplay>();
            await using var conn = await _db.GetOpenConnectionAsync();
            const string sql = @"SELECT f.FeedbackId, f.TutorId, f.TuteeId, f.SessionId, f.AuthorId, f.Rating, f.Comment,
                                         s.Subject,
                                         tutorUser.DisplayName AS TutorName,
                                         tuteeUser.DisplayName AS TuteeName
                                  FROM feedback f
                                  JOIN sessions s ON s.SessionId = f.SessionId
                                  JOIN users tutorUser ON tutorUser.UserId = f.TutorId
                                  JOIN users tuteeUser ON tuteeUser.UserId = f.TuteeId
                                  ORDER BY f.FeedbackId DESC;";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new FeedbackDisplay
                {
                    FeedbackId = reader.GetInt32("FeedbackId"),
                    TutorId = reader.GetInt32("TutorId"),
                    TuteeId = reader.GetInt32("TuteeId"),
                    SessionId = reader.GetInt32("SessionId"),
                    AuthorId = reader.GetInt32("AuthorId"),
                    Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? 0 : reader.GetInt32("Rating"),
                    Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString("Comment"),
                    Subject = reader.GetString("Subject"),
                    OtherPartyName = $"{reader.GetString("TutorName")} (Tutor) / {reader.GetString("TuteeName")} (Student)"
                });
            }
            return list;
        }

        private static FeedbackDisplay ReadFeedback(MySqlDataReader reader, string otherPartyColumn) => new()
        {
            FeedbackId = reader.GetInt32("FeedbackId"),
            TutorId = reader.GetInt32("TutorId"),
            TuteeId = reader.GetInt32("TuteeId"),
            SessionId = reader.GetInt32("SessionId"),
            AuthorId = reader.GetInt32("AuthorId"),
            Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? 0 : reader.GetInt32("Rating"),
            Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) ? null : reader.GetString("Comment"),
            Subject = reader.GetString("Subject"),
            OtherPartyName = reader.GetString(otherPartyColumn)
        };
    }
}