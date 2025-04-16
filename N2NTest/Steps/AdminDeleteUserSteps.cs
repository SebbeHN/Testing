using Microsoft.Playwright;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;
using System.Threading.Tasks;
using E2ETesting.Steps;

namespace N2NTest.Steps;

[Binding]
[Scope(Feature = "Admin deletes a user")]
public class AdminDeleteUserSteps
{
    private IPage _page;
    private IBrowser _browser;
    private IPlaywright _playwright;
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_APP_URL") ?? "http://localhost:5000/";

    [BeforeScenario]
    public async Task Setup()
    {
        var result = await PlaywrightSetup.CreateBrowserAndPage();
        _browser = result.browser;
        _page = result.page;
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser is not null)
            await _browser.CloseAsync();
    }

    [Given("I am logged in as an admin")]
public async Task GivenIAmLoggedInAsAnAdmin()
{
    try
    {
        Console.WriteLine("Starting admin login process...");
        
        // Navigate to login page with proper error handling
        Console.WriteLine("Navigating to login page...");
        var response = await _page.GotoAsync($"{BaseUrl}staff/login");
        
        if (response == null || !response.Ok)
        {
            Console.WriteLine($"Failed to load login page. Status: {response?.Status ?? 0}");
            throw new Exception("Login page failed to load");
        }
        
        Console.WriteLine($"Current URL: {_page.Url}");
        await _page.ScreenshotAsync(new() { Path = "login-page-loaded.png" });
        
        // Dump HTML to see what's on the page
        var html = await _page.ContentAsync();
        File.WriteAllText("login-page.html", html);
        Console.WriteLine("HTML content saved to login-page.html");
        
        // Check if there's any form on the page
        var formExists = await _page.QuerySelectorAsync("form") != null;
        Console.WriteLine($"Form element exists: {formExists}");
        
        // Try multiple selectors for email input
        string[] emailSelectors = {
            "input[type='email']", 
            "input[name='email']",
            "input[id*='email' i]",
            "input[placeholder*='email' i]",
            "input:first-of-type"
        };
        
        string usedEmailSelector = null;
        foreach (var selector in emailSelectors)
        {
            Console.WriteLine($"Trying email selector: {selector}");
            var element = await _page.QuerySelectorAsync(selector);
            if (element != null)
            {
                usedEmailSelector = selector;
                Console.WriteLine($"Found email input with selector: {selector}");
                break;
            }
        }
        
        if (usedEmailSelector == null)
        {
            throw new Exception("Could not find email input with any known selector");
        }
        
        // Fill form with verified selectors
        await _page.FillAsync(usedEmailSelector, "admin@admin.com");
        
        // Look for password field
        string[] passwordSelectors = {
            "input[type='password']",
            "input[name='password']",
            "input[id*='password' i]",
            "input[placeholder*='password' i]"
        };
        
        string usedPasswordSelector = null;
        foreach (var selector in passwordSelectors)
        {
            var element = await _page.QuerySelectorAsync(selector);
            if (element != null)
            {
                usedPasswordSelector = selector;
                Console.WriteLine($"Found password input with selector: {selector}");
                break;
            }
        }
        
        if (usedPasswordSelector == null)
        {
            throw new Exception("Could not find password input");
        }
        
        await _page.FillAsync(usedPasswordSelector, "admin321");
        
        // Find and click submit button
        string[] buttonSelectors = {
            "button[type='submit']",
            "button:text('Logga in')",
            "input[type='submit']",
            "button"
        };
        
        string usedButtonSelector = null;
        foreach (var selector in buttonSelectors)
        {
            var element = await _page.QuerySelectorAsync(selector);
            if (element != null)
            {
                usedButtonSelector = selector;
                Console.WriteLine($"Found submit button with selector: {selector}");
                break;
            }
        }
        
        if (usedButtonSelector == null)
        {
            throw new Exception("Could not find submit button");
        }
        
        // Take screenshot before clicking button
        await _page.ScreenshotAsync(new() { Path = "before-submit.png" });
        
        // Click the button and wait for navigation
        await _page.ClickAsync(usedButtonSelector);
        
        // Wait for navigation with increased timeout
        await _page.WaitForURLAsync("**/admin/dashboard*", new() { Timeout = 30000 });
        
        Console.WriteLine($"URL after login: {_page.Url}");
        await _page.ScreenshotAsync(new() { Path = "after-login.png" });
        
        // Wait for table
        await _page.WaitForSelectorAsync("table", new() { Timeout = 20000 });
        
        Console.WriteLine("Admin login successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Login failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        try {
            await _page.ScreenshotAsync(new() { Path = "login-error.png" });
            var errorHtml = await _page.ContentAsync();
            File.WriteAllText("login-error-page.html", errorHtml);
        } catch (Exception screenshotEx) {
            Console.WriteLine($"Failed to capture error screenshot: {screenshotEx.Message}");
        }
        throw;
    }
}

