Feature: Check Evaluation Management screen

Scenario: Check Evaluation Management screen
	Given I login to App with acc as admin
	When  I check data in Evaluation Management screen
	And   I login to App with acc as project leader
	Then  I login to App with acc as member
	