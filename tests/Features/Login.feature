@automated @system_requirement_1234 @brite_login
Feature: User Login
  As a registered user
  I want to log in to the application
  So that I can access my account

  @brite_login_success
  Scenario: Successful login with valid credentials
    Given the user is on the login page
    When the user enters valid username "alice" and password "secret"
    And the user clicks the "Sign in" button
    Then the user is redirected to the dashboard
    And a welcome message "Welcome, alice" is displayed

  @brite_login_invalid_password
  Scenario: Failed login with invalid password
    Given the user is on the login page
    When the user enters valid username "alice" and password "wrong"
    And the user clicks the "Sign in" button
    Then an error message "Invalid credentials" is displayed
    And the user remains on the login page

  @brite_login_logout
  Scenario: Successful logout after login
    Given the user is logged in as "alice"
    When the user clicks the "Log out" button
    Then the user is redirected to the login page
    And the session is terminated
