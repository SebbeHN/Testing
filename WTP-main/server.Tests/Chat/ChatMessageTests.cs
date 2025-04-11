using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using server.Models;
using Xunit;

namespace server.Tests.Chat
{
    public class ChatMessageTests
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=wtp;Username=sebastianholmberg;";

        [Fact]
public async Task SendChatMessage_SavesMessageToDatabase()
{
    // Arrange
    await using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();

    // Starta en transaktion som vi kommer att rulla tillbaka
    await using var transaction = await connection.BeginTransactionAsync();

    try
    {
        var chatMessage = new ChatMessage
        {
            ChatToken = Guid.NewGuid().ToString(), // Unikt för detta test
            Sender = "TestUser",
            Message = "Detta är ett testmeddelande",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = @"
            INSERT INTO chat_messages (chat_token, sender, message, submitted_at)
            VALUES (@chat_token, @sender, @message, @submitted_at)
            RETURNING id, sender, message, submitted_at, chat_token";

        cmd.Parameters.AddWithValue("chat_token", chatMessage.ChatToken);
        cmd.Parameters.AddWithValue("sender", chatMessage.Sender);
        cmd.Parameters.AddWithValue("message", chatMessage.Message);
        cmd.Parameters.AddWithValue("submitted_at", chatMessage.Timestamp);

        // Omslut med try/catch för att få bättre felmeddelanden
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync(), "Kunde inte läsa resultatet efter insert");

            // För säkerhets skull, verifiera att kolumnerna finns
            Assert.True(reader.FieldCount >= 5, $"Förväntade minst 5 kolumner, fick {reader.FieldCount}");

            var savedSender = reader.GetString(1);
            var savedMessage = reader.GetString(2);
            var savedToken = reader.GetString(4);

            // Assert
            Assert.Equal(chatMessage.Sender, savedSender);
            Assert.Equal(chatMessage.Message, savedMessage);
            Assert.Equal(chatMessage.ChatToken, savedToken);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Fel vid dataläsning: {ex.Message}");
        }
    }
    finally
    {
        // Rulla tillbaka transaktionen även om testet misslyckas
        if (transaction != null)
        {
            await transaction.RollbackAsync();
        }
    }
}

        [Fact(Skip = "Inget befintligt chatToken angivet")]
        public async Task GetChatMessages_ReturnsMessagesForSpecificChatToken()
        {
            // Arrange
            var dataSource = NpgsqlDataSource.Create(_connectionString);

            string existingChatToken = ""; // Fyll i ett chatToken som finns i din databas

            if (string.IsNullOrEmpty(existingChatToken))
            {
                return; // Testet är markerat som hoppat
            }

            // Act
            List<dynamic> messages = new();

            await using var cmd = dataSource.CreateCommand(@"
                SELECT id, sender, message, submitted_at, chat_token
                FROM chat_messages
                WHERE chat_token = @chat_token
                ORDER BY submitted_at ASC");

            cmd.Parameters.AddWithValue("chat_token", existingChatToken);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                messages.Add(new
                {
                    Id = reader.GetInt32(0),
                    Sender = reader.GetString(1),
                    Message = reader.GetString(2),
                    Timestamp = reader.GetDateTime(3),
                    ChatToken = reader.GetString(4)
                });
            }

            // Assert
            Assert.All(messages, message => Assert.Equal(existingChatToken, message.ChatToken));
        }
    }
}