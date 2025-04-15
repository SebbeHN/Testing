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
    [BeforeScenario]
public async Task EnsureTestUserExists()
{
    Console.WriteLine("Ensuring test user exists before running delete test...");
    
    // Navigate to admin dashboard
    await _page.GotoAsync("http://localhost:3001/staff/login");
    await LoginHelper.LoginAsRole(_page, "admin");
    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    await Task.Delay(2000);
    
    // Check if user exists
    var userEmail = "newstaff@example.com";
    var userExists = await _page.EvaluateAsync<bool>($@"(email) => {{
        const rows = Array.from(document.querySelectorAll('table tr'));
        return rows.some(row => row.textContent.includes(email));
    }}", userEmail);
    
    if (!userExists) {
        Console.WriteLine($"Creating test user {userEmail} for delete test...");
        await CreateTestUser(userEmail);
    } else {
        Console.WriteLine($"Test user {userEmail} already exists");
    }
}

private async Task CreateTestUser(string email)
{
    // Click create user button with proper error handling
    await _page.ScreenshotAsync(new() { Path = "before-create-user.png" });
    
    try {
        var createUserLink = await _page.QuerySelectorAsync("a:has-text('Create User'), a:has-text('Skapa användare')");
        if (createUserLink != null) {
            await createUserLink.ClickAsync();
        } else {
            await _page.EvaluateAsync(@"() => {
                const links = Array.from(document.querySelectorAll('a'));
                const createUserLink = links.find(a =>
                    a.textContent.includes('Create User') ||
                    a.textContent.includes('Skapa användare') ||
                    (a.textContent.includes('Create') && a.textContent.includes('User'))
                );
                if (createUserLink) createUserLink.click();
            }");
        }
    } catch (Exception ex) {
        Console.WriteLine($"Error clicking create user: {ex.Message}");
    }

    // Wait for page to load
    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await Task.Delay(2000);
    
    // Fill email with multiple attempts
    try {
        await _page.FillAsync("input[name='email']", email);
    } catch (Exception) {
        try {
            await _page.FillAsync("input[type='email']", email);
        } catch (Exception) {
            await _page.EvaluateAsync($@"() => {{
                const inputs = Array.from(document.querySelectorAll('input'));
                const emailInput = inputs.find(i =>
                    i.name === 'email' ||
                    i.placeholder?.includes('e-post') ||
                    i.type === 'email' ||
                    i === document.querySelector('input')
                );
                if (emailInput) {{
                    emailInput.value = '{email}';
                    emailInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                }}
            }}");
        }
    }
    
    // Fill name field
    try {
        await _page.FillAsync("input[name='firstName']", "Test");
    } catch (Exception) {
        await _page.EvaluateAsync(@"() => {
            const inputs = Array.from(document.querySelectorAll('input'));
            const nameInput = inputs.find(i =>
                i.name === 'firstName' ||
                i.placeholder?.includes('användarnamn') ||
                inputs.indexOf(i) === 1
            );
            if (nameInput) {
                nameInput.value = 'Test';
                nameInput.dispatchEvent(new Event('input', { bubbles: true }));
            }
        }");
    }
    
    // Fill password field
    try {
        await _page.FillAsync("input[name='password']", "Password123!");
    } catch (Exception) {
        try {
            await _page.FillAsync("input[type='password']", "Password123!");
        } catch (Exception) {
            await _page.EvaluateAsync(@"() => {
                const inputs = Array.from(document.querySelectorAll('input'));
                const passwordInput = inputs.find(i =>
                    i.type === 'password' ||
                    i.name === 'password'
                );
                if (passwordInput) {
                    passwordInput.value = 'Password123!';
                    passwordInput.dispatchEvent(new Event('input', { bubbles: true }));
                }
            }");
        }
    }
    
    // Select staff role
    try {
        await _page.SelectOptionAsync("select[name='role']", "staff");
    } catch (Exception) {
        await _page.EvaluateAsync(@"() => {
            const select = document.querySelector('select[name=""role""]') ||
                           document.querySelectorAll('select')[1];
            if (select) {
                select.value = 'staff';
                select.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }");
    }
    
    // Click submit button
    await _page.ScreenshotAsync(new() { Path = "before-submit-create.png" });
    try {
        await _page.ClickAsync("button[type='submit'], button:has-text('Skapa'), button:has-text('Create')");
    } catch (Exception) {
        await _page.EvaluateAsync(@"() => {
            const buttons = Array.from(document.querySelectorAll('button'));
            const submitBtn = buttons.find(b => 
                b.type === 'submit' || 
                b.textContent.includes('Skapa') || 
                b.textContent.includes('Create')
            );
            if (submitBtn) submitBtn.click();
        }");
    }
    
    // Wait for completion and verify
    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    await Task.Delay(3000);
    await _page.ScreenshotAsync(new() { Path = "after-create-user.png" });
    
    // Return to admin dashboard to ensure we're ready for the delete test
    await _page.GotoAsync("http://localhost:3001/admin/dashboard");
    await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
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
        var response = await _page.GotoAsync("http://localhost:3001/staff/login", 
            new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });
        
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
