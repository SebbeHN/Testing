namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Chat Functionality")]
public class ChatFunctionalitySteps
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;
    private string _pendingChatToken;

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

    [Given(@"I am logged in as a staff member")]
    public async Task GivenIAmLoggedInAsAStaffMember()
    {
        // Navigate to login page
        await _page.GotoAsync("http://localhost:3001/staff/login");
        Console.WriteLine("Navigated to WTP staff login page");
    
        // Take screenshot
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "staff-login.png" });
    
        // Fill in staff credentials - adjust based on your test data
        await _page.FillAsync("input[name='username'], input[type='text']", "staff");
        await _page.FillAsync("input[name='password'], input[type='password']", "staff123");
    
        // Submit login form
        await _page.ClickAsync("button[type='submit'], .staff-login-button");
    
        // Wait for login to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
        // Take screenshot after login
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "staff-after-login.png" });
    
        Console.WriteLine($"After staff login, URL is: {_page.Url}");
    
        // Verify we're logged in
        var staffElement = await _page.WaitForSelectorAsync(
            ".user-menu, .navbar-right, .staff-dashboard, .ticket-tasks", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
    
        Assert.NotNull(staffElement);
    }

    [Given(@"there is a pending chat request from a customer")]
    public async Task GivenThereIsAPendingChatRequestFromACustomer()
    {
        // This step assumes there's already a pending chat request in the system
        // For a more robust test, you could create a chat request programmatically
        // or store a token in a shared context from a previous form submission test
        
        // Here we'll simulate by creating a placeholder token
        _pendingChatToken = "test-token-123";
        
        // In a real test, you might query the API or database for a pending chat
        // or use the result from a previous form submission test
    }

    [When(@"I navigate to the staff dashboard")]
    public async Task WhenINavigateToTheStaffDashboard()
    {
        await _page.GotoAsync("http://localhost:3001/staff/dashboard");
        
        // Wait for the dashboard to load
        await _page.WaitForSelectorAsync(".ticket-tasks");
    }

    [When(@"I open the chat with the pending request")]
    public async Task WhenIOpenTheChatWithThePendingRequest()
    {
        // Click on the first chat link in the dashboard
        await _page.ClickAsync(".ticket-task-token a");
        
        // Wait for the chat modal to appear
        await _page.WaitForSelectorAsync(".chat-modal__container");
    }

    [When(@"I send a message ""(.*)""")]
    public async Task WhenISendAMessage(string message)
    {
        // Type the message
        await _page.FillAsync(".chat-modal__input-field", message);
        
        // Click the send button
        await _page.ClickAsync(".chat-modal__send-button");
    }

    [Then(@"the message should appear in the chat window")]
    public async Task ThenTheMessageShouldAppearInTheChatWindow()
    {
        // Wait for the message to appear
        var messageElement = await _page.WaitForSelectorAsync(".chat-modal__message--sent");
        
        // Verify the message text
        var messageText = await messageElement.TextContentAsync();
        Assert.Contains("How can I help you today?", messageText);
    }
}