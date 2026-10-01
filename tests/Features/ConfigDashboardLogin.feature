@automated @system_requirement_253764 @capsule_login @brite_capsule_login
Feature: User Authentication
	As a register user
	I want to log in to the application
	So that i can access my dashboard

	@TestCaseId_485642
	@capsule_login_success
	Scenario: Successful login with valid credentials
		Given I navigate to the login page
		When I enter valid credentials
            | Username  | Password    |
            | bernoulli  | CPC_01  |
		And I click the login button
        Then I should be redirected to the dashboard page
        And I should see the dashboard welcome message

	@capsule_login_success_API
	Scenario: Successfully retrieve ASP.NET Core auth cookie with valid credentials
        Given I set up the API request context
        When I send a POST request to "/security/login" with urlencoded form data
            | Username  | Password |
            | bernoulli | CPC_01   |
        Then the response status code should be 200 OK
        And the response should drop the ".AspNetCore.Cookies" authentication cookie