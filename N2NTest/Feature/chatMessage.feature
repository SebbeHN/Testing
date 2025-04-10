Feature: Chat Functionality
As a staff member
I want to respond to a customer's chat request
So that I can provide support directly through the dashboard

    Scenario: Staff responds to a chat request
        Given I click on a ticket on öppna chatt
        When I write a response in the chat
        And I click on the send button
        Then I should see my response in the chat

