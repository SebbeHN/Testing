namespace N2NTest.Steps;
using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Register a user at Shoptester")]
public class RegisterSteps
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
    
    // Replace your existing step definitions with these

    [Given(@"I am on Shoptester homepage")]
    public async Task GivenIAmOnShoptester()
    {
        await _page.GotoAsync("http://localhost:5000");
    }

    [Given(@"I see the ""(.*)"" button")]
    public async Task GivenISeeTheButton(string buttonName)
    {
        var registerButton = await _page.QuerySelectorAsync($"text={buttonName}");
        Assert.NotNull(registerButton);
    }

    [When(@"I click on the ""(.*)"" button")]
    public async Task WhenIClickOnTheButton(string buttonName)
    {
        await _page.ClickAsync($"text={buttonName}");
    }

    [Then(@"I should see the registration form")]
    public async Task ThenIShouldSeeTheRegistrationForm()
    {
        // Wait for the form or its container to appear
        await _page.WaitForSelectorAsync(".registration-form, #registration-form, form", 
            new PageWaitForSelectorOptions { Timeout = 5000 });
    
        // Check for various possible selectors
        var registrationForm = await _page.QuerySelectorAsync(
            ".registration-form, #registration-form, form, [data-testid='registration-form']");
    
        Assert.NotNull(registrationForm);
    }
        
    
        
            
   
}