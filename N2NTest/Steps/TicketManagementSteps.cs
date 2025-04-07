namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Staff Dashboard")]
public class TicketManagementSteps
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

    [Given(@"I am logged in as a staff member")]
    public async Task GivenIAmLoggedInAsAStaffMember()
    {
        // Navigate to login page
        await _page.GotoAsync("http://localhost:3001/staff/login");
        Console.WriteLine("Navigated to WTP staff login page for ticket management test");
    
        // Take screenshot
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "ticket-staff-login.png" });
    
        // Fill in staff credentials
        await _page.FillAsync("input[name='username'], input[type='text']", "staff");
        await _page.FillAsync("input[name='password'], input[type='password']", "staff123");
    
        // Submit login form
        await _page.ClickAsync("button[type='submit'], .staff-login-button");
    
        // Wait for login to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
        // Take screenshot after login
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "ticket-staff-after-login.png" });
    
        Console.WriteLine($"After staff login for ticket management, URL is: {_page.Url}");
    
        // Verify we're logged in by checking for dashboard elements
        var dashboardElement = await _page.WaitForSelectorAsync(".ticket-tasks, .main-container", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
    
        Assert.NotNull(dashboardElement);
    }

    [When(@"I navigate to the staff dashboard")]
    public async Task WhenINavigateToTheStaffDashboard()
    {
        await _page.GotoAsync("http://localhost:3001/staff/dashboard");
        
        // Wait for the dashboard to load with increased timeout
        await _page.WaitForSelectorAsync(".ticket-tasks, .main-container", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
    }

    [When(@"I drag a ticket from ""(.*)"" to ""(.*)""")]
    public async Task WhenIDragATicketFromTo(string source, string target)
    {
        // Find a ticket in the source column
        var sourceTicket = await _page.WaitForSelectorAsync(
            source == "Ärenden" ? ".ticket-tasks .ticket-task-item" : ".ticket-my-tasks .ticket-task-item", 
            new PageWaitForSelectorOptions { Timeout = 60000 });
        
        // Find the target drop area
        var targetArea = await _page.QuerySelectorAsync(
            target == "Mina ärenden" ? ".ticket-my-tasks" : ".ticket-tasks");
        
        if (sourceTicket != null && targetArea != null)
        {
            // Get bounding boxes for the elements
            var sourceBox = await sourceTicket.BoundingBoxAsync();
            var targetBox = await targetArea.BoundingBoxAsync();
            
            if (sourceBox != null && targetBox != null)
            {
                // Perform the drag and drop
                await _page.Mouse.MoveAsync(
                    sourceBox.X + sourceBox.Width / 2,
                    sourceBox.Y + sourceBox.Height / 2);
                await _page.Mouse.DownAsync();
                await _page.Mouse.MoveAsync(
                    targetBox.X + targetBox.Width / 2,
                    targetBox.Y + targetBox.Height / 2);
                await _page.Mouse.UpAsync();
                
                // Wait for the drag effect to complete
                await _page.WaitForTimeoutAsync(1000);
            }
        }
    }

    [Then(@"the ticket should appear in the ""(.*)"" column")]
    public async Task ThenTheTicketShouldAppearInTheColumn(string targetColumn)
    {
        // Check if a ticket exists in the target column
        var targetSelector = targetColumn == "Mina ärenden" ? 
            ".ticket-my-tasks .ticket-task-item" : 
            ".ticket-done .ticket-task-item";
        
        var ticketInTarget = await _page.WaitForSelectorAsync(targetSelector,
            new PageWaitForSelectorOptions { Timeout = 60000 });
        
        Assert.NotNull(ticketInTarget);
    }
}