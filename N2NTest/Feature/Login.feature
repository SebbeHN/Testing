Feature: Login as admin at Shoptester

    Scenario: Log in form
        Given I am on Shoptester homepage
        And I see the "Login" button
        When I click on the "Login" button
        And i fill in the login form with valid credentials
        And press the submit button
        Then I should be logged in