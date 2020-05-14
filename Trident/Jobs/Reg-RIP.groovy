@Library('ProjectMayhem@v1') _

properties([
	parameters([
		string(
			name: 'RegTestsConfig', 
            choices: ['Reg-B', 'Reg-Zero', 'Reg-A', 'Reg-Prod'],
			description: '[Required] Set regression environment'
		),
		string(
			name: 'AdminUsername', 
			defaultValue: '', 
			description: '[Required] User used for test'
		),
		password(
			name: 'AdminPassword', 
			defaultValue: '', 
			description: '[Required] Password for User'
		),
		// password(
		// 	name: 'UseExistingTemplate', 
		// 	defaultValue: '', 
		// 	description: 'Set workspace name which would be used as Template Workspace for test cases'
		// ),
		string(
			name: 'TestsFilter', 
			defaultValue: '', 
			description: 'Filter for tests. In general it narrows tests to run'
		),
		choice(
			name: 'UITestsBrowser',
			choices: ['chromium-portable', 'chrome', 'chromium', 'firefox'],
			defaultValue: 'chromium-portable',
			description: 'Name of browser the UI tests should be run on.'
		),
	])
])

jobWithSut {
	sutTemplate = "aio-juniper-1"
    jobScript = "Trident/Scripts/UI-RegressionRIP.ps1"
}