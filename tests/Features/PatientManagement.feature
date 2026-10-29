@automated @patient_management
Feature: Patient Management via Database
  As an automation engineer
  I want to set up and tear down patient data via database helpers
  So that UI and API tests have consistent prerequisites

  @admit_patient_scenario
  Scenario: Admit a test patient into a specific room
    Given a patient with MRN "998857" is admitted to room "Bed 02" as "Patient" "Test"

  @discharge_patient_scenario
  Scenario: Clean up patient state by discharging via MRN
    Given the patient with MRN "998857" is discharged from the system