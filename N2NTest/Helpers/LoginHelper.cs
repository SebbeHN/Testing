using Microsoft.Playwright;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace N2NTest.Helpers
{
    public static class LoginHelper
    {
        private static string BaseUrl => Environment.GetEnvironmentVariable("TEST_APP_URL") ?? "http://localhost:5000/";
        
        private static readonly Dictionary<string, (string Username, string Password)> Credentials =
            new Dictionary<string, (string Username, string Password)>
            {
                { "staff", ("staff", "staff123") },
                { "admin", ("admin", "admin321") }
            };
        
    
        public static async Task LoginAsRole(IPage page, string role)
        {
            try
            {
                // Debug info
                Console.WriteLine($"Attempting to login as: {role}");

                // Navigate to the login page first
                await page.GotoAsync($"{BaseUrl}staff/login");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
                // Take screenshot of login page
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"login-page-{role}.png" });
            
                // Set correct credentials
                string username = role;
                string password = role == "admin" ? "admin321" : "staff123";
            
                Console.WriteLine($"Using username: {username}, password: {password}");
            
                // Fill login form with explicit selectors
                await page.FillAsync("input[name='username'], input[type='text']", username);
                await page.FillAsync("input[name='password'], input[type='password']", password);
            
                // Take screenshot before submitting
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"before-submit-{role}.png" });
            
                // Click login button
                await page.ClickAsync("button[type='submit'], input[type='submit']");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
                // Take screenshot after login attempt
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"after-login-{role}.png" });
                Console.WriteLine($"Current URL after login attempt: {page.Url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"login-error-{role}.png" });
                throw;
            }
        }

        public static async Task Login(IPage page, string username, string password)
        {
            await page.GotoAsync($"{BaseUrl}staff/login");

            await page.FillAsync("input[name='username']", username);
            await page.FillAsync("input[name='password']", password);
            await page.ClickAsync("button[type='submit']");

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}