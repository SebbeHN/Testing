Feature: Insurance Form Submission

    Background:
        Given I am on the dynamic form page

    Scenario: Submit an ongoing insurance claim
        When I select "Försäkringsärenden" as the company type
        And I fill in the customer name as "Insurance Claimant"
        And I fill in the email as "claim@example.com"
        And I select "Hemförsäkring" as the insurance type
        And I select "Pågående skadeärende" as the issue type
        And I enter "I want to check the status of my ongoing claim #12345." as the message
        And I submit the form
        Then I should see a success message
        And I should receive a chat link via email