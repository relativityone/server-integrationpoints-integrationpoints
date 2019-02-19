#!groovy

library 'PipelineTools@RMT-9.3.1'
library 'SCVMMHelpers@3.2.0'
library 'GitHelpers@1.0.0'
library 'SlackHelpers@3.0.0'

import groovy.transform.Field

properties([
	[$class: 'BuildDiscarderProperty', strategy: [
			$class: 'LogRotator', 
			artifactDaysToKeepStr: '30',
			artifactNumToKeepStr: '', 
			daysToKeepStr: '30', 
			numToKeepStr: ''
		]
	],
	parameters([
		string(
			name: 'relativityBranch', 
			defaultValue: '', 
			description: 'Set Relativity branch'
		),
		string(
			name: 'relativityBuildType', 
			defaultValue: 'DEV', 
			description: 'Set Relativity build type (DEV, GOLD, etc.)'
		),
		string(
			name: 'relativityBuildVersion', 
			defaultValue: '', 
			description: 'Set Relativity version on which RIP will be tested. Leave blank to set the latest version.'
		),
		booleanParam(
			name: 'skipIntegrationTests', 
			defaultValue: false, 
			description: 'Check if you want to skip Integrations Tests stage.'
		),
		booleanParam(
			name: 'skipUITests', 
			defaultValue: true, 
			description: 'Check if you want to skip UI Tests stage.'
		),
		string(
			name: 'testsFilter', 
			defaultValue: 'cat == SmokeTest', 
			description: 'Set filter for integration and UI tests'
		),
		string(
			name: 'nightlyTestsFilter', 
			defaultValue: '', 
			description: 'Set filter for nightly integration and UI tests'
		),
		booleanParam(
			name: 'enableSonarAnalysis', 
			defaultValue: true, 
			description: 'Enable SonarQube analysis for develop branch.'
		)
	])
])

def jenkinsHelpers = null

def numberOfFailedTests = -1
def numberOfPassedTests = -1
def numberOfSkippedTests = -1

def agentsPool = "SCVMM-AGENTS-POOL"

