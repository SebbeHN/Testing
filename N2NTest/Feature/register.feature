Feature: Register a user at Shoptester

Scenario: Open register form
    Given I am on Shoptester homepage
    And I see the "Register" button
    When I click on the "Register" button
    Then I should see the registration form



