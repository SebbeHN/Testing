namespace N2NTest.Steps;

using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Xunit;
using System;
using System.Threading.Tasks;

[Binding]
[Scope(Feature = "Chat Functionality")]
public class ChatFunctionalitySteps
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private string? _messageText;

    [BeforeScenario]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();

        // Öka timeout för hela testet
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        { 
            Headless = false, 
            SlowMo = 500,
            Timeout = 60000 // 60 sekunder timeout
        });

        _context = await _browser.NewContextAsync();

        // Sätt timeout efter att context har skapats
        if (_context != null)
        {
            _context.SetDefaultTimeout(60000); // 60 sekunder timeout för alla operationer
            _context.SetDefaultNavigationTimeout(60000); // 60 sekunder för navigationer
        }

        _page = await _context.NewPageAsync();
    }

    [AfterScenario]
    public async Task Teardown()
    {
        if (_browser != null)
            await _browser.CloseAsync();
        if (_playwright != null)
            _playwright.Dispose();
    }

    // Hjälpfunktion för att vänta på att sidan ska laddas helt
    private async Task WaitForPageToLoad()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Vänta lite extra för att säkerställa att allt har laddats
        await Task.Delay(1000);
        
        // Ta en skärmdump för diagnostik
        await _page.ScreenshotAsync(new PageScreenshotOptions { 
            Path = $"chat-page-loaded-{DateTime.Now:yyyyMMdd-HHmmss}.png",
            FullPage = true
        });
    }

    [Given(@"I am logged in as a staff member")]
    public async Task GivenIAmLoggedInAsAStaffMember()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");

        try
        {
            Console.WriteLine("Navigating to login page...");
            await _page.GotoAsync("http://localhost:3001/staff/login");
            await WaitForPageToLoad();

            Console.WriteLine("Filling in staff credentials...");
            await _page.FillAsync("input[type='text'], input[name='username']", "staff");
            await _page.FillAsync("input[type='password'], input[name='password']", "staff123");

            Console.WriteLine("Clicking login button...");
            await _page.ClickAsync("button[type='submit'], .login-button");

            // Vänta extra på eventuell omdirigering eller laddning
            await Task.Delay(3000);
            await WaitForPageToLoad();

            // Logga URL efter inloggning
            string currentUrl = _page.Url;
            Console.WriteLine($"URL after login: {currentUrl}");

            // Ta skärmdump för felsökning
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "after-staff-login-attempt.png",
                FullPage = true
            });

            // Kontrollera att vi verkligen är inloggade (exempelvis via dashboard)
            if (!currentUrl.Contains("dashboard") && !currentUrl.Contains("staff"))
            {
                throw new Exception("Login failed or not redirected to dashboard.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "staff-login-error.png",
                FullPage = true
            });

            // Kasta om vi vill att testet ska faila – eller kommentera ut för att gå vidare ändå
            throw;
        }
    }

    [Given(@"there is a pending chat request from a customer")]
    public async Task GivenThereIsAPendingChatRequestFromACustomer()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        // Detta steg är svårt att simulera direkt i ett E2E-test
        // Vi antar att det finns en förfrågan i systemet eller fortsätter med testet
        
        try
        {
            Console.WriteLine("Checking for pending chat requests...");
            
            // Ta skärmdump för att se hur dashboarden ser ut
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "staff-dashboard-pending-chats.png",
                FullPage = true
            });
            
            // Försök hitta någon indikation på en chattförfrågan
            var pendingChats = await _page.QuerySelectorAllAsync(".chat-request, .pending-request, tr:has-text('chatt')");
            
            Console.WriteLine($"Found {pendingChats.Count} potential chat requests");
            
            // Om vi inte hittar något, loggar vi bara detta men fortsätter med testet
            if (pendingChats.Count == 0)
            {
                Console.WriteLine("No pending chat requests found, but continuing test");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking for pending chats: {ex.Message}");
        }
    }

    [When(@"I navigate to the staff dashboard")]
    public async Task WhenINavigateToTheStaffDashboard()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        try
        {
            Console.WriteLine("Navigating to staff dashboard...");
            await _page.GotoAsync("http://localhost:3001/staff/dashboard");
            await WaitForPageToLoad();
            
            // Logga URL
            string currentUrl = _page.Url;
            Console.WriteLine($"Current URL: {currentUrl}");
            
            // Ta skärmdump av dashboarden
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "staff-dashboard.png",
                FullPage = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error navigating to dashboard: {ex.Message}");
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "dashboard-navigation-error.png",
                FullPage = true
            });
            throw;
        }
    }

    [When(@"I open the chat with the pending request by clicking Öppna chatt")]
    public async Task WhenIOpenTheChatWithThePendingRequest()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        try
        {
            Console.WriteLine("Looking for 'Öppna chatt' link/button");
            
            // Ta skärmdump innan vi försöker hitta knappen
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "before-open-chat-click.png",
                FullPage = true
            });
            
            // Använd den kända selektorn du har hittat
            Console.WriteLine("Using the known selector: div.ticket-task-token a");
            
            // Vänta på att elementet ska vara synligt och klickbart
            await _page.WaitForSelectorAsync("div.ticket-task-token a", new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });
            
            // Logga alla div.ticket-task-token a element som finns
            var elements = await _page.QuerySelectorAllAsync("div.ticket-task-token a");
            Console.WriteLine($"Found {elements.Count} elements with selector 'div.ticket-task-token a'");
            
            foreach (var element in elements)
            {
                string? text = await element.TextContentAsync();
                string? href = await element.GetAttributeAsync("href");
                Console.WriteLine($"Element text: '{text?.Trim()}', href: '{href}'");
            }
            
            // Försök klicka på elementet
            await _page.ClickAsync("div.ticket-task-token a");
            Console.WriteLine("Clicked on the 'div.ticket-task-token a' element");
            
            // Vänta på att chattfönstret ska öppnas eller sidan ska laddas
            await WaitForPageToLoad();
            
            // Ta skärmdump efter klick
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "after-open-chat-click.png",
                FullPage = true
            });
            
            // Logga URL efter klick
            string currentUrl = _page.Url;
            Console.WriteLine($"URL after clicking Öppna chatt: {currentUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when opening chat: {ex.Message}");
            
            // Fallback lösning: Försök med alla olika selektorer om det första inte fungerade
            try
            {
                Console.WriteLine("Fallback: Trying alternative selectors...");
                
                // Lista alla klickbara element på sidan för diagnostik
                var allElements = await _page.QuerySelectorAllAsync("a, button");
                Console.WriteLine($"All clickable elements on page ({allElements.Count}):");
                
                foreach (var element in allElements)
                {
                    string? text = await element.TextContentAsync();
                    string? href = await element.GetAttributeAsync("href");
                    string? id = await element.GetAttributeAsync("id");
                    string? className = await element.GetAttributeAsync("class");
                    
                    Console.WriteLine($"Element: Text='{text?.Trim()}', href='{href}', id='{id}', class='{className}'");
                    
                    // Försök identifiera element som kan vara Öppna chatt-knappen
                    if (!string.IsNullOrEmpty(text) && 
                        (text.Contains("Öppna chatt", StringComparison.OrdinalIgnoreCase) || 
                         text.Contains("Öppna", StringComparison.OrdinalIgnoreCase) ||
                         text.Contains("chatt", StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.WriteLine($"Found potential 'Öppna chatt' element: '{text.Trim()}'");
                        
                        try
                        {
                            await element.ClickAsync();
                            Console.WriteLine($"Successfully clicked alternative element with text: '{text.Trim()}'");
                            
                            // Vänta på eventuell omdirigering
                            await WaitForPageToLoad();
                            break;
                        }
                        catch (Exception clickEx)
                        {
                            Console.WriteLine($"Failed to click element: {clickEx.Message}");
                        }
                    }
                }
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Fallback approach also failed: {fallbackEx.Message}");
            }
            
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "open-chat-error.png",
                FullPage = true
            });
            
            // Omskriva selektorn och försök igen med JavaScript click som sista utväg
            try
            {
                Console.WriteLine("Trying JavaScript click as last resort...");
                await _page.EvaluateAsync(@"
                    const elements = Array.from(document.querySelectorAll('div.ticket-task-token a, a:contains(""Öppna""), a:contains(""chatt"")'));
                    for (const el of elements) {
                        console.log('Trying to click:', el.textContent, el);
                        el.click();
                    }
                ");
                
                await WaitForPageToLoad();
                Console.WriteLine("Attempted JavaScript click on potential elements");
            }
            catch (Exception jsEx)
            {
                Console.WriteLine($"JavaScript click attempt failed: {jsEx.Message}");
            }
            
            // Vi kanske vill fortsätta testet trots felet, så vi kommenterar ut throw
            // Om vi vill att testet ska faila här, avkommentera nästa rad
            throw;
        }
    }

    [When(@"I send a message ""(.*)""")]
    public async Task WhenISendAMessage(string message)
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        
        try
        {
            Console.WriteLine($"Attempting to send message: '{message}'");
            
            // Spara meddelandet för senare verifiering
            _messageText = message;
            
            // Ta skärmdump innan vi försöker skicka meddelandet
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "before-send-message.png",
                FullPage = true
            });
            
            // Försök hitta textfält för meddelanden
            var inputFields = await _page.QuerySelectorAllAsync("input[type='text'], textarea");
            bool messageSent = false;
            
            foreach (var field in inputFields)
            {
                string? placeholder = await field.GetAttributeAsync("placeholder");
                Console.WriteLine($"Found input field with placeholder: '{placeholder}'");
                
                if (placeholder != null && 
                    (placeholder.Contains("meddelande", StringComparison.OrdinalIgnoreCase) || 
                     placeholder.Contains("message", StringComparison.OrdinalIgnoreCase) ||
                     placeholder.Contains("chat", StringComparison.OrdinalIgnoreCase) ||
                     placeholder.Contains("chatt", StringComparison.OrdinalIgnoreCase)))
                {
                    await field.FillAsync(message);
                    Console.WriteLine("Filled message in input field with matching placeholder");
                    
                    // Hitta och klicka på skicka-knappen
                    var sendButton = await _page.QuerySelectorAsync("button[type='submit'], button:has-text('Skicka'), button:has-text('Send')");
                    if (sendButton != null)
                    {
                        await sendButton.ClickAsync();
                        Console.WriteLine("Clicked send button");
                        messageSent = true;
                        break;
                    }
                }
            }
            
            // Om vi inte hittade ett fält med matchande placeholder, försök med första textfältet
            if (!messageSent && inputFields.Count > 0)
            {
                await inputFields[0].FillAsync(message);
                Console.WriteLine("Filled message in first available input field");
                
                // Leta efter en skicka-knapp
                var sendButton = await _page.QuerySelectorAsync("button[type='submit'], button:has-text('Skicka'), button:has-text('Send'), button.send-button");
                if (sendButton != null)
                {
                    await sendButton.ClickAsync();
                    Console.WriteLine("Clicked send button");
                    messageSent = true;
                }
                else
                {
                    // Om ingen skicka-knapp hittas, försök med Enter-tangenten
                    await inputFields[0].PressAsync("Enter");
                    Console.WriteLine("Pressed Enter key to send message");
                    messageSent = true;
                }
            }
            
            // Vänta lite för att meddelandet ska behandlas
            await Task.Delay(2000);
            
            // Ta skärmdump efter att meddelandet skickats
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "after-send-message.png",
                FullPage = true
            });
            
            if (!messageSent)
            {
                throw new Exception("Failed to send message - no suitable input field found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "message-send-error.png",
                FullPage = true
            });
            throw;
        }
    }

    [Then(@"the message should appear in the chat window")]
    public async Task ThenTheMessageShouldAppearInTheChatWindow()
    {
        if (_page == null) throw new InvalidOperationException("Page is not initialized");
        if (_messageText == null) throw new InvalidOperationException("Message text is not set");
        
        try
        {
            Console.WriteLine($"Verifying that message '{_messageText}' appears in chat window");
            
            // Vänta lite för att säkerställa att meddelandet visas
            await Task.Delay(2000);
            
            // Ta en slutlig skärmdump
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "final-chat-window.png",
                FullPage = true
            });
            
            // Försök hitta meddelandet i chattfönstret på olika sätt
            var messageElements = await _page.QuerySelectorAllAsync(".message, .chat-message, .message-bubble");
            bool messageFound = false;
            
            foreach (var element in messageElements)
            {
                string? text = await element.TextContentAsync();
                Console.WriteLine($"Found message element with text: '{text}'");
                
                if (text != null && text.Contains(_messageText))
                {
                    Console.WriteLine("Message found in chat window!");
                    messageFound = true;
                    break;
                }
            }
            
            // Om vi inte hittade meddelandet i specifika element, kolla hela chattfönstret
            if (!messageFound)
            {
                Console.WriteLine("Message not found in specific elements, checking entire chat window");
                
                var chatWindow = await _page.QuerySelectorAsync(".chat-window, .chat-container, .messages-container");
                if (chatWindow != null)
                {
                    string? chatText = await chatWindow.TextContentAsync();
                    if (chatText != null && chatText.Contains(_messageText))
                    {
                        Console.WriteLine("Message found in chat window text!");
                        messageFound = true;
                    }
                }
            }
            
            // Som sista utväg, kolla hela sidans text
            if (!messageFound)
            {
                string pageText = await _page.TextContentAsync("body");
                if (pageText.Contains(_messageText))
                {
                    Console.WriteLine("Message found in page text!");
                    messageFound = true;
                }
            }
            
            Assert.True(messageFound, $"Message '{_messageText}' not found in chat window");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying message: {ex.Message}");
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "message-verification-error.png",
                FullPage = true
            });
            throw;
        }
    }
}