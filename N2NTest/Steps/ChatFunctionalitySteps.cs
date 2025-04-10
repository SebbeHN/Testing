namespace N2NTest.Steps;

using Microsoft.Playwright;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;
using System;
using System.Threading.Tasks;

[Binding]
[Scope(Feature = "Chat Functionality")]
public class ChatFunctionalitySteps
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private string? _messageText;

    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false, SlowMo = 500 });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    private async Task WaitForPageToLoad()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(1000);
    }

    [Given(@"I am logged in as a staff member")]
    public async Task GivenIAmLoggedInAsAStaffMember()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        await LoginHelper.LoginAsRole(_page, "staff");
        await WaitForPageToLoad();
    }

    [Given(@"there is a pending chat request from a customer")]
    public async Task GivenThereIsPendingChatRequest()
    {
        await Task.Delay(500); // Simulerad väntan – justera som behövs
    }

    [When(@"I navigate to the staff dashboard")]
    public async Task WhenINavigateToTheStaffDashboard()
    {
        await _page.GotoAsync("http://localhost:3001/staff/dashboard");
        await WaitForPageToLoad();
    }

    [When(@"I open the chat with the pending request by clicking Öppna chatt")]
    public async Task WhenIOpenTheChat()
    {
        await _page.ClickAsync("text=Öppna chatt");
        await WaitForPageToLoad();
    }

    [When(@"I send a message ""(.*)""")]
    public async Task WhenISendAMessage(string message)
    {
        _messageText = message;
        await _page.FillAsync("textarea", message);
        await _page.ClickAsync("button:has-text('Send')");
        await Task.Delay(500);
    }

    [Then(@"the message should appear in the chat window")]
    public async Task ThenMessageShouldAppear()
    {
        var lastMessage = await _page.InnerTextAsync(".chat-message:last-child");
        Assert.Contains(_messageText, lastMessage);
    }
}
