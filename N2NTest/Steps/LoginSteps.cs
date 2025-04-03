namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Login as admin at Shoptester")] // Add this line to scope to specific feature
public class LoginSteps
{
    // SETUP:
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

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
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
    
    
    // STEPS:
    [Given(@"I am on Shoptester homepage")]
    public async Task GivenIAmOnShoptesterHomepage()
    {
        await _page.GotoAsync("http://localhost:5000");
    }

    [Given(@"I see the ""(.*)"" button")]
    public async Task GivenISeeTheButton(string buttonName)
    {
        var loginButton = await _page.QuerySelectorAsync($"text={buttonName}");
        Assert.NotNull(loginButton);
    }

    [When(@"I click on the ""(.*)"" button")]
    public async Task WhenIClickOnTheButton(string buttonName)
    {
        await _page.ClickAsync($"text={buttonName}");
    }

    [When(@"i fill in the login form with valid credentials")]
    public async Task WhenIFillInTheLoginFormWithValidCredentials()
    {
        // Wait for login form to appear
        await _page.WaitForSelectorAsync("form, .login-form");

        // Fill in login credentials
        await _page.FillAsync("input[name='email'], input[type='email']", "admin@admin.com");
        await _page.FillAsync("input[name='password'], input[type='password']", "admin123");

        
    }
    
    [When(@"press the submit button")]
    public async Task WhenPressTheSubmitButton()
    {
        // Submit the form
        await _page.ClickAsync("button[type='submit'], input[type='submit'], button:has-text('Submit')");
    }

    [Then(@"I should be logged in")]
    public async Task ThenIShouldBeLoggedIn()
    {
        // Wait for an element that indicates successful login
        await _page.WaitForSelectorAsync(".user-profile, .logged-in-indicator, .welcome-message, .dashboard");

        // Verify login was successful
        var loggedInIndicator = await _page.QuerySelectorAsync(".user-profile, .logged-in-indicator, .welcome-message, .dashboard");
        Assert.NotNull(loggedInIndicator);
    }
}
    
