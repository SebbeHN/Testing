using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace N2NTest.Helpers
{
    public static class PlaywrightSetup
    {
        // Kontrollera om vi kör i CI-miljö
        public static bool IsRunningInCI => Environment.GetEnvironmentVariable("CI") != null;
        
        // Anpassa timeouts baserat på körmiljö
        public static int DefaultTimeout => IsRunningInCI ? 60000 : 30000;
        public static int NavigationTimeout => IsRunningInCI ? 45000 : 20000;
        
        // Gemensam metod för att skapa browser och page
        public static async Task<(IBrowser browser, IPage page)> CreateBrowserAndPage()
        {
            var playwright = await Playwright.CreateAsync();
            
            // Använd alltid headless i CI, valbart lokalt
            bool headless = IsRunningInCI ? true : false;
            
            // Justera SlowMo baserat på miljö
            int slowMo = IsRunningInCI ? 100 : 300;
            
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                SlowMo = slowMo,
                Timeout = DefaultTimeout
            });
            
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
                AcceptDownloads = true
            });
            
            var page = await context.NewPageAsync();
            
            // Ställ in lämpliga timeouts
            page.SetDefaultTimeout(DefaultTimeout);
            page.SetDefaultNavigationTimeout(NavigationTimeout);
            
            // Lägg till nätverksoptimering för CI
            if (IsRunningInCI)
            {
                await context.RouteAsync("**/*", async route =>
                {
                    // Ignorera bilder och andra icke-kritiska resurser i CI
                    var request = route.Request;
                    if (request.ResourceType == "image" || 
                        request.ResourceType == "font" || 
                        request.ResourceType == "stylesheet")
                    {
                        await route.AbortAsync();
                    }
                    else
                    {
                        await route.ContinueAsync();
                    }
                });
            }
            
            return (browser, page);
        }
        
        // Hjälpmetod för att vänta på element på ett robust sätt
        public static async Task WaitForElementSafely(IPage page, string selector, int? timeout = null)
        {
            timeout = timeout ?? DefaultTimeout;
            
            try
            {
                await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = timeout.Value
                });
            }
            catch (TimeoutException)
            {
                // Ta skärmdump vid timeout
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = $"timeout-{DateTime.Now.Ticks}-{selector.Replace(':', '-')}.png",
                    FullPage = true
                });
                
                // Logg htmlkod för felsökning
                var html = await page.ContentAsync();
                System.IO.File.WriteAllText($"page-dump-{DateTime.Now.Ticks}.html", html);
                
                throw;
            }
        }
        
        // Hjälpmetod för mer pålitliga klick
        public static async Task ClickSafely(IPage page, string selector, int retries = 3)
        {
            Exception lastException = null;
            
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    // Vänta på elementet
                    await WaitForElementSafely(page, selector);
                    
                    // Försök klicka
                    await page.ClickAsync(selector, new PageClickOptions
                    {
                        Force = i > 0, // Använd force om det är ett nytt försök
                        Timeout = DefaultTimeout
                    });
                    
                    // Om vi kommer hit utan exception, lyckat klick
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // Vänta lite innan nytt försök
                    await Task.Delay(1000 * (i + 1));
                    
                    // Ta skärmdump vid fel
                    await page.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = $"click-retry-{i}-{DateTime.Now.Ticks}-{selector.Replace(':', '-')}.png"
                    });
                    
                    Console.WriteLine($"Försök {i+1} att klicka på {selector} misslyckades: {ex.Message}");
                }
            }
            
            // Om vi kommer hit har alla försök misslyckats
            throw new Exception($"Kunde inte klicka på {selector} efter {retries} försök", lastException);
        }
    }
}