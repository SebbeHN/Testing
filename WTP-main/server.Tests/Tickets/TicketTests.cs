using System;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using server.Models;
using Xunit;

namespace server.Tests.Tickets
{
    public class TicketTests
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=wtp;Username=sebastianholmberg;";
        
      [Fact]
public async Task CreateTeleForm_ShouldCreateRecordAndGenerateChatToken()
{
    // Arrange
    await using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();
    
    // Starta en transaktion som vi kommer att rulla tillbaka
    await using var transaction = await connection.BeginTransactionAsync();
    
    try
    {
        var testChatToken = Guid.NewGuid().ToString();
        var testTimestamp = DateTime.UtcNow;
        
        var teleForm = new TeleForm
        {
            FirstName = "TestUser",
            Email = "test@example.com",
            ServiceType = "Bredband",
            IssueType = "Tekniskt problem",
            Message = "Detta är ett testärende",
            CompanyType = "Tele/Bredband",
            ChatToken = testChatToken,
            SubmittedAt = testTimestamp,
            IsChatActive = true
        };
        
        // Act
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        
        // Ändra till att bara returnera chat_token (utan id)
        cmd.CommandText = @"
            INSERT INTO tele_forms (first_name, email, service_type, issue_type, message, chat_token, submitted_at, is_chat_active, company_type)
            VALUES (@first_name, @email, @service_type, @issue_type, @message, @chat_token, @submitted_at, @is_chat_active, @company_type)
            RETURNING chat_token";
            
        cmd.Parameters.AddWithValue("first_name", teleForm.FirstName);
        cmd.Parameters.AddWithValue("email", teleForm.Email);
        cmd.Parameters.AddWithValue("service_type", teleForm.ServiceType);
        cmd.Parameters.AddWithValue("issue_type", teleForm.IssueType);
        cmd.Parameters.AddWithValue("message", teleForm.Message);
        cmd.Parameters.AddWithValue("chat_token", testChatToken);
        cmd.Parameters.AddWithValue("submitted_at", testTimestamp);
        cmd.Parameters.AddWithValue("is_chat_active", teleForm.IsChatActive);
        cmd.Parameters.AddWithValue("company_type", teleForm.CompanyType);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        bool hasRow = await reader.ReadAsync();
        Assert.True(hasRow, "Borde få tillbaka en rad efter INSERT");
        
        if (hasRow)
        {
            string chatToken = reader.GetString(0);
            
            // Assert
            Assert.True(testChatToken == chatToken, "Chat-token borde matcha det som skickades in");
        }
    }
    finally
    {
        // Rulla alltid tillbaka transaktionen
        await transaction.RollbackAsync();
    }
}
        
        [Fact]
        public async Task GetTickets_FiltersByCompanyType()
        {
            // Arrange
            var dataSource = NpgsqlDataSource.Create(_connectionString);
            string companyType = "Tele/Bredband"; // Ändra till en av dina faktiska företagstyper
            
            // Act
            List<dynamic> tickets = new();
            
            await using var cmd = dataSource.CreateCommand(
                "SELECT chat_token, message, sender, submitted_at, issue_type, email, form_type " +
                "FROM initial_form_messages WHERE form_type = @usercompany");
                
            cmd.Parameters.AddWithValue("usercompany", companyType);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                tickets.Add(new
                {
                    ChatToken = reader.GetString(0),
                    Message = reader.GetString(1),
                    Sender = reader.GetString(2),
                    Timestamp = reader.GetDateTime(3),
                    IssueType = reader.GetString(4),
                    Email = reader.GetString(5),
                    FormType = reader.GetString(6)
                });
            }
            
            // Assert
            Assert.All(tickets, ticket => Assert.Equal(companyType, ticket.FormType));
        }
    }
}