namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "CRM Form Submission")]
public class TeleSubmissionSteps
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
    
    [Given(@"I am on the dynamic form page")]
    public async Task GivenIAmOnTheDynamicFormPage()
    {
        await _page.GotoAsync("http://localhost:3001/dynamisk");
    }
    
    [When(@"I select ""(.*)"" as the company type")]
    public async Task WhenISelectAsTheCompanyType(string companyType)
    {
        await _page.SelectOptionAsync("select[name='companyType']", companyType);
    }
    
    // Implement other steps similarly
    
    [Then(@"I should see a success message")]
    public async Task ThenIShouldSeeASuccessMessage()
    {
        var successMessage = await _page.WaitForSelectorAsync(".dynamisk-message.success");
        Assert.NotNull(successMessage);
    }
}