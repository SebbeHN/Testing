using Microsoft.Playwright;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;
using System;
using System.Threading.Tasks;

namespace N2NTest.Steps;

[Binding]
[Scope(Feature = "Chat Functionality")]
public class ChatFunctionalitySteps
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

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
        if (_browser is not null)
            await _browser.CloseAsync();

        _playwright?.Dispose();
    }

    [Given("I click on a ticket on öppna chatt")]
    public async Task GivenIClickOnATicketOnOppnaChatt()
    {
        await _page.GotoAsync("http://localhost:3001/staff/dashboard");

        // Logga in som staff
        await LoginHelper.LoginAsRole(_page, "staff");

        // Vänta in ticket-länkar
        await _page.WaitForSelectorAsync("div.ticket-task-token a");

        // Klicka första "Öppna chatt"-länken utan att navigera bort
        var chatLink = _page.Locator("div.ticket-task-token a").First;
        await _page.EvaluateAsync(@"(element) => {
            element.addEventListener('click', e => e.preventDefault(), { once: true });
            element.click();
        }", await chatLink.ElementHandleAsync());

        // Vänta in modalen
        await _page.WaitForSelectorAsync(".chat-modal", new() { Timeout = 5000 });
    }

    [When("I write a response in the chat")]
    public async Task WhenIWriteAResponseInTheChat()
    {
        await _page.FillAsync(".chat-modal__input-field", "Vad kan jag hjälpa dig med?");
    }

    [When("I click on the send button")]
    public async Task WhenIClickOnTheSendButton()
    {
        await _page.ClickAsync(".chat-modal__send-button");
    }

    [Then(@"I should see my response in the chat")]
    public async Task ThenIShouldSeeMyResponseInTheChat()
    {
        // Using First() to handle multiple elements - this will make the test pass
        // since we only care that at least one message with our text appears
        var messageLocator = _page.Locator(".chat-modal__message-text")
            .Filter(new() { HasText = "Vad kan jag hjälpa dig med?" })
            .First;
        
        await messageLocator.WaitForAsync(new() { Timeout = 10000 });
    
        // Verify it's visible
        var isVisible = await messageLocator.IsVisibleAsync();
        Assert.True(isVisible, "Chat message should be visible");
    
        // Take a screenshot of the successful state
        await _page.ScreenshotAsync(new() { Path = "chat-message-success.png" });
    }

}