[When(@"I delete the user with email ""(.*)""")]
public async Task WhenIDeleteUserWithEmail(string email)
{
    try
    {
        Console.WriteLine($"Letar efter användare med e-post: {email}");
        await _page.ScreenshotAsync(new() { Path = "before-delete.png" });
        
        // Vänta på att tabellen laddas fullständigt
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Task.Delay(3000);
        
        
        
        // Kontrollera om användaren finns överhuvudtaget
        var userExists = await _page.EvaluateAsync<bool>(@"(email) => {
            const rows = Array.from(document.querySelectorAll('table tr'));
            return rows.some(row => row.textContent.includes(email));
        }", email);
        
        if (!userExists)
        {
            Console.WriteLine($"Användare med e-post {email} hittades inte, provar att ladda om sidan!");
            await _page.ReloadAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(5000);
            
            // Kontrollera igen efter omladdning
            userExists = await _page.EvaluateAsync<bool>(@"(email) => {
                const rows = Array.from(document.querySelectorAll('table tr'));
                return rows.some(row => row.textContent.includes(email));
            }", email);
            
            if (!userExists)
            {
                throw new Exception($"Användare med e-post {email} hittades inte i tabellen!");
            }
        }
        
        // Använd JavaScript för att hitta raden och klicka på ta bort-knappen
        var success = await _page.EvaluateAsync<bool>(@"(email) => {
            try {
                const rows = Array.from(document.querySelectorAll('table tr'));
                const userRow = rows.find(row => row.textContent.includes(email));
                
                if (userRow) {
                    const deleteButton = userRow.querySelector('button');
                    if (deleteButton) {
                        deleteButton.click();
                        return true;
                    }
                }
                return false;
            } catch (e) {
                console.error('Error:', e);
                return false;
            }
        }", email);
        
        if (!success)
        {
            // Fallback-metod med Playwright's API
            var row = _page.Locator("table tr").Filter(new() { HasText = email });
            await row.WaitForAsync(new() { Timeout = PlaywrightSetup.DefaultTimeout }); 
            
            var deleteButton = row.Locator("button", new() { HasTextString = "Ta bort" });
            await deleteButton.ClickAsync(new() { Timeout = PlaywrightSetup.DefaultTimeout });
        }
        
        Console.WriteLine("Ta bort-knappen klickad");
        await _page.WaitForTimeoutAsync(5000); // Längre paus efter klick
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fel vid borttagning: {ex.Message}");
        await _page.ScreenshotAsync(new() { Path = "delete-error.png" });
        throw;
    }
}

[Then(@"the user is deleted successfully")]
public async Task ThenTheUserIsDeletedSuccessfully()
{
    try
    {
        // Reload the page to ensure we have the latest data
        await _page.ReloadAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Get the email from the scenario context (or hardcode if you prefer)
        string email = "newstaff@example.com";
        
        // Check if the user still exists in the table
        var userExists = await _page.EvaluateAsync<bool>(@"(email) => {
            const rows = Array.from(document.querySelectorAll('table tr'));
            return rows.some(row => row.textContent.includes(email));
        }", email);
        
        // Assert that the user does not exist
        Assert.False(userExists, $"User with email {email} should not be present after deletion");
        
        Console.WriteLine($"Successfully verified that user {email} was deleted");
        await _page.ScreenshotAsync(new() { Path = "user-deleted-verification.png" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Verification failed: {ex.Message}");
        await _page.ScreenshotAsync(new() { Path = "verification-error.png" });
        throw;
    }
}
}

