using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using N2NTest.Helpers;
using System.Threading.Tasks;

namespace E2ETesting.Steps;

[Binding]
[Scope(Feature = "Staff Dashboard")]
public class TicketManagementSteps
{
    private IPage _page;
    private IBrowser _browser;
    private ILocator _ticket;
    private char _ticketText;
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_APP_URL") ?? "http://localhost:5000/";


    [BeforeScenario]
    public async Task Setup()
    {
        (_browser, _page) = await PlaywrightService.CreateNewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        await PlaywrightService.ClosePageAsync(_browser);
    }

    [Given("I am logged in as staff")]
    public async Task GivenIAmLoggedInAsStaff()
    {
        await LoginHelper.LoginAsRole(_page, "staff");

        try
        {
            await _page.WaitForURLAsync($"{BaseUrl}/staff/dashboard", new() { Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "login-failed.png" });
            throw;
        }
    }

    [Given("I navigate to the staff dashboard")]
    public async Task GivenINavigateToTheStaffDashboard()
    {
        await _page.ClickAsync("a[href='/staff/dashboard']");
        await _page.WaitForURLAsync("**/staff/dashboard", new() { Timeout = 5000 });
        Assert.Contains("/staff/dashboard", _page.Url);
    }

    [Given(@"I see a ticket in the ""Ärenden"" column")]
    public async Task GivenISeeATicketInArenden()
    {
        _ticket = _page.Locator("div.ticket-tasks div.ticket-task-item").First;
        await _ticket.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    [When(@"I drag the ticket to the ""Mina ärenden"" column")]
    public async Task WhenIDragTheTicketToMinaArenden()
    {
        var target = _page.Locator("div.ticket-my-tasks");
        await _ticket.DragToAsync(target);
        await _page.WaitForTimeoutAsync(3000);
    }

    [When(@"I drag the same ticket to the ""Klara"" column")]
    public async Task WhenIDragTheTicketToKlara()
    {
        var target = _page.Locator("div.ticket-done");
        await _ticket.DragToAsync(target);
        await _page.WaitForTimeoutAsync(3000);
    }

    [Then(@"the ticket should appear in the ""Klara"" column")]
    public async Task ThenTicketShouldBeInKlara()
    {
        var ticketsInKlara = _page.Locator("div.ticket-done div.ticket-task-item");
        await ticketsInKlara.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        int count = await ticketsInKlara.CountAsync();

        Assert.True(count > 0, "No tickets found in 'Klara' column.");
    }

    [Then(@"the ticket should not appear in the ""Ärenden"" or ""Mina ärenden"" columns")]
    public async Task ThenTicketShouldNotBeInPreviousColumns()
    {
        var arendenTickets = _page.Locator("div.ticket-tasks div.ticket-task-item");
        
        var minaArendenTickets = _page.Locator("div.ticket-my-tasks div.ticket-task-item");

        var arendenText = await _page.Locator("div.ticket-tasks").InnerTextAsync();
        Assert.DoesNotContain(_ticketText, arendenText);

        var minaText = await _page.Locator("div.ticket-my-tasks").InnerTextAsync();
        Assert.DoesNotContain(_ticketText, minaText);

    }
}

public static class PlaywrightService
{
    public static async Task<(IBrowser browser, IPage page)> CreateNewPageAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // true = kör i bakgrunden
        });

        var page = await browser.NewPageAsync();
        return (browser, page);
    }

    public static async Task ClosePageAsync(IBrowser browser)
    {
        if (browser != null)
        {
            await browser.CloseAsync();
        }
    }
}
