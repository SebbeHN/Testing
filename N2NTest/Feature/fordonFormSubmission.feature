Feature: Vehicle Service Form Submission

    Background:
        Given I am on the dynamic form page

    Scenario: Submit a vehicle repair issue
        When I select "Fordonsservice" as the company type
        And I fill in the customer name as "Car Owner"
        And I fill in the email as "car.owner@example.com"
        And I enter vehicle registration number "ABC123"
        And I select "Problem efter reparation" as the issue type
        And I enter "The problem with my brakes persists after the repair." as the message
        And I submit the form
        Then I should see a success message
        And I should receive a chat link via email