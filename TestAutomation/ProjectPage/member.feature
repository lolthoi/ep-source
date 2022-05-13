 Feature: member project 

Scenario: member project successfully
	Given I am on the login employee pages
	When I create project
	And I select member
	And  I  test  search invalid
	And I edit role
	Then I test search member	