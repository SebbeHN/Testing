namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;

[Binding]
[Scope(Feature = "Add to cart at Shoptester")]
public class AddToCartSteps
{

    // SETUP:

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
    
    // Steps

    [Given(@"I am on Shoptester homepage")]
    async Task GivenIAmOnShoptesterHomepage()
    {
        await _page.GotoAsync("http://localhost:5000");
    }
    
    [Given(@"I am logged in")]
    public async Task GivenIAmLoggedIn()
    {
        // Navigate to the login page
        await _page.GotoAsync("http://localhost:5000");
    
        // Click on the Login button
        await _page.ClickAsync("text=Login");
    
        // Fill in login credentials
        await _page.FillAsync("input[name='email'], input[type='email']", "admin@admin.com");
        await _page.FillAsync("input[name='password'], input[type='password']", "admin123");
    
        // Submit the form
        await _page.ClickAsync("button[type='submit'], input[type='submit'], button:has-text('Submit')");
    
        // Verify successful login
        await _page.WaitForSelectorAsync(".user-profile, .logged-in-indicator, .welcome-message, .dashboard");
    }

    [Given(@"I am on the product page")]
    public async Task GivenIAmOnTheProductPage()
    {
        // Navigate to shop page
        await _page.ClickAsync("text=Shop");
    
        // Wait for products to load
        await _page.WaitForSelectorAsync(".products, .product-list, .shop-items");
    }
    
    [When(@"I click on the ""(.*)"" button")]
    public async Task WhenIClickOnTheButton(string buttonName)
    {
        await _page.ClickAsync($"text={buttonName}");
    }

    [Then(@"I should see the product in my cart")]
    public async Task ThenIShouldSeeTheProductInMyCart()
    {
        // Navigate to cart page or open cart modal
        await _page.ClickAsync("a:has-text('Cart'), button:has-text('Cart')");
    
        // Wait for cart to load
        await _page.WaitForSelectorAsync(".cart-items, .cart-container");
    
        // Verify at least one product is in cart
        var cartItem = await _page.QuerySelectorAsync(".cart-item");
        Assert.NotNull(cartItem);
    }

[Then(@"The cart count should be updated")]
public async Task ThenTheCartCountShouldBeUpdated()
{
    // Verify cart count is greater than zero
    var cartCount = await _page.QuerySelectorAsync(".cart-count");
    var countText = await cartCount.TextContentAsync();
    Assert.NotEqual("0", countText);
}
    
}