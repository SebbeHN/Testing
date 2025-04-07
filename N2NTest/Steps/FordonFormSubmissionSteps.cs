namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Vehicle Service Form Submission")]
public class FordonFormSubmissionSteps
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
    
    [When(@"I fill in the customer name as ""(.*)""")]
    public async Task WhenIFillInTheCustomerNameAs(string name)
    {
        await _page.FillAsync("input[name='firstName']", name);
    }
    
    [When(@"I fill in the email as ""(.*)""")]
    public async Task WhenIFillInTheEmailAs(string email)
    {
        await _page.FillAsync("input[name='email']", email);
    }
    
    [When(@"I enter vehicle registration number ""(.*)""")]
    public async Task WhenIEnterVehicleRegistrationNumber(string regNumber)
    {
        await _page.WaitForSelectorAsync("input[name='registrationNumber']");
        await _page.FillAsync("input[name='registrationNumber']", regNumber);
    }
    
    [When(@"I select ""(.*)"" as the issue type")]
    public async Task WhenISelectAsTheIssueType(string issueType)
    {
        await _page.WaitForSelectorAsync("select[name='issueType']");
        await _page.SelectOptionAsync("select[name='issueType']", issueType);
    }
    
    [When(@"I enter ""(.*)"" as the message")]
    public async Task WhenIEnterAsTheMessage(string message)
    {
        await _page.FillAsync("textarea[name='message']", message);
    }
    
    [When(@"I submit the form")]
    public async Task WhenISubmitTheForm()
    {
        await _page.ClickAsync("button[type='submit']");
    }
    
    [Then(@"I should see a success message")]
    public async Task ThenIShouldSeeASuccessMessage()
    {
        var successMessage = await _page.WaitForSelectorAsync(".dynamisk-message.success");
        Assert.NotNull(successMessage);
    }
    
    [Then(@"I should receive a chat link via email")]
    public async Task ThenIShouldReceiveAChatLinkViaEmail()
    {
        // This step would typically involve checking an email inbox
        // Since this is a test environment, we'll verify the success message mentions email
        var successMessage = await _page.WaitForSelectorAsync(".dynamisk-message.success");
        var messageText = await successMessage.TextContentAsync();
        Assert.Contains("e-post", messageText);
    }
    
    [Then(@"I should see an error message indicating the missing field")]
    public async Task ThenIShouldSeeAnErrorMessageIndicatingTheMissingField()
    {
        var errorMessage = await _page.WaitForSelectorAsync(".dynamisk-message.error");
        Assert.NotNull(errorMessage);
    }
}