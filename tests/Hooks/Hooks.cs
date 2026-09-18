using Microsoft.Playwright;
using Reqnroll;

namespace specsync_demo.tests.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly ScenarioContext _scenarioContext;
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public async Task RegisterPlaywrightPage()
        {
            // Bypass local proxy blocks for IPC WebSocket traffic on AWS
            Environment.SetEnvironmentVariable("NO_PROXY", "127.0.0.1,localhost");
            Environment.SetEnvironmentVariable("no_proxy", "127.0.0.1,localhost");

            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                // 1. Mandatory for msvsmon / non-interactive AWS sessions
                Headless = true,

                // 2. Prevent infinite deadlocks
                Timeout = 15000,

                // 3. Essential flags for AWS remote execution
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-gpu",
                    "--ignore-certificate-errors"
                }
            });

            // 1. Get today's date formatted as YYYY-MM-DD
            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");

            // Specify a fixed, absolute directory for all test execution videos
            string videoDirectory = Path.Combine(@"C:\Automation\specsync_demo\TestVideos", dateFolder);

            // Ensure the folder exists on the machine before Playwright runs
            Directory.CreateDirectory(videoDirectory);

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                RecordVideoDir = videoDirectory,
                RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 }
            });

            _page = await _context.NewPageAsync();
            _scenarioContext.Set(_page);
        }

        [AfterScenario]
        public async Task TearDown()
        {
            try
            {
                // Closing context flushes the video file to disk
                if (_context != null)
                {
                    await _context.CloseAsync();
                }

                if (_browser != null)
                {
                    await _browser.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during teardown: {ex.Message}");
            }
            finally
            {
                _playwright?.Dispose();
                _playwright = null;
                _browser = null;
                _context = null;
            }
        }
    }
}
