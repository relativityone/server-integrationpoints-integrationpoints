@Library('ProjectMayhem@v1') _

properties([
	buildDiscarder(logRotator(artifactDaysToKeepStr: '7', artifactNumToKeepStr: '7', daysToKeepStr: '30', numToKeepStr: '30')),
	parameters([
		string(
			name: 'RegTestsConfig', 
            choices: ['Reg-B', 'Reg-Zero', 'Reg-A', 'Reg-Prod'],
			description: '[Required] Set regression environment'
		)
		// string(
		// 	name: 'AdminUsername', 
		// 	defaultValue: '', 
		// 	description: '[Required] User used for test'
		// ),
		// password(
		// 	name: 'AdminPassword', 
		// 	defaultValue: '', 
		// 	description: '[Required] Password for User'
		// ),
		// password(
		// 	name: 'UseExistingTemplate', 
		// 	defaultValue: '', 
		// 	description: 'Set workspace name which would be used as Template Workspace for test cases'
		// ),
		// string(
		// 	name: 'TestsFilter', 
		// 	defaultValue: '', 
		// 	description: 'Filter for tests. In general it narrows tests to run'
		// ),
		// choice(
		// 	name: 'UITestsBrowser',
		// 	choices: ['chromium-portable', 'chrome', 'chromium', 'firefox'],
		// 	defaultValue: 'chromium-portable',
		// 	description: 'Name of browser the UI tests should be run on.'
		// ),
	])
])

timestamps {
	node("PolandBuild") {
		timeout(time: 6, unit: 'HOURS') {
			try {
				stage('Checkout') {
					def scmVars = checkout([
							$class: 'GitSCM',
							branches: scm.branches,
							extensions: scm.extensions + [[$class: 'CleanBeforeCheckout']],
							userRemoteConfigs: scm.userRemoteConfigs
					])
					commitHash = scmVars.GIT_COMMIT
				}

				stage('Run Job') {
					withCredentials([usernamePassword(credentialsId: 'ProgetCI', passwordVariable: 'nugetSvcPassword', usernameVariable: 'nugetSvcUsername')]) {
						powershell "./Trident/Scripts/Reg-RIP.ps1"
					}
				}

				currentBuild.result = 'SUCCESS'
			} catch (err) {
				currentBuild.result = 'FAILURE'
				error (err.toString())
			} finally {
				utils.publishLogs()
				powershell '''
					if(Test-Path Modules)
					{
						Write-Host "Deleting downloaded modules"
						Remove-Item .\\Modules -Recurse -Force -ErrorAction SilentlyContinue
					}
				'''
			}
		}
	}
}