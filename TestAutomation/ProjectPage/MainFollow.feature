Feature: Create Project

Scenario: Create Project successfully
	Given I log in to App successfully
	When  I create new a Project with status is open
	And   I create new a Project with status is pending
	And   I create new a Project with status is closed
	And   I edit a projetc
	And   I delete a project
	And   I search a project
	And   I order columns