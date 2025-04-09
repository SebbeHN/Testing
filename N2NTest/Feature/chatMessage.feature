Feature: Chat Functionality

    Scenario: Staff responds to customer chat
        Given I am logged in as a staff member
        And there is a pending chat request from a customer
        When I navigate to the staff dashboard
        And I open the chat with the pending request by clicking Öppna chatt
        And I send a message "How can I help you today?"
        Then the message should appear in the chat window