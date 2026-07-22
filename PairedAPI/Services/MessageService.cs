using PairedAPI.Models;
using MySqlConnector;

namespace PairedAPI.Services
{
    public class MessageService
    {
        private readonly string _connectionString;

        public MessageService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> SendMessage(Message message)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                @"INSERT INTO Messages (SenderId, ReceiverId, Content, SentAt) 
                  VALUES (@SenderId, @ReceiverId, @Content, @SentAt)", conn);

            cmd.Parameters.AddWithValue("@SenderId", message.SenderId);
            cmd.Parameters.AddWithValue("@ReceiverId", message.ReceiverId);
            cmd.Parameters.AddWithValue("@Content", message.Content);
            cmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<Message>> GetConversation(int user1Id, int user2Id)
        {
            var messages = new List<Message>();
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                @"SELECT * FROM Messages 
                  WHERE (SenderId=@User1 AND ReceiverId=@User2) 
                     OR (SenderId=@User2 AND ReceiverId=@User1)
                  ORDER BY SentAt ASC", conn);

            cmd.Parameters.AddWithValue("@User1", user1Id);
            cmd.Parameters.AddWithValue("@User2", user2Id);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new Message
                {
                    MessageId = reader.GetInt32("MessageId"),
                    SenderId = reader.GetInt32("SenderId"),
                    ReceiverId = reader.GetInt32("ReceiverId"),
                    Content = reader.GetString("Content"),
                    SentAt = reader.GetDateTime("SentAt")
                });
            }
            return messages;
        }
    }
}
