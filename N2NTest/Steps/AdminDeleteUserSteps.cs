using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using System.Threading.Tasks;
using N2NTest.Helpers;

namespace N2NTest.Steps;

[Binding]
[Scope(Feature = "Admin deletes a user")]
public class AdminDeleteUserSteps
{
    private IPage _page;
    private IBrowser _browser;
    private IPlaywright _playwright;
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_APP_URL") ?? "http://localhost:5000/";

    [BeforeScenario]
    public async Task Setup()
    {
        var result = await PlaywrightSetup.CreateBrowserAndPage();
        _browser = result.browser;
        _page = result.page;

        await Task.Delay(3000);
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
        await Task.Delay(2000);

        var response = await _page.GotoAsync($"{BaseUrl}staff/login");
        await Task.Delay(3000);

        if (response == null || !response.Ok)
            throw new Exception("Login page failed to load");

        await Task.Delay(2000);

        string[] emailSelectors = {
            "input[type='email']",
            "input[name='email']",
            "input[id*='email' i]",
            "input[placeholder*='email' i]",
            "input:first-of-type"
        };

        string usedEmailSelector = null;
        foreach (var selector in emailSelectors)
        {
            if (await _page.QuerySelectorAsync(selector) != null)
            {
                usedEmailSelector = selector;
                break;
            }
        }

        if (usedEmailSelector == null)
            throw new Exception("Could not find email input");

        await _page.FillAsync(usedEmailSelector, "admin@admin.com");
        await Task.Delay(1000);

        string[] passwordSelectors = {
            "input[type='password']",
            "input[name='password']",
            "input[id*='password' i]",
            "input[placeholder*='password' i]"
        };

        string usedPasswordSelector = null;
        foreach (var selector in passwordSelectors)
        {
            if (await _page.QuerySelectorAsync(selector) != null)
            {
                usedPasswordSelector = selector;
                break;
            }
        }

        if (usedPasswordSelector == null)
            throw new Exception("Could not find password input");

        await _page.FillAsync(usedPasswordSelector, "admin321");
        await Task.Delay(1000);

        string[] buttonSelectors = {
            "button[type='submit']",
            "button:text('Logga in')",
            "input[type='submit']",
            "button"
        };

        string usedButtonSelector = null;
        foreach (var selector in buttonSelectors)
        {
            if (await _page.QuerySelectorAsync(selector) != null)
            {
                usedButtonSelector = selector;
                break;
            }
        }

        if (usedButtonSelector == null)
            throw new Exception("Could not find submit button");

        await Task.Delay(2000);
        await _page.ClickAsync(usedButtonSelector);
        await Task.Delay(5000);

        await _page.WaitForURLAsync("**/admin/dashboard*", new() { Timeout = 30000 });
        await Task.Delay(5000);

        await _page.WaitForSelectorAsync("table", new() { Timeout = 20000 });
        await Task.Delay(5000);
    }

    [When(@"I delete the user with email ""(.*)""")]
    public async Task WhenIDeleteUserWithEmail(string email)
    {
        try
        {
            await _page.ScreenshotAsync(new() { Path = "before-delete.png" });
            await Task.Delay(5000);

            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await Task.Delay(8000);
            await _page.ScreenshotAsync(new() { Path = "table-before-searching.png" });

            var userExists = await _page.EvaluateAsync<bool>(@"(email) => {
                const rows = Array.from(document.querySelectorAll('table tr'));
                return rows.some(row => row.textContent.includes(email));
            }", email);

            if (!userExists)
            {
                await _page.ReloadAsync();
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(10000);

                userExists = await _page.EvaluateAsync<bool>(@"(email) => {
                    const rows = Array.from(document.querySelectorAll('table tr'));
                    return rows.some(row => row.textContent.includes(email));
                }", email);

                if (!userExists)
                    throw new Exception($"Användare med e-post {email} hittades inte i tabellen!");
            }

            await Task.Delay(5000);

            _page.Dialog += async (_, dialog) =>
            {
                await dialog.AcceptAsync();
            };

            var success = await _page.EvaluateAsync<bool>(@"(email) => {
                try {
                    const rows = Array.from(document.querySelectorAll('table tr'));
                    const userRow = rows.find(row => row.textContent.includes(email));
                    if (userRow) {
                        const deleteButton = userRow.querySelector('button');
                        if (deleteButton) {
                            deleteButton.click();
                            return true;
                        }
                    }
                    return false;
                } catch {
                    return false;
                }
            }", email);

            if (!success)
            {
                var row = _page.Locator("table tr").Filter(new() { HasText = email });
                await row.WaitForAsync(new() { Timeout = PlaywrightSetup.DefaultTimeout });
                await Task.Delay(3000);

                var deleteButton = row.Locator("button").Nth(1); // Assuming second button = delete
                await deleteButton.ClickAsync(new() { Timeout = PlaywrightSetup.DefaultTimeout });
            }

            await Task.Delay(3000);
            await _page.ScreenshotAsync(new() { Path = "after-delete-button-click.png" });

            await Task.Delay(10000);
            await _page.ScreenshotAsync(new() { Path = "after-delete-complete.png" });
        }
        catch (Exception)
        {
            await _page.ScreenshotAsync(new() { Path = "delete-error.png" });
            throw;
        }
    }

    [Then(@"the user is deleted successfully")]
    public async Task ThenTheUserIsDeletedSuccessfully()
    {
        try
        {
            await Task.Delay(5000);
            await _page.ReloadAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(8000);
            await _page.ScreenshotAsync(new() { Path = "table-after-reload.png" });

            string email = "newstaff@example.com";

            var userExists = await _page.EvaluateAsync<bool>(@"(email) => {
                const rows = Array.from(document.querySelectorAll('table tr'));
                return rows.some(row => row.textContent.includes(email));
            }", email);

            await Task.Delay(3000);
            Assert.False(userExists, $"User with email {email} should not be present after deletion");

            await _page.ScreenshotAsync(new() { Path = "user-deleted-verification.png" });
        }
        catch (Exception)
        {
            await _page.ScreenshotAsync(new() { Path = "verification-error.png" });
            throw;
        }
    }
}