timestamps
{
	try
	{
		node ('PolandBuild')
		{
			stage ('Checkout')
			{
				timeout(time: 10, unit: 'MINUTES')
				{
					checkout scm
					step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
				}
				jenkinsHelpers = load "DevelopmentScripts/JenkinsHelpers.groovy"
                jenkinsHelpers.initializeRIPPipeline(env, params)
			}
			stage ('Get Version')
			{
                jenkinsHelpers.getVersion()
			}
			stage ('Build')
			{
                jenkinsHelpers.build()
			}
			stage ('Unit Tests')
			{
				jenkinsHelpers.unitTest()
			}
			stage ('Package')
			{
                jenkinsHelpers.packageRIP()
			}

			stage ('Stash Tests Artifacts')
			{
                jenkinsHelpers.stashTestsArtifacts()
			}

			if (jenkinsHelpers.testingVMsAreRequired(params))
			{
				// Provision SUT
				stage('Install RAID')
				{
                    jenkinsHelpers.raid(this)
				}

				// Run tests on provisioned SUT
				node ("$session_id && dependencies")
				{
					stage ('Unstash Tests Artifacts')
					{
						timeout(time: 3, unit: 'MINUTES')
						{
							unstash 'testdlls'
							unstash 'dynamicallyLoadedDLLs'
							unstash 'integrationPointsRap'
							unstash 'nunitProjectFiles'
							unstash 'nunitConsoleRunner'
							unstash 'nunitProjectLoader'
							unstash 'buildps1'
							unstash 'buildScripts'
							unstash 'psake'
							unstash 'nuget'
							unstash 'version'
						}
					}
					try
					{
						stage ('Integration Tests')
						{
							timeout(time: 180, unit: 'MINUTES')
							{
								jenkinsHelpers.runIntegrationTests(params)
							}
						}
						if (jenkinsHelpers.isNightly())
						{
							stage ('Integration Tests in Quarantine')
							{
								timeout(time: 180, unit: 'MINUTES')
								{
									jenkinsHelpers.runIntegrationTestsInQuarantine(params)
								}
							}
						}
						stage ('UI Tests')
						{
							jenkinsHelpers.updateChromeToLatestVersion()
							timeout(time: 8, unit: 'HOURS')
							{
								jenkinsHelpers.runUiTests(params)
							}
						}
					}
					catch(err)
					{
						echo err.toString()
						currentBuild.result = "FAILED"
					}
					finally
					{
						stage ('Gathering test stats')
						{
							timeout(time: 5, unit: 'MINUTES')
							{
								if (!params.skipUITests)
								{
									archiveArtifacts artifacts: "lib/UnitTests/app.jeeves-ci.config", fingerprint: true
									archiveArtifacts artifacts: "lib/UnitTests/*.png", fingerprint: true, allowEmptyArchive: true
								}

                                powershell "Import-Module ./Vendor/psake/tools/psake.psm1; Invoke-psake ./DevelopmentScripts/psake-test.ps1 generate_nunit_reports" 
                                def artifactsPath = jenkinsHelpers.getConstants().ARTIFACTS_PATH
                                archiveArtifacts artifacts: "$artifactsPath/**/*", fingerprint: true, allowEmptyArchive: true

								if (!params.skipIntegrationTests)
								{
                                    numberOfFailedTests = jenkinsHelpers.getTestsStatistic('failed')
                                    numberOfPassedTests = jenkinsHelpers.getTestsStatistic('passed')
                                    numberOfSkippedTests = jenkinsHelpers.getTestsStatistic('skipped')
                                }
							}
						}
					}
				}
			}

			stage ('Publish to NuGet')
			{
				withCredentials([string(credentialsId: 'ProgetNugetApiKey', variable: 'key')])
				{
					retry(3)
					{
						powershell "./build.ps1 -sk -nuget $key $commonBuildArgs"
					}
				}
			}

			stage ('Publish to bld-pkgs')
			{
				def credentials = [
					usernamePassword(
						credentialsId: 'jenkins_packages_svc', 
						passwordVariable: 'BLDPKGSPASSWORD', 
						usernameVariable: 'BLDPKGSUSERNAME'
					)
				]
				withCredentials(credentials)
				{
					jenkinsHelpers.publishToBldPkgs(
						BLDPKGSUSERNAME, 
						BLDPKGSPASSWORD, 
						'./BuildPackages', 
						jenkinsHelpers.getConstants().PACKAGE_NAME, 
						env.BRANCH_NAME, 
						version
					)
				}
			}

			currentBuild.result = 'SUCCESS'
		}
	}
	catch (err)
	{
		echo err.toString()
		currentBuild.result = "FAILED"
	}
	finally
	{
		stage('Cleanup VMs')
		{
			try
			{
				timeout(time: 20, unit: 'MINUTES')
				{
					if(sut?.name)
					{
						// If we don't have a result, we didn't get to a test because somthing failed out earlier.
						// If the result is FAILURE, a test failed.
						if (!currentBuild.result || currentBuild.result == "FAILURE")
						{
							try
							{
								timeout(time: 5, unit: 'MINUTES')
								{
									//it returns username who submitted the request to save vms
									user = input(
										message: 'Save the VMs?', 
										ok: 'Save', 
										submitter: 'JNK-Basic', 
										submitterParameter: 'submitter'
									)
								}
								ScvmmInstance.saveVMs(user)
							}
							// Exception is thrown if you click abort or let it time out
							catch(err)
							{
								echo "Deleting VMs..."
								ScvmmInstance.deleteVMs()
							}
						}
					}
					deleteNodes(this, session_id)
				}
			}
			catch (err)
			{
				echo "Cleanup VMs FAILED."
			}
		}

		stage('Reporting')
		{
			try
			{
				echo "Build result: $currentBuild.result"
				node(agentsPool)
				{
					timeout(time: 3, unit: 'MINUTES')
					{
						step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
						withCredentials([string(credentialsId: 'SlackJenkinsIntegrationToken', variable: 'token')])
						{
							message = "*${currentBuild.result.toString()}* ${((currentBuild.result.toString() == "FAILURE") ? ":alert:" : "" )} \n\n" +
								"Build *#${env.BUILD_NUMBER}* from *${env.BRANCH_NAME}*.\n" +
								":greencheck: Passed tests: ${numberOfPassedTests}\n" +
								":negative_squared_cross_mark: Failed tests: ${numberOfFailedTests}\n" +
								":yellow_card: Skipped tests: ${numberOfSkippedTests} \n\n" +
								"${env.BUILD_URL} \n" +
								"Relativity branch: ${relativityBranch} \n" +
								"Relativity build type: ${relativityBuildType} \n" +
								"Relativity build version: ${(relativityBuildVersion ?: "0.0.0.0")}"
							slackSend channel: jenkinsHelpers.getSlackChannelName().toString(), color: "E8E8E8", message: "${message}", teamDomain: 'kcura-pd', token: token
						}
					}
				}
			}
			catch (err)  // Just catch everything here, if reporting/cleanup is the only thing that failed, let's not fail out the pipeline.
			{
				echo "Reporting failed: $err"
			}
		}
	}
}
