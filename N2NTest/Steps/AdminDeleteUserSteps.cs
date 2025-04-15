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
        var row = _page.Locator("table tr").Filter(new() { HasText = email });

        // 🔍 Debug innan vi väntar
        Console.WriteLine(await _page.ContentAsync());
        await _page.ScreenshotAsync(new() { Path = $"debug-{Guid.NewGuid()}.png" });

        await row.WaitForAsync(new() { Timeout = 10000 }); // 👈 detta ska vara kvar!
    
        var deleteButton = row.Locator("button", new() { HasTextString = "Ta bort" });
        await deleteButton.ClickAsync();
    

        await _page.WaitForTimeoutAsync(1500); // Paus efter klick
    }


    [Then(@"the user with email ""(.*)"" should no longer be visible")]
    public async Task ThenUserShouldBeGone(string email)
    {
        await _page.ReloadAsync();

        var rows = await _page.Locator("table tr").Filter(new() { HasText = email }).CountAsync();

        Assert.Equal(0, rows);
    }

}
