using System;
using System.Threading.Tasks;
using Npgsql;
using Xunit;

namespace server.Tests.Authentication
{
    public class LoginTests
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=wtp;Username=sebastianholmberg;";
        
        [Fact]
public async Task Login_WithValidCredentials_ReturnsUserWithCorrectRole()
{
    // Arrange
    await using var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();
    
    await using var transaction = await connection.BeginTransactionAsync();
    
    try
    {
        // Skapa en ny användare för testet
        string testUsername = "testuser_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string testPassword = "testpass123";
        
        // Lägg till testanvändaren
        await using var createCmd = connection.CreateCommand();
        createCmd.Transaction = transaction;
        createCmd.CommandText = @"
            INSERT INTO users (first_name, password, company, created_at, role_id, email)
            VALUES (@first_name, @password, @company, @created_at, @role_id, @email)";
        
        createCmd.Parameters.AddWithValue("first_name", testUsername);
        createCmd.Parameters.AddWithValue("password", testPassword);
        createCmd.Parameters.AddWithValue("company", "tele");
        createCmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
        createCmd.Parameters.AddWithValue("role_id", 1); // 1 = staff
        createCmd.Parameters.AddWithValue("email", testUsername + "@example.com");
        
        await createCmd.ExecuteNonQueryAsync();
        
        // Act - Testa inloggning med den nya användaren
        await using var loginCmd = connection.CreateCommand();
        loginCmd.Transaction = transaction;
        loginCmd.CommandText = @"
            SELECT ""Id"", first_name, company, role_id, email
            FROM users
            WHERE (email = @login_id OR LOWER(TRIM(first_name)) = LOWER(TRIM(@login_id)))
            AND password = @password";
        
        loginCmd.Parameters.AddWithValue("login_id", testUsername);
        loginCmd.Parameters.AddWithValue("password", testPassword);
        
        await using var reader = await loginCmd.ExecuteReaderAsync();
        bool userFound = await reader.ReadAsync();
        
        string role = "";
        if (userFound)
        {
            int roleId = reader.GetInt32(3);
            role = roleId switch
            {
                1 => "staff",
                2 => "admin",
                3 => "admin",
                _ => "unknown"
            };
        }
        
        // Assert
        Assert.True(userFound, "Användaren borde hittas med giltiga uppgifter");
        Assert.Equal("staff", role);
    }
    finally
    {
        // Rulla tillbaka transaktionen för att rensa testdata
        await transaction.RollbackAsync();
    }
}
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsNoUser()
        {
            // Arrange
            var dataSource = NpgsqlDataSource.Create(_connectionString);
            string testUsername = "invaliduser"; // Använd en användare som INTE finns i din databas
            string testPassword = "invalidpassword";
            
            // Act
            await using var cmd = dataSource.CreateCommand(@"
                SELECT ""Id"", first_name, company, role_id, email
                FROM users
                WHERE (email = @login_id OR LOWER(TRIM(first_name)) = LOWER(TRIM(@login_id)))
                AND password = @password");
                
            cmd.Parameters.AddWithValue("login_id", testUsername);
            cmd.Parameters.AddWithValue("password", testPassword);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            bool userFound = await reader.ReadAsync();
            
            // Assert
            Assert.False(userFound, "Inga användare borde hittas med ogiltiga uppgifter");
        }
    }
}