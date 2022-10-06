#!groovy

@Library('ProjectMayhem@v1') _

properties([
	buildDiscarder(logRotator(artifactDaysToKeepStr: '7', artifactNumToKeepStr: '7', daysToKeepStr: '30', numToKeepStr: '30')),
	parameters([
		choice(
			name: 'RegTestsConfig', 
            choices: ['Reg-B', 'Reg-Zero', 'Regression-A', 'Reg-Prod', 'Reg-Prod-Update'],
			description: 'Set regression environment config'
		)
	])
])

timestamps {
	node("jobWithSutNode") {
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
						powershell "./Trident/Scripts/RegressionTests.ps1 -RegEnv $params.RegTestsConfig"
					}
				}

				currentBuild.result = 'SUCCESS'
			} catch (err) {
				currentBuild.result = 'FAILURE'
				error (err.toString())
			} finally {
				utils.publishLogs()
			}
		}
	}
}