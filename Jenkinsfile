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

@Field
def sut = null

def jenkinsHelpers = null

def version = null
def commonBuildArgs = null

def relativityBuildVersion = ""
def relativityBuildType = ""
def relativityBranch = params.relativityBranch ?: env.BRANCH_NAME

// *********
// IMPORTANT
// *********
// Set variable below to the branch name, when you create new release branch!!!
// This should be changed on the release branch
def relativityBranchFallback = "develop"

def chef_attributes = 'fluidOn:1,cdonprem:1'
def ripCookbooks = getCookbooks()

def numberOfFailedTests = -1
def numberOfPassedTests = -1
def numberOfSkippedTests = -1

def installing_relativity = true
def installing_invariant = false
def installing_analytics = false
def installing_datagrid = false

def agentsPool = "SCVMM-AGENTS-POOL"

// Do not modify.
def run_list = createRunList(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)
def profile = createProfile(installing_relativity, installing_invariant, installing_analytics, installing_datagrid)
def knife = 'C:\\Python27\\Lib\\site-packages\\jeeves\\knife.rb'
def session_id = System.currentTimeMillis().toString()
def event_hash = java.security.MessageDigest.getInstance("MD5").digest(env.JOB_NAME.bytes).encodeHex().toString()
def ScvmmInstance = scvmm(this, session_id)
ScvmmInstance.setHoursToLive("12")

// Make changes here if necessary.
def python_packages = 'jeeves==4.1.0 phonograph==5.2.0 selenium==3.0.1'

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
			}
			stage ('Get Version')
			{
				version = jenkinsHelpers.incrementBuildVersion(jenkinsHelpers.Constants.PACKAGE_NAME, params.relativityBuildType)

				currentBuild.displayName="${params.relativityBuildType}-$version"
				commonBuildArgs = "release $params.relativityBuildType -ci -v $version -b $env.BRANCH_NAME"
			}
			stage ('Build')
			{
				def sonarParameter = jenkinsHelpers.shouldRunSonar(params.enableSonarAnalysis, env.BRANCH_NAME)
				powershell "./build.ps1 $sonarParameter $commonBuildArgs"
				archiveArtifacts artifacts: "DevelopmentScripts/*.html", fingerprint: true
			}
			stage ('Unit Tests')
			{
				timeout(time: 3, unit: 'MINUTES')
				{
					powershell "./build.ps1 -sk -t $commonBuildArgs"
					archiveArtifacts artifacts: "TestLogs/*", fingerprint: true
					currentBuild.result = 'SUCCESS'
				}
			}
			stage ('Package')
			{
				powershell "./build.ps1 -sk -package -root ./BuildPackages $commonBuildArgs"
			}

			stage ('Stash Tests Artifacts')
			{
				timeout(time: 3, unit: 'MINUTES')
				{
					stash includes: 'lib/UnitTests/**', name: 'testdlls'
					stash includes: 'DynamicallyLoadedDLLs/Search-Standard/*', name: 'dynamicallyLoadedDLLs'
					stash includes: 'Applications/RelativityIntegrationPoints.Auto.rap', name: 'integrationPointsRap'
					stash includes: 'DevelopmentScripts/IntegrationPointsTests.*', name: 'nunitProjectFiles'
					stash includes: 'DevelopmentScripts/NUnit.ConsoleRunner/tools/*', name: 'nunitConsoleRunner'
					stash includes: 'DevelopmentScripts/NUnit.Extension.NUnitProjectLoader/tools/*', name: 'nunitProjectLoader'
					stash includes: 'DevelopmentScripts/*.ps1', name: 'buildScripts'
					stash includes: 'build.ps1', name: 'buildps1'
					stash includes: 'Vendor/psake/tools/*', name: 'psake'
					stash includes: 'Vendor/NuGet/NuGet.exe', name: 'nuget'
					stash includes: 'Version/version.txt', name: 'version'
				}
			}

			if (jenkinsHelpers.testingVMsAreRequired(params))
			{
				// Provision SUT
				stage('Install RAID')
				{
					timeout(time: 90, unit: 'MINUTES')
					{
						echo "Getting server from pool, session_id: $session_id, Relativity build type: $params.relativityBuildType, event hash: $event_hash"

						sut = ScvmmInstance.getServerFromPool()

						echo "Acquired server: ${sut.name} @ ${sut.domain} (${sut.ip})"

						parallel (
							Deploy:
							{
								if (installing_relativity)
								{
									(relativityBuildVersion, relativityBranch, relativityBuildType) = jenkinsHelpers.getNewBranchAndVersion(
										relativityBranchFallback, 
										relativityBranch, 
										params.relativityBuildVersion, 
										params.relativityBuildType, 
										session_id
									)
									echo "Installing Relativity, branch: $relativityBranch, version: $relativityBuildVersion, type: $relativityBuildType"
								}

								uploadEnvironmentFile(
									this, 
									sut.name, 
									relativityBuildVersion, 
									relativityBranch, 
									relativityBuildType,
									"", //invariant version
									"", //invariant branch
									ripCookbooks, 
									chef_attributes, 
									knife,
									"", //analytics version
									"", //analytics branch
									session_id, 
									installing_relativity, 
									installing_invariant, 
									installing_analytics
								)

								addRunlist(
									this, 
									session_id, 
									sut.name, 
									sut.domain, 
									sut.ip, 
									run_list, 
									knife, 
									profile, 
									event_hash, 
									"", 
									""
								)

								checkWorkspaceUpgrade(this, sut.name, session_id)
							},
							ProvisionNodes:
							{
								def numberOfSlaves = 1
								def numberOfExecutors = '1'
								ScvmmInstance.createNodes(numberOfSlaves, 60, numberOfExecutors)
								bootstrapDependencies(
									this, 
									python_packages, 
									relativityBranch, 
									relativityBuildVersion, 
									relativityBuildType, 
									session_id
								)
							}
						)
					}
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
                                archiveArtifacts artifacts: "$jenkinsHelpers.Constants.ARTIFACTS_PATH/**/*", fingerprint: true, allowEmptyArchive: true

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
						jenkinsHelpers.Constants.PACKAGE_NAME, 
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
