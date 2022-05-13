 Feature: gridpersonal project 

Scenario: gridpersonal successfully
	Given I am on the login employee pages
	When I open Evaluation Personal
	And I check selfevaluation button
	And I create invalid
	And I check from-to
	And I check project field
	Then I check sort function
	