using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using System.Text.Json;

namespace specsync_demo.tests.StepDefinitions
{
    [Binding]
    public class LoginStepDefinitions
    {
        private IPlaywright _playwright;
        private IAPIRequestContext _apiRequestContext;
        private IAPIResponse _response;
        public string AuthCookieValue { get; private set; }
        private readonly IPage _page;
        private static readonly string MachineName = Environment.MachineName.ToLower();
        private readonly string _baseUrl = $"https://{MachineName}/configurationdashboard";

        public class CredentialModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public LoginStepDefinitions(ScenarioContext scenarioContext)
        {
            _page = scenarioContext.Get<IPage>();
        }

        [Given(@"I set up the API request context")]
        public async Task GivenISetUpTheAPIRequestContext()
        {
            _playwright = await Playwright.CreateAsync();

            _apiRequestContext = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
            {
                // Ensure _baseUrl ends with '/' so Playwright appends endpoints to /configurationdashboard/
                BaseURL = _baseUrl.EndsWith("/") ? _baseUrl : $"{_baseUrl}/",
                Timeout = 30000,
                IgnoreHTTPSErrors = true
            });
        }

        [When(@"I send a POST request to ""(.*)"" with urlencoded form data")]
        public async Task WhenISendAPOSTRequestToWithUrlencodedFormData(string endpoint, Table table)
        {
            var row = table.Rows[0];

            // Converts "/security/login" to "security/login"
            string relativeEndpoint = endpoint.TrimStart('/');

            var formData = _apiRequestContext.CreateFormData();
            formData.Append("username", row["Username"]);
            formData.Append("password", row["Password"]);

            // Playwright resolves "https://machinename/configurationdashboard/" + "security/login"
            _response = await _apiRequestContext.PostAsync(relativeEndpoint, new()
            {
                Form = formData
            });
        }

        [Then(@"the response status code should be 200 OK")]
        public async Task ThenTheResponseStatusCodeShouldBe200OK()
        {
            int statusCode = _response.Status;

            if (statusCode != 200)
            {
                string responseText = await _response.TextAsync();
                Assert.Fail($"Login failed with status code: {statusCode}. Response Body: {responseText}");
            }

            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Then(@"the response should drop the ""(.*)"" authentication cookie")]
        public async Task ThenTheResponseShouldDropTheAuthenticationCookie(string cookieName)
        {
            // StorageStateAsync() returns a JSON string
            var storageStateJson = await _apiRequestContext.StorageStateAsync();
            using var jsonDocument = JsonDocument.Parse(storageStateJson);
            var root = jsonDocument.RootElement;

            string foundCookieValue = null;

            if (root.TryGetProperty("cookies", out var cookiesElement) && cookiesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var cookie in cookiesElement.EnumerateArray())
                {
                    if (cookie.TryGetProperty("name", out var nameProp) &&
                        nameProp.GetString().Equals(cookieName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundCookieValue = cookie.GetProperty("value").GetString();
                        break;
                    }
                }
            }

            Assert.That(foundCookieValue, Is.Not.Null,
                $"The server authenticated successfully but failed to drop the '{cookieName}' header.");

            AuthCookieValue = foundCookieValue;
            Assert.That(AuthCookieValue, Is.Not.Empty, $"The '{cookieName}' value was empty.");
        }

        [Given(@"I navigate to the login page$")]
        public async Task GivenINavigateToTheLoginPage()
        {
            await _page.GotoAsync(_baseUrl);
          //  await _page.PauseAsync(); // Forces browser window open and pauses execution
        }

        [When(@"I enter valid credentials")]
        public async Task WhenIEnterValidCredentials(DataTable dataTable)
        {
            // Reqnroll automatically recognizes 'Username' and 'Password' headers from feature file
            var credentials = dataTable.CreateInstance<CredentialModel>();

            await _page.FillAsync("#username", credentials.Username);
            await _page.FillAsync("#password", credentials.Password);
        }

        [When(@"I click the login button")]
        public async Task WhenIClickTheLoginButton()
        {
            await _page.ClickAsync("#btn-login");
        }

        [Then(@"I should be redirected to the dashboard page")]
        public async Task ThenIShouldBeRedirectedToTheDashboardPage()
        {
            await _page.WaitForURLAsync("**/configurationdashboard");
            Assert.That(_page.Url, Does.Contain("/configurationdashboard"));
        }

        [Then(@"I should see the dashboard welcome message")]
        public async Task ThenIShouldSeeTheDashboardWelcomeMessage()
        {
            var navTabsElement = _page.Locator("#c");

            await navTabsElement.WaitForAsync();

            Assert.That(await navTabsElement.IsVisibleAsync(), Is.True);
        }
        [AfterScenario("@api")]
        public async Task TearDown()
        {
            if (_apiRequestContext != null)
            {
                await _apiRequestContext.DisposeAsync();
            }

            _playwright?.Dispose();
        }
    }
}