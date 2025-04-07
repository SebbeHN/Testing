Feature: CRM Form Submission

Background: Given i am on the dynamic form page

    Scenario: Submit a telecom support request
        Given I am on the dynamic form page
        When I select "Tele/Bredband" as the company type
        And I fill in the customer name as "Test User"
        And I fill in the email as "test@example.com"
        And I select "Bredband" as the service type
        And I select "Tekniskt problem" as the issue type
        And I enter "Test message" as the message
        And I submit the form
        Then I should see a success message