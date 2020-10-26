Feature: Expenses
	In order to reclaim my business expenses 
	As a travelling business man
	I want to be able to submit my expenses

Scenario: Submit an expense
	Given a business expense
	When I submit the expense
	And I submit a receipt
	Then the expense should be stored
	And the receipt should be available to download