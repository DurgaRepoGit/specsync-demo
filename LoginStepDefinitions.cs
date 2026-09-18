using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace specsync_demo.tests.StepDefinitions
{
    [Binding]
    public class LoginStepDefinitions
    {
        private readonly IPage _page;
        private static readonly string MachineName = Environment.MachineName.ToLower();
        private static readonly string BaseUrl = $"https://{MachineName}/configurationdashboard";

        // Simple POCO class to hold the credentials data
        public class CredentialModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public LoginStepDefinitions(ScenarioContext scenarioContext)
        {
            _page = scenarioContext.Get<IPage>();
        }

        // Matched: Given I navigate to the login page "/security/login"
        [Given(@"I navigate to the login page$")]
        public async Task GivenINavigateToTheLoginPage()
        {
            //string fullUrl = $"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
            await _page.GotoAsync(BaseUrl);
            await _page.PauseAsync(); // Forces browser window open and pauses execution
        }

        // Matched: When I enter valid credentials (dynamically reads from DataTable)
        [When(@"I enter valid credentials")]
        public async Task WhenIEnterValidCredentials(DataTable dataTable)
        {
            // Reqnroll automatically recognizes 'Username' and 'Password' headers from your feature file
            var credentials = dataTable.CreateInstance<CredentialModel>();

            await _page.FillAsync("#username", credentials.Username);
            await _page.FillAsync("#password", credentials.Password);
        }

        // Matched: And I click the login button
        [When(@"I click the login button")]
        public async Task WhenIClickTheLoginButton()
        {
            await _page.ClickAsync("#btn-login");
        }

        // Matched: Then I should be redirected to the dashboard page
        [Then(@"I should be redirected to the dashboard page")]
        public async Task ThenIShouldBeRedirectedToTheDashboardPage()
        {
            await _page.WaitForURLAsync("**/configurationdashboard");
            Assert.That(_page.Url, Does.Contain("/configurationdashboard"));
        }

        // Matched: And I should see the dashboard welcome message
        [Then(@"I should see the dashboard welcome message")]
        public async Task ThenIShouldSeeTheDashboardWelcomeMessage()
        {
            // Locates the navigation tabs container (<ul id="c" class="nav nav-tabs">)
            var navTabsElement = _page.Locator("#c");

            // Auto-waits until the element is present and visible in the DOM
            await navTabsElement.WaitForAsync();

            // Verifies the user is successfully logged in and the element is visible
            Assert.That(await navTabsElement.IsVisibleAsync(), Is.True);
        }
    }
}