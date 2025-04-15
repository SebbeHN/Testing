Feature: Admin deletes a user
As an admin
I want to delete a user from the system
So that I can manage users effectively

    Scenario: Admin deletes an existing user
        Given I am logged in as an admin
     
        When I delete the user with email "test@test.com"
        Then the user with email "test@test.com" should no longer be visible