Feature: Check Evaluation Management screen

Scenario: Check Evaluation Management screen
	Given I login to App with acc as admin
	When  I check order in grid view
	And   I check search in grid view
	And   I check dropdown Project
	And   I check point evaluation
	And   I login to App with acc as project leader
	And   I check dropdown Project of leader
	And   I check search in grid view of leader
	And   I check point evaluation of leader