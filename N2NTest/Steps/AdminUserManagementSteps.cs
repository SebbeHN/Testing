namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using System;
using System.Threading.Tasks;

[Binding]
[Scope(Feature = "Admin User Management")]
public class AdminUserManagementSteps
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 5000 });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser != null)
            await _browser.CloseAsync();
        if (_playwright != null)
            _playwright.Dispose();
    }

    [Given(@"I am logged in as an admin")]
    public async Task GivenIAmLoggedInAsAnAdmin()
    {
        // Navigate to login page
        await _page.GotoAsync("http://localhost:3001/staff/login");
        Console.WriteLine("Navigated to staff login page");
        
        // Take screenshot for debugging
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "login-page.png" });
        
        // From your screenshots, looks like these are the login field selectors
        await _page.FillAsync(".staff-field-input, input.login-bar, input[type='text']", "admin");
        await _page.FillAsync("input[type='password'], input.login-bar[type='password']", "admin321");
        
        // Submit login form - from screenshot appears to be a button in a div with class login-knapp
        await _page.ClickAsync(".login-knapp button, button.staff-login-button, button[type='submit']");
        
        // Wait for login to complete and dashboard to load
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Take screenshot after login
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "after-login.png" });
        
        Console.WriteLine($"After admin login, URL is: {_page.Url}");
        
        // Verify we're logged in as admin by checking URL contains dashboard
        Assert.Contains("dashboard", _page.Url);
    }

    [When(@"I click on create user")]
    public async Task WhenIClickOnCreateUser()
    {
        // From your screenshots, we can see the Create User link in the nav
        // Using a more flexible selector to match what we see in the screenshots
        await _page.ClickAsync("a[href='/admin/create-user'], text=Create User, a:has-text('Create User')");
        
        // Wait for navigation to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Take screenshot after navigation
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "create-user-page.png" });
        
        Console.WriteLine($"After clicking create user, URL is: {_page.Url}");
    }

    [Then(@"I am on the create user page")]
    public async Task ThenIAmOnTheCreateUserPage()
    {
        // Verify we're on the create user page
        Assert.Contains("create-user", _page.Url);
        
        // From your screenshot, we can see the heading is "Skapa användare"
        var heading = await _page.QuerySelectorAsync("h1:has-text('Skapa användare')");
        Assert.NotNull(heading);
        
        Console.WriteLine("We are on the create user page with the heading 'Skapa användare'");
    }

    [When(@"I fill in the user email as ""(.*)""")]
    public async Task WhenIFillInTheUserEmailAs(string email)
    {
        // From your screenshot, we can see the placeholder text is "Ange en e-postadress"
        await _page.FillAsync("input[placeholder='Ange en e-postadress'], input.login-bar", email);
        Console.WriteLine($"Filled in user email as: {email}");
    }

    [When(@"I fill in the user name as ""(.*)""")]
    public async Task WhenIFillInTheUserNameAs(string name)
    {
        // From your screenshot, we can see the placeholder text is "Ange ett användarnamn"
        await _page.FillAsync("input[placeholder='Ange ett användarnamn'], input.login-bar", name);
        Console.WriteLine($"Filled in user name as: {name}");
    }

    [When(@"I fill in the password as ""(.*)""")]
    public async Task WhenIFillInThePasswordAs(string password)
    {
        // From your screenshot, we can see the placeholder text is "Ange ett lösenord"
        await _page.FillAsync("input[placeholder='Ange ett lösenord'], input.login-bar[type='password']", password);
        Console.WriteLine("Filled in password");
    }

    [When(@"I select ""(.*)"" as the company")]
    public async Task WhenISelectAsTheCompany(string company)
    {
        // From your screenshot, there's a dropdown with text "Välj företag"
        await _page.ClickAsync("select:has-text('Välj företag')");
        await _page.SelectOptionAsync("select", company);
        Console.WriteLine($"Selected company: {company}");
    }

    [When(@"I select ""(.*)"" as the role")]
    public async Task WhenISelectAsTheRole(string role)
    {
        // From your screenshot, there's a dropdown with text "Kundtjänst"
        await _page.ClickAsync("select:has-text('Kundtjänst')");
        await _page.SelectOptionAsync("select:nth-of-type(2)", role);
        Console.WriteLine($"Selected role: {role}");
    }

    [When(@"I click the ""(.*)"" button")]
    public async Task WhenIClickTheButton(string buttonText)
    {
        // From your screenshot, the button is inside a div with class login-knapp
        await _page.ClickAsync(".login-knapp button, button.bla, button:has-text('Skapa användare')");
        
        // Wait for any network activity to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine($"Clicked the button to create user");
        
        // Take screenshot after clicking
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "after-submit.png" });
    }

    [Then(@"I should see a success message")]
    public async Task ThenIShouldSeeASuccessMessage()
    {
        // Wait for success message with a timeout
        var successMessage = await _page.WaitForSelectorAsync(
            ".success-message, div:not(.error-message):has-text('Användare skapades'), div:has-text('framgångsrikt')", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
        
        Assert.NotNull(successMessage);
        
        // Take a screenshot of the success state
        await _page.ScreenshotAsync(new PageScreenshotOptions { 
            Path = "create-user-success.png" 
        });
        
        Console.WriteLine("Success message found");
    }
}