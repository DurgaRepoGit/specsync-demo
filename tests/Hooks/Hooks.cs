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
                //Mandatory for msvsmon / non-interactive AWS sessions
                Headless = true,

                //Prevent infinite deadlocks
                Timeout = 15000,

                //Essential flags for AWS remote execution
                Args = new[]
                {
                    "--no-sandbox", //Disables the OS-level browser sandbox security container
                    "--disable-setuid-sandbox", //afe to include alongside --no-sandbox for cross-platform compatibility.
                    "--disable-gpu", //Disables Hardware Graphics Acceleration (GPU)
                    "--ignore-certificate-errors" //Ignores SSL/TLS certificate warnings
                }
            });

            //Get today's date formatted as YYYY-MM-DD
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
            string originalVideoPath = null;
            try
            {
                // Get the generated video path before closing the page/context
                if (_page != null)
                {
                    var video = _page.Video;
                    if (video != null)
                    {
                        originalVideoPath = await video.PathAsync();
                    }
                }
                // Closing context flushes the video file to disk
                if (_context != null)
                {
                    await _context.CloseAsync();
                }

                if (_browser != null)
                {
                    await _browser.CloseAsync();
                }

                // Rename the video file after context is closed
                if (!string.IsNullOrEmpty(originalVideoPath) && File.Exists(originalVideoPath))
                {
                    string directory = Path.GetDirectoryName(originalVideoPath);

                    // Clean scenario title to ensure it's a valid Windows filename
                    string scenarioName = _scenarioContext.ScenarioInfo.Title;
                    string safeFileName = string.Concat(scenarioName.Split(Path.GetInvalidFileNameChars())) + ".webm";

                    string newVideoPath = Path.Combine(directory, safeFileName);

                    // Overwrite if a video with the same test name already exists
                    if (File.Exists(newVideoPath))
                    {
                        File.Delete(newVideoPath);
                    }

                    File.Move(originalVideoPath, newVideoPath);
                    Console.WriteLine($"Saved video recording to: {newVideoPath}");
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
