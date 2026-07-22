using PairedAPI.Models;
using MySqlConnector;

namespace PairedAPI.Services
{
    public class RequestService
    {
        private readonly string _connectionString;

        public RequestService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> CreateRequest(Request request)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                @"INSERT INTO Requests (TuteeId, TutorId, Subject, PreferredDate, Status, RejectionReason) 
                  VALUES (@TuteeId, @TutorId, @Subject, @PreferredDate, @Status, @RejectionReason)", conn);

            cmd.Parameters.AddWithValue("@TuteeId", request.TuteeId);
            cmd.Parameters.AddWithValue("@TutorId", request.TutorId);
            cmd.Parameters.AddWithValue("@Subject", request.Subject);
            cmd.Parameters.AddWithValue("@PreferredDate", request.PreferredDate);
            cmd.Parameters.AddWithValue("@Status", request.Status ?? "Pending");
            cmd.Parameters.AddWithValue("@RejectionReason", request.RejectionReason ?? "");

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<Request>> GetRequests(int tuteeId)
        {
            var requests = new List<Request>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT * FROM Requests WHERE TuteeId=@TuteeId", conn);

            cmd.Parameters.AddWithValue("@TuteeId", tuteeId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(new Request
                {
                    RequestId = reader.GetInt32("RequestId"),
                    TuteeId = reader.GetInt32("TuteeId"),
                    TutorId = reader.GetInt32("TutorId"),
                    Subject = reader.GetString("Subject"),
                    PreferredDate = reader.GetDateTime("PreferredDate"),
                    Status = reader.GetString("Status"),
                    RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason"))
                                      ? null
                                      : reader.GetString("RejectionReason")
                });
            }
            return requests;
        }

        public async Task<List<Request>> GetRequestsForTutor(int tutorId)
        {
            var requests = new List<Request>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "SELECT * FROM Requests WHERE TutorId=@TutorId", conn);

            cmd.Parameters.AddWithValue("@TutorId", tutorId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(new Request
                {
                    RequestId = reader.GetInt32("RequestId"),
                    TuteeId = reader.GetInt32("TuteeId"),
                    TutorId = reader.GetInt32("TutorId"),
                    Subject = reader.GetString("Subject"),
                    PreferredDate = reader.GetDateTime("PreferredDate"),
                    Status = reader.GetString("Status"),
                    RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason"))
                                      ? null
                                      : reader.GetString("RejectionReason")
                });
            }
            return requests;
        }

        public async Task<List<Request>> GetRequestsByStatus(int userId, string role, string status)
        {
            var requests = new List<Request>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = role == "Tutor"
                ? "SELECT * FROM Requests WHERE TutorId=@UserId AND Status=@Status"
                : "SELECT * FROM Requests WHERE TuteeId=@UserId AND Status=@Status";

            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Status", status);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(new Request
                {
                    RequestId = reader.GetInt32("RequestId"),
                    TuteeId = reader.GetInt32("TuteeId"),
                    TutorId = reader.GetInt32("TutorId"),
                    Subject = reader.GetString("Subject"),
                    PreferredDate = reader.GetDateTime("PreferredDate"),
                    Status = reader.GetString("Status"),
                    RejectionReason = reader.IsDBNull(reader.GetOrdinal("RejectionReason"))
                                      ? null
                                      : reader.GetString("RejectionReason")
                });
            }
            return requests;
        }


        public async Task<bool> UpdateRequestStatus(int requestId, string status, string? rejectionReason = null)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            // Update the request status
            var cmd = new MySqlCommand(
                "UPDATE Requests SET Status=@Status, RejectionReason=@RejectionReason WHERE RequestId=@RequestId", conn);

            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@RejectionReason", rejectionReason ?? "");
            cmd.Parameters.AddWithValue("@RequestId", requestId);

            var rows = await cmd.ExecuteNonQueryAsync();

            // If accepted, create a session linked to this request
            if (rows > 0 && status == "Accepted")
            {
                var getCmd = new MySqlCommand("SELECT * FROM Requests WHERE RequestId=@RequestId", conn);
                getCmd.Parameters.AddWithValue("@RequestId", requestId);

                using var reader = await getCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int tuteeId = reader.GetInt32("TuteeId");
                    int tutorId = reader.GetInt32("TutorId");
                    string subject = reader.GetString("Subject");
                    DateTime preferredDate = reader.GetDateTime("PreferredDate");
                    reader.Close();

                    var sessionCmd = new MySqlCommand(
                        @"INSERT INTO Sessions (TutorId, StudentId, Subject, ScheduledTime, Status) 
                  VALUES (@TutorId, @StudentId, @Subject, @ScheduledTime, 'Upcoming')", conn);

                    sessionCmd.Parameters.AddWithValue("@TutorId", tutorId);
                    sessionCmd.Parameters.AddWithValue("@StudentId", tuteeId);
                    sessionCmd.Parameters.AddWithValue("@Subject", subject);
                    sessionCmd.Parameters.AddWithValue("@ScheduledTime", preferredDate);

                    await sessionCmd.ExecuteNonQueryAsync();
                }
            }

            return rows > 0;
        }
    }
}
