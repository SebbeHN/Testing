Feature: Staff Dashboard

    Scenario: Staff moves ticket to "My Tasks"
        Given I am logged in as a staff member
        When I navigate to the staff dashboard
        And I drag a ticket from "Ärenden" to "Mina ärenden"
        Then the ticket should appear in the "Mina ärenden" column