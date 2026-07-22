using PairedAPI.Models;
using MySqlConnector;

namespace PairedAPI.Services
{
    public class FeedbackService
    {
        private readonly string _connectionString;

        public FeedbackService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> AddFeedback(Feedback feedback)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                @"INSERT INTO Feedback (SessionId, TuteeId, TutorId, Rating, Comments) 
                  VALUES (@SessionId, @TuteeId, @TutorId, @Rating, @Comments)", conn);

            cmd.Parameters.AddWithValue("@SessionId", feedback.SessionId);
            cmd.Parameters.AddWithValue("@TuteeId", feedback.TuteeId);
            cmd.Parameters.AddWithValue("@TutorId", feedback.TutorId);
            cmd.Parameters.AddWithValue("@Rating", feedback.Rating);
            cmd.Parameters.AddWithValue("@Comments", feedback.Comments ?? "");

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<Feedback>> GetFeedbackBySession(int sessionId)
        {
            var feedbackList = new List<Feedback>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT * FROM Feedback WHERE SessionId=@SessionId", conn);

            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                feedbackList.Add(new Feedback
                {
                    FeedbackId = reader.GetInt32("FeedbackId"),
                    SessionId = reader.GetInt32("SessionId"),
                    TuteeId = reader.GetInt32("TuteeId"),
                    TutorId = reader.GetInt32("TutorId"),
                    Rating = reader.GetInt32("Rating"),
                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments"))
                               ? null
                               : reader.GetString("Comments")
                });
            }
            return feedbackList;
        }
    }
}
