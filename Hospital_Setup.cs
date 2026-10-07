using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace specsync_demo.tests.StepDefinitions
{
    [Binding]
    public class Hospital_Setup
    {
        private readonly IPage _page;
        private static readonly string MachineName = Environment.MachineName.ToLower();
        private static readonly string BaseUrl = $"https://{MachineName}/configurationdashboard";

        public class CredentialModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        public Hospital_Setup(ScenarioContext scenarioContext)
        {
            _page = scenarioContext.Get<IPage>();
        }

        //StepDefintion is a flexible attribute in Reqnroll that matches Given, When, or Then steps
        [StepDefinition(@"I navigate to the Hospital Setup page")]
        public async Task GivenINavigateToTheHospitalSetupPage()
        {
            await _page.ClickAsync("#SubBtn_0");
        }
        [When(@"I click the Add Unit button")]
        public async Task WhenIClickTheAddUnitButton()
        {
            await _page.ClickAsync("xpath=//*[@id='AddControls']/button");
        }
        [Then(@"the Add Unit modal should be displayed")]
        public async Task ThenTheAddUnitModalShouldBeDisplayed()
        {
            var modal = _page.Locator("xpath=//*[@class='modal-content w-full']");
            await modal.WaitForAsync();
            Assert.That(await modal.IsVisibleAsync(), Is.True, "The Add Unit modal is not visible.");
        }
        [When("I enter unit name {string}")]
        public async Task WhenIEnterUnitName(string unitName)
        {
            await _page.FillAsync("#UnitName", unitName);
        }
        [When(@"I click the Save button")]
        public async Task WhenIClickTheSaveButton()
        {
            await _page.ClickAsync("#btnSASubmit");
        }

        [Then(@"the unit ""(.*)"" should appear in the units list")]
        public async Task ThenTheUnitShouldAppearInTheUnitsList(string unitName)
        {
            var unitListItem = _page.Locator($".unit-name:has-text('{unitName}')");
            await unitListItem.WaitForAsync();
            Assert.That(await unitListItem.IsVisibleAsync(), Is.True, $"Unit '{unitName}' was not found in the list.");
        }

        [Then("an error message indicating duplicate unit names should be displayed")]
        public async Task ThenAnErrorMessageIndicatingDuplicateUnitNamesShouldBeDisplayed()
        {
            var errorMessage = _page.Locator("#ErrorTextModal");
            await errorMessage.WaitForAsync();

            string errorText = await errorMessage.TextContentAsync() ?? string.Empty;
            Assert.That(await errorMessage.IsVisibleAsync(), Is.True, "Error message container was not displayed.");
            Assert.That(errorText, Does.Contain("A unit with that name already exists"));
        }
        [When(@"I select the check box for unit ""(.*)"" in the Units table")]
        public async Task WhenISelectTheCheckBoxForUnitInTheUnitsTable(string unitName)
        {
            var checkbox = _page.Locator($"xpath=//tr[.//span[contains(@class, 'unit-name') and normalize-space()='{unitName}']]//span[@class='RoledlsCB']");
            await checkbox.CheckAsync();
        }
        [When(@"I click the Delete button")]
        public async Task WhenIClickTheDeleteButton()
        {
            await _page.ClickAsync("#RemoveUnitsButton");
        }

        [Then(@"the Delete Unit confirmation modal should be displayed")]
        public async Task ThenTheDeleteUnitConfirmationModalShouldBeDisplayed()
        {
            var modal = _page.Locator("#confirmModal");
            await modal.WaitForAsync();
            Assert.That(await modal.IsVisibleAsync(), Is.True, "Delete Unit confirmation modal was not displayed.");
        }

        [When(@"I click Cancel on the Delete Unit modal")]
        public async Task WhenIClickCancelOnTheDeleteUnitModal()
        {
            await _page.ClickAsync($"xpath=//*[@class='btn btn-default']");
        }

        [Then(@"the Delete Unit modal should close")]
        public async Task ThenTheDeleteUnitModalShouldClose()
        {
            var modal = _page.Locator("#confirmModal");
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            Assert.That(await modal.IsHiddenAsync(), Is.True, "Delete Unit modal did not close.");
        }

        [Then(@"the unit ""(.*)"" should still exist in the Units table")]
        public async Task ThenTheUnitShouldStillExistInTheUnitsTable(string unitName)
        {
            var unitRow = _page.Locator($".unit-name:has-text('{unitName}')");
            Assert.That(await unitRow.IsVisibleAsync(), Is.True, $"Unit '{unitName}' was deleted when it should have been kept.");
        }

        [When(@"I confirm the unit deletion")]
        public async Task WhenIConfirmTheUnitDeletion()
        {
            await _page.ClickAsync($"xpath=//*[@class='btn btn-primary' and text()='Yes']");
        }

        [Then(@"the unit ""(.*)"" should be removed from the Units table")]
        public async Task ThenTheUnitShouldBeRemovedFromTheUnitsTable(string unitName)
        {
            var unitRow = _page.Locator($".unit-name:has-text('{unitName}')");
            await unitRow.WaitForAsync(new() { State = WaitForSelectorState.Detached });
            Assert.That(await unitRow.CountAsync(), Is.EqualTo(0), $"Unit '{unitName}' was not removed from the table.");
        }

        [Then(@"an error message stating ""(.*)"" should be displayed")]
        public async Task ThenAnErrorMessageStatingShouldBeDisplayed(string expectedMessage)
        {
            var errorMessage = _page.Locator("#UnitError");
            await errorMessage.WaitForAsync();
            string actualText = await errorMessage.TextContentAsync() ?? string.Empty;
            Assert.That(actualText, Does.Contain(expectedMessage));
        }

        [When(@"I click the header check mark at the top of the Units table")]
        public async Task WhenIClickTheHeaderCheckMarkAtTheTopOfTheUnitsTable()
        {
            await _page.ClickAsync("xpath=//td[.//input[@onclick='checkAll()']]//label[@class='RoledlsCBRoot']");
        }

        [Then(@"all unit check boxes should be toggled")]
        public async Task ThenAllUnitCheckBoxesShouldBeToggled()
        {
            var checkboxes = await _page.Locator("tr.unit-row input.sa-checkbox").AllAsync();
            Assert.That(checkboxes, Is.Not.Empty, "No checkboxes were found in the Units table.");

            foreach (var cb in checkboxes)
            {
                bool isChecked = await cb.IsCheckedAsync();
                Assert.That(isChecked, Is.True, "A unit checkbox was not checked after toggling the header check mark.");
            }
        }
    }
}
