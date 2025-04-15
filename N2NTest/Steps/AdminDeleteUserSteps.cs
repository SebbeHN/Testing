using Microsoft.Playwright;
using N2NTest.Helpers;
using TechTalk.SpecFlow;
using Xunit;
using System.Threading.Tasks;
using E2ETesting.Steps;

namespace N2NTest.Steps;

[Binding]
[Scope(Feature = "Admin deletes a user")]
public class AdminDeleteUserSteps
{
    private IPage _page;
    private IBrowser _browser;
    private IPlaywright _playwright;

    [BeforeScenario]
    public async Task Setup()
    {
        var result = await PlaywrightService.CreateNewPageAsync();
       
        
        _browser = result.browser;
        _page = result.page;
        
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser is not null)
            await _browser.CloseAsync();
    }

    [Given("I am logged in as an admin")]
    public async Task GivenIAmLoggedInAsAnAdmin()
    {
        await _page.GotoAsync("http://localhost:3001/admin/dashboard");
        await LoginHelper.LoginAsRole(_page, "admin");
        await _page.WaitForURLAsync("**/admin/dashboard", new() { Timeout = 5000 });

        // Vänta på att användartabellen ska visas
        await _page.WaitForSelectorAsync("table", new() { Timeout = 5000 });
    }

    [When(@"I delete the user with email ""(.*)""")]
    public async Task WhenIDeleteUserWithEmail(string email)
    {
        var row = _page.Locator("table tr").Filter(new() { HasTextString = email });
        await row.WaitForAsync(new() { Timeout = 10000 });

        // Hantera både confirm och alert
        _page.Dialog += async (_, dialog) =>
        {
            Console.WriteLine($"[Dialog] Typ: {dialog.Type} – Meddelande: {dialog.Message}");
            await Task.Delay(800);
            await dialog.AcceptAsync();
        };

        var deleteButton = row.Locator("button:has-text('Ta bort')");
        await Task.Delay(1000); // Paus innan klick
        await deleteButton.ClickAsync();

        await _page.WaitForTimeoutAsync(1500); // Paus efter klick
    }


    [Then(@"the user with email ""(.*)"" should no longer be visible")]
    public async Task ThenUserShouldBeGone(string email)
    {
        var row = _page.Locator("table tr").Filter(new() { HasTextString = email });
        var count = await row.CountAsync();

        Assert.Equal(0, count);
    }
}
