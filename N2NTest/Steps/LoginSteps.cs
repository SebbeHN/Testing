namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Login")] // Update this to match your feature file name exactly
public class LoginSteps
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
        _playwright?.Dispose();
    }
    
    [Given(@"I am at the WTP homepage")]
    public async Task GivenIAmAtTheWTPHomepage()
    {
        await _page.GotoAsync("http://localhost:3001");
        
        // Wait for page to load completely
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine($"Navigated to WTP homepage: {_page.Url}");
    }

    [Given(@"I see the register button")]
    public async Task GivenISeeTheRegisterButton()
    {
        // Take a screenshot to verify the page state
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "wtp-homepage.png" });
        
        // Look for login/register UI element - adjust selector based on your actual UI
        var loginElement = await _page.QuerySelectorAsync("a[href='/staff/login'], img.login-img, .login-button");
        Console.WriteLine($"Login element found: {loginElement != null}");
        Assert.NotNull(loginElement);
    }

    [When(@"I click on the register button")]
    public async Task WhenIClickOnTheRegisterButton()
    {
        // Click on the login button/image
        await _page.ClickAsync("a[href='/staff/login'], img.login-img, .login-button");
        
        // Wait for navigation to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Console.WriteLine($"After clicking login button, URL is: {_page.Url}");
    }

    [Then(@"I should see the register form")]
    public async Task ThenIShouldSeeTheLoginForm()
    {
        // Take a screenshot to debug
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "login-form.png" });
        
        // Wait for the login form to appear
        var form = await _page.WaitForSelectorAsync("form, .login-container, .staff-login-form", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
        
        Assert.NotNull(form);
        Console.WriteLine("Login form is visible");
    }

    [When(@"I fill in the form with valid data")]
    public async Task WhenIFillInTheFormWithValidData()
    {
        // Fill in the WTP login form
        await _page.FillAsync("input[name='username'], input[type='text']", "staff");
        await _page.FillAsync("input[name='password'], input[type='password']", "staff123");
        
        Console.WriteLine("Filled in login form with staff credentials");
    }

    [When(@"I click on the submit button")]
    public async Task WhenIClickOnTheSubmitButton()
    {
        // Take screenshot before submission
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "before-submit.png" });
        
        // Click submit button
        await _page.ClickAsync("button[type='submit'], .staff-login-button");
        
        // Wait for navigation or response
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Take screenshot after submission
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "after-submit.png" });
        
        Console.WriteLine($"After form submission, URL is: {_page.Url}");
    }

    [Then(@"I should see a success message")]
    public async Task ThenIShouldSeeASuccessMessage()
    {
        // Check for successful login indicators
        var dashboardElement = await _page.WaitForSelectorAsync(
            ".navbar-right, .user-menu, .staff-dashboard, .ticket-tasks", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
        
        Assert.NotNull(dashboardElement);
        Console.WriteLine("Successfully logged in and found dashboard element");
    }
}
    
