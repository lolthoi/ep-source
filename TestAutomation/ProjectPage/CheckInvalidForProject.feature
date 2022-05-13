Feature: Check Invalid for Project

Scenario: Check cases invalid for Project
	Given I login to App 
	When I create a new project
	And  I check data in the field name
	And  I check data in the field Status
	And  I check data in the field StartDate
	And  I edit data in the field Name
	And  I edit data in the field StartDate
	Then I delete the new project