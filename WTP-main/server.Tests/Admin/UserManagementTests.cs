using System;
using System.Threading.Tasks;
using Npgsql;
using Xunit;
using server.Models;

namespace server.Tests.Admin
{
    public class UserManagementTests
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=wtp;Username=sebastianholmberg;";
        
        [Fact]
        public async Task CreateUser_AssignsCorrectRole()
        {
            // Arrange
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Starta en transaktion som vi kommer att rulla tillbaka
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // Generera unika testdata med Guid för att undvika krockar
                string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                string testFirstName = $"TestUser_{uniqueId}";
                string testEmail = $"test_{uniqueId}@example.com";
                
                // Act
                await using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT INTO users (first_name, password, company, created_at, role_id, email)
                    VALUES (@first_name, @password, @company, @created_at, @role_id, @email)
                    RETURNING ""Id"", role_id";
                    
                cmd.Parameters.AddWithValue("first_name", testFirstName);
                cmd.Parameters.AddWithValue("password", "TestPassword");
                cmd.Parameters.AddWithValue("company", "tele");
                cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("role_id", 1); // 1 = staff
                cmd.Parameters.AddWithValue("email", testEmail);
                
                await using var reader = await cmd.ExecuteReaderAsync();
                
                // Assert
                Assert.True(await reader.ReadAsync(), "Det borde gå att skapa en användare");
                
                int userId = reader.GetInt32(0);
                int roleId = reader.GetInt32(1);
                
                Assert.True(userId > 0, "UserId borde vara större än 0");
                Assert.Equal(1, roleId); // Verifierar att role_id är 1 (staff)
            }
            finally
            {
                // Rulla tillbaka transaktionen oavsett om testet lyckas eller misslyckas
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
            }
        }
        
        [Fact]
        public async Task DeleteUser_RemovesUserFromDatabase()
        {
            // Arrange
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Starta en transaktion som vi kommer att rulla tillbaka
            await using var transaction = await connection.BeginTransactionAsync();
            
            try 
            {
                // Skapa temporär testanvändare som vi sedan ska ta bort
                string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                string testFirstName = $"DeleteTestUser_{uniqueId}";
                string testEmail = $"delete_test_{uniqueId}@example.com";
                
                // Skapa användaren
                await using var createCmd = connection.CreateCommand();
                createCmd.Transaction = transaction;
                createCmd.CommandText = @"
                    INSERT INTO users (first_name, password, company, created_at, role_id, email)
                    VALUES (@first_name, @password, @company, @created_at, @role_id, @email)
                    RETURNING ""Id""";
                    
                createCmd.Parameters.AddWithValue("first_name", testFirstName);
                createCmd.Parameters.AddWithValue("password", "TestPassword");
                createCmd.Parameters.AddWithValue("company", "tele");
                createCmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
                createCmd.Parameters.AddWithValue("role_id", 1);
                createCmd.Parameters.AddWithValue("email", testEmail);
                
                
                var userId = Convert.ToInt32(await createCmd.ExecuteScalarAsync());
                Assert.True(userId > 0, "En användare borde skapas");
                
                // Act - Ta bort användaren
                await using var deleteCmd = connection.CreateCommand();
                deleteCmd.Transaction = transaction;
                deleteCmd.CommandText = @"DELETE FROM users WHERE ""Id"" = @userId";
                deleteCmd.Parameters.AddWithValue("userId", userId);
                
                int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
                
                // Verifiera att användaren togs bort
                await using var checkCmd = connection.CreateCommand();
                checkCmd.Transaction = transaction;
                checkCmd.CommandText = @"SELECT COUNT(*) FROM users WHERE ""Id"" = @userId";
                checkCmd.Parameters.AddWithValue("userId", userId);
                
                long userCount = (long)await checkCmd.ExecuteScalarAsync();
                
                // Assert
                Assert.Equal(1, rowsAffected);
                Assert.Equal(0, userCount);
            }
            finally
            {
                // Rulla tillbaka transaktionen
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
            }
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUserWithCorrectRole()
        {
            // Arrange
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // Skapa en testanvändare som vi kan logga in med
                string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
                string testFirstName = $"LoginTestUser_{uniqueId}";
                string testPassword = "TestLoginPassword"; 
                string testEmail = $"login_test_{uniqueId}@example.com";
                
                // Skapa användaren
                await using var createCmd = connection.CreateCommand();
                createCmd.Transaction = transaction;
                createCmd.CommandText = @"
                    INSERT INTO users (first_name, password, company, created_at, role_id, email)
                    VALUES (@first_name, @password, @company, @created_at, @role_id, @email)";
                    
                createCmd.Parameters.AddWithValue("first_name", testFirstName);
                createCmd.Parameters.AddWithValue("password", testPassword);
                createCmd.Parameters.AddWithValue("company", "tele");
                createCmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
                createCmd.Parameters.AddWithValue("role_id", 1); // staff
                createCmd.Parameters.AddWithValue("email", testEmail);
                
                await createCmd.ExecuteNonQueryAsync();
                
                // Act - Försök logga in med den skapade användaren
                await using var loginCmd = connection.CreateCommand();
                loginCmd.Transaction = transaction;
                loginCmd.CommandText = @"
                    SELECT ""Id"", first_name, company, role_id, email
                    FROM users
                    WHERE (email = @login_id OR LOWER(TRIM(first_name)) = LOWER(TRIM(@login_id)))
                    AND password = @password";
                    
                loginCmd.Parameters.AddWithValue("login_id", testFirstName); // Logga in med användarnamn
                loginCmd.Parameters.AddWithValue("password", testPassword);
                
                await using var reader = await loginCmd.ExecuteReaderAsync();
                bool userFound = await reader.ReadAsync();
                
                // Assert
                Assert.True(userFound, "Användaren borde kunna logga in med giltiga uppgifter");
                if (userFound)
                {
                    string firstName = reader.GetString(1);
                    int roleId = reader.GetInt32(3);
                    
                    Assert.Equal(testFirstName, firstName);
                    Assert.Equal(1, roleId); // staff
                }
            }
            finally
            {
                // Rulla tillbaka transaktionen
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}