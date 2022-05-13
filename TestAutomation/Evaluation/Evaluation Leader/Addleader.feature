 Feature: Personal project 

Scenario: Personal successfully
	Given I am on the login employee pages
	When I open evaluation Personal screen
	And I evaluation leader successfully
	And I evaluation leader unsuccessfully
	Then I check detail evaluate1 