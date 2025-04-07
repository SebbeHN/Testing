Feature: Admin User Management

    Scenario: Admin creates a new staff user
        Given I am logged in as an admin
        When I navigate to the admin create user page
        And I fill in the user email as "newstaff@example.com"
        And I fill in the user name as "New Staff"
        And I fill in the password as "password123"
        And I select "tele" as the company
        And I select "staff" as the role
        And I click the "Skapa användare" button
        Then I should see a success message