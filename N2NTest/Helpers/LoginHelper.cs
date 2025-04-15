// Helpers/LoginHelper.cs
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using System.IO;

namespace N2NTest.Helpers
{
    public static class LoginHelper
    {
        public static async Task LoginAsRole(IPage page, string role)
        {
            Console.WriteLine($"Attempting to login as {role}...");
            
            try
            {
                // Take screenshot before login attempt
                await page.ScreenshotAsync(new() { Path = $"before-login-{role}.png" });
                
                // Check current URL to see where we are
                var currentUrl = page.Url;
                Console.WriteLine($"Current URL before login: {currentUrl}");
                
                // Determine if we need to navigate to login page
                if (!currentUrl.Contains("/login") && !currentUrl.Contains("/auth"))
                {
                    Console.WriteLine("Not on login page, navigating to login page...");
                    await page.GotoAsync("http://localhost:3001/staff/login");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }
                
                // Check if login form exists
                var hasLoginForm = await page.QuerySelectorAsync("form") != null;
                Console.WriteLine($"Login form found: {hasLoginForm}");
                
                if (!hasLoginForm)
                {
                    var pageContent = await page.ContentAsync();
                    File.WriteAllText($"login-page-missing-form-{DateTime.Now:yyyyMMddHHmmss}.html", pageContent);
                    throw new Exception("Login form not found on the page");
                }

                // Get appropriate credentials
                var (email, password) = GetCredentialsForRole(role);
                Console.WriteLine($"Using email: {email}");
                
                // Fill login form
                await page.FillAsync("input[type='email'], input[name='email']", email);
                await page.FillAsync("input[type='password'], input[name='password']", password);
                
                // Take screenshot before clicking login button
                await page.ScreenshotAsync(new() { Path = $"login-form-filled-{role}.png" });
                
                // Click login button and wait for navigation
                Console.WriteLine("Clicking login button...");
                await page.ClickAsync("button[type='submit'], button:has-text('Logga in')");
                
                // Wait for navigation to complete
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await page.WaitForTimeoutAsync(2000); // Give the page a moment to process the login
                
                // Take screenshot after login attempt
                await page.ScreenshotAsync(new() { Path = $"after-login-{role}.png" });
                
                // Verify login was successful
                var newUrl = page.Url;
                Console.WriteLine($"URL after login attempt: {newUrl}");
                
                // Check for common login failure indicators
                var hasErrorMessage = await page.QuerySelectorAsync(".error, .alert-error, [role='alert']") != null;
                if (hasErrorMessage)
                {
                    var errorText = await page.TextContentAsync(".error, .alert-error, [role='alert']");
                    throw new Exception($"Login failed. Error message: {errorText}");
                }
                
                // Success log
                Console.WriteLine($"Successfully logged in as {role}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                await page.ScreenshotAsync(new() { Path = $"login-error-{role}.png" });
                throw;
            }
        }

        private static (string email, string password) GetCredentialsForRole(string role)
        {
            return role.ToLower() switch
            {
                "admin" => ("admin@admin.com", "admin321"),
                "staff" => ("staff@test.com", "staff123"),
                "customer" => ("customer@example.com", "Password123!"),
                _ => throw new ArgumentException($"Unknown role: {role}")
            };
        }
    }
}