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
    try
    {
        // Vänta på att chattrutan ska vara helt laddad
        await PlaywrightSetup.WaitForElementSafely(_page, ".chat-modal", PlaywrightSetup.DefaultTimeout);
        
        // Ta en skärmdump för felsökning
        await _page.ScreenshotAsync(new() { Path = "chat-before-check.png" });
        
        // Vänta lite extra för att säkerställa att chattmeddelanden har tid att renderas
        await Task.Delay(3000);
        
        // Hämta alla chattmeddelanden för loggning
        var allMessages = await _page.EvaluateAsync<string[]>(@"() => {
            return Array.from(document.querySelectorAll('.chat-modal__message-text'))
                    .map(el => el.textContent.trim());
        }");
        
        Console.WriteLine("Alla chattmeddelanden:");
        foreach (var msg in allMessages)
        {
            Console.WriteLine($"  - '{msg}'");
        }
        
        // Sök efter vårt meddelande med olika metoder
        int maxRetries = 3;
        bool messageFound = false;
        
        for (int retry = 0; retry < maxRetries && !messageFound; retry++)
        {
            if (retry > 0)
            {
                Console.WriteLine($"Försök {retry+1} att hitta chattmeddelande...");
                await Task.Delay(3000);
            }
            
            try
            {
                // Metod 1: Använd Playwright's selektorer
                var messageSelector = ".chat-modal__message-text";
                var messageCount = await _page.Locator(messageSelector).CountAsync();
                
                if (messageCount > 0)
                {
                    for (int i = 0; i < messageCount; i++)
                    {
                        var messageText = await _page.Locator(messageSelector).Nth(i).TextContentAsync();
                        if (messageText.Contains("Vad kan jag hjälpa dig med?"))
                        {
                            messageFound = true;
                            Console.WriteLine($"Hittade meddelande med index {i}: '{messageText}'");
                            break;
                        }
                    }
                }
                
                // Metod 2: Om vi fortfarande inte hittat meddelandet, använd JavaScript
                if (!messageFound)
                {
                    messageFound = await _page.EvaluateAsync<bool>(@"() => {
                        const messages = Array.from(document.querySelectorAll('.chat-modal__message-text'));
                        return messages.some(el => el.textContent.includes('Vad kan jag hjälpa dig med?'));
                    }");
                    
                    if (messageFound)
                    {
                        Console.WriteLine("Hittade meddelande via JavaScript");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid sökning efter chattmeddelande (försök {retry+1}): {ex.Message}");
                await Task.Delay(1000);
            }
        }
        
        // Ta en slutlig skärmdump
        await _page.ScreenshotAsync(new() { Path = "chat-final-state.png" });
        
        Assert.True(messageFound, "Kunde inte hitta 'Vad kan jag hjälpa dig med?' i chattmeddelanden");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fel vid kontroll av chattmeddelande: {ex.Message}");
        await _page.ScreenshotAsync(new() { Path = "chat-error.png" });
        throw;
    }
}

}

