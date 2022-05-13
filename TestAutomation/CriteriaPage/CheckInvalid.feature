Feature: verify Criteria type

Scenario: verify Criteria type successfully
	Given I login to App
	When  I add a new type
	Then  I check creating invalid data
	And   I check creating required data
	And   I check editing invalid name 
	And   I check editing required name
	And   I check editing invalid description 
	Then   I check editing required description

	