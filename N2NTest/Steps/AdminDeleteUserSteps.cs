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

    [BeforeScenario]
    public async Task Setup()
    {
        var result = await PlaywrightService.CreateNewPageAsync();
       
        
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
        
            // First navigate to the correct login page (not dashboard)
            await _page.GotoAsync("http://localhost:3001/staff/login");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
            Console.WriteLine($"Current URL: {_page.Url}");
            await _page.ScreenshotAsync(new() { Path = "before-login.png" });
        
            // Fill login form
            await _page.FillAsync("input[type='email']", "admin@example.com");
            await _page.FillAsync("input[type='password']", "Password123!");
        
            Console.WriteLine("Filled login form, clicking submit...");
            await _page.ScreenshotAsync(new() { Path = "login-form-filled.png" });
        
            // Click login button
            await _page.ClickAsync("button[type='submit']");
        
            // Wait with increased timeout
            await _page.WaitForURLAsync("**/admin/dashboard", new() { Timeout = 15000 });
        
            Console.WriteLine($"After login URL: {_page.Url}");
            await _page.ScreenshotAsync(new() { Path = "after-login.png" });
        
            // Wait for table with increased timeout
            await _page.WaitForSelectorAsync("table", new() { Timeout = 15000 });
        
            Console.WriteLine("Admin login successful, table found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            await _page.ScreenshotAsync(new() { Path = "login-failure.png" });
        
            // Dump HTML for debugging
            var html = await _page.ContentAsync();
            File.WriteAllText("login-page.html", html);
            throw;
        }
    }

    [When(@"I delete the user with email ""(.*)""")]
    public async Task WhenIDeleteUserWithEmail(string email)
    {
        try
        {
            Console.WriteLine($"Looking for user with email: {email}");
            await _page.ScreenshotAsync(new() { Path = "before-delete.png" });
        
            // Log table contents for debugging
            var tableContent = await _page.EvaluateAsync<string>(@"() => {
            const rows = Array.from(document.querySelectorAll('table tr'));
            return rows.map(row => row.textContent).join('\n');
        }");
            Console.WriteLine($"Table content: {tableContent}");
        
            // Check if the user with email exists at all
            var userExists = await _page.EvaluateAsync<bool>(@"(email) => {
    const rows = Array.from(document.querySelectorAll('table tr'));
    return rows.some(row => row.textContent.includes(email));
}", email);
        
            if (!userExists)
            {
                Console.WriteLine($"User with email {email} not found in the table!");
            }
        
            // Try to find the row with increased timeout
            var row = _page.Locator("table tr").Filter(new() { HasText = email });
            await row.WaitForAsync(new() { Timeout = 20000 }); 
        
            var deleteButton = row.Locator("button", new() { HasTextString = "Ta bort" });
            await deleteButton.ClickAsync();
        
            Console.WriteLine("Delete button clicked");
            await _page.WaitForTimeoutAsync(2500); // Longer pause after click
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during delete: {ex.Message}");
            await _page.ScreenshotAsync(new() { Path = "delete-error.png" });
            throw;
        }
    }


    [Then(@"the user with email ""(.*)"" should no longer be visible")]
    public async Task ThenUserShouldBeGone(string email)
    {
        await _page.ReloadAsync();

        var rows = await _page.Locator("table tr").Filter(new() { HasText = email }).CountAsync();

        Assert.Equal(0, rows);
    }

}
