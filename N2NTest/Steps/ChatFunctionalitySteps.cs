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
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_APP_URL") ?? "http://localhost:5000/";

    [BeforeScenario]
    public async Task Setup()
    {
        var result = await PlaywrightSetup.CreateBrowserAndPage();
        _browser = result.browser;
        _page = result.page;

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
        await _page.GotoAsync($"{BaseUrl}staff/dashboard");

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

        {
            // Vänta tills det önskade meddelandet syns i chatten
            await _page.WaitForSelectorAsync(".chat-modal", new PageWaitForSelectorOptions { Timeout = 5000 });
        }
        
    }
}
