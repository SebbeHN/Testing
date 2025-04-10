Feature: Staff Dashboard

    Scenario: Staff user moves a ticket through the workflow
        Given I am logged in as staff
        And I navigate to the staff dashboard
        And I see a ticket in the "Ärenden" column
        When I drag the ticket to the "Mina ärenden" column
        And I drag the same ticket to the "Klara" column
        Then the ticket should appear in the "Klara" column
        And the ticket should not appear in the "Ärenden" or "Mina ärenden" columns