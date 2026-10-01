@automated @system_requirement_253764 @capsule_login @brite_capsule_login
Feature: Hospital Setup
	As an administrator of the Configuration Dashboard
	I want to manage unit creation rules
	So that unit names remain unique across the system

	Background:
    Given I navigate to the login page
    When I enter valid credentials
      | Username | Password |
      | bernoulli    | CPC_01 |
    And I click the login button
    Then I should be redirected to the dashboard page
    And I navigate to the Hospital Setup page

	@capsule_addUnit
	Scenario: Attempt to create a unit with a duplicate name
    When I click the Add Unit button
    Then the Add Unit modal should be displayed
    When I enter unit name "Engineering"
    And I click the Save button
    Then the unit "Engineering" should appear in the units list
    When I click the Add Unit button
    Then the Add Unit modal should be displayed
    When I enter unit name "Engineering"
    And I click the Save button
    Then an error message indicating duplicate unit names should be displayed

    @capsule_deleteUnit
    Scenario: Delete unit confirmation, cancellation, and validation with assigned locations
    When I select the check box for unit "Engineering" in the Units table
    And I click the Delete button
    Then the Delete Unit confirmation modal should be displayed
    When I click Cancel on the Delete Unit modal
    Then the Delete Unit modal should close
    And the unit "Engineering" should still exist in the Units table
    When I click the Delete button
    Then the Delete Unit confirmation modal should be displayed
    When I confirm the unit deletion
    Then the unit "Engineering" should be removed from the Units table
    When I select the check box for unit "Unit" in the Units table
    And I click the Delete button
    Then the Delete Unit confirmation modal should be displayed
    When I confirm the unit deletion
    Then an error message stating "Units cannot be removed while rooms are assigned to them." should be displayed
    When I click the header check mark at the top of the Units table
    Then all unit check boxes should be toggled