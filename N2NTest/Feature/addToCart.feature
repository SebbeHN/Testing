Feature: Add to cart at Shoptester

    Scenario: When logged in add to cart
        Given I am logged in
        And I am on the product page
        When I click on the "Add to cart" button
        Then I should see the product in my cart