namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Admin User Management")]
public class AdminUserManagementSteps
{
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

    [Given(@"I am logged in as an admin")]
    public async Task GivenIAmLoggedInAsAnAdmin()
    {
        // Navigate to login page
        await _page.GotoAsync("http://localhost:3001/staff/login");
        Console.WriteLine("Navigated to staff login page");
    
        // Take screenshot to verify page
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "admin-login-page.png" });
    
        // Fill in admin credentials for WTP
        await _page.FillAsync("input[name='username'], input[type='text']", "admin");
        await _page.FillAsync("input[name='password'], input[type='password']", "admin321");
    
        // Submit login form
        await _page.ClickAsync("button[type='submit'], .staff-login-button");
    
        // Wait for login to complete and dashboard to load
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
        // Take screenshot after login
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "admin-after-login.png" });
    
        Console.WriteLine($"After admin login, URL is: {_page.Url}");
    
        // Verify we're logged in as admin
        var adminElement = await _page.WaitForSelectorAsync(
            ".user-menu, .navbar-right, a[href='/admin/dashboard']", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
    
        Assert.NotNull(adminElement);
    }

    [When(@"I navigate to the admin create user page")]
    public async Task WhenINavigateToTheAdminCreateUserPage()
    {
        await _page.GotoAsync("http://localhost:3001/admin/create-user");
    }

    [When(@"I fill in the user email as ""(.*)""")]
    public async Task WhenIFillInTheUserEmailAs(string email)
    {
        await _page.FillAsync("input[name='email']", email);
    }

    [When(@"I fill in the user name as ""(.*)""")]
    public async Task WhenIFillInTheUserNameAs(string name)
    {
        await _page.FillAsync("input[name='firstName']", name);
    }

    [When(@"I fill in the password as ""(.*)""")]
    public async Task WhenIFillInThePasswordAs(string password)
    {
        await _page.FillAsync("input[name='password']", password);
    }

    [When(@"I select ""(.*)"" as the company")]
    public async Task WhenISelectAsTheCompany(string company)
    {
        await _page.SelectOptionAsync("select[name='company']", company);
    }

    [When(@"I select ""(.*)"" as the role")]
    public async Task WhenISelectAsTheRole(string role)
    {
        await _page.SelectOptionAsync("select[name='role']", role);
    }

    [When(@"I click the ""(.*)"" button")]
    public async Task WhenIClickTheButton(string buttonText)
    {
        await _page.ClickAsync($"button:has-text('{buttonText}')");
    }

    [Then(@"I should see a success message")]
    public async Task ThenIShouldSeeASuccessMessage()
    {
        var successMessage = await _page.WaitForSelectorAsync(".success-message, div:has-text('Användare skapades framgångsrikt')");
        Assert.NotNull(successMessage);
    }
}