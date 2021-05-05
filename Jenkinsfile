#!groovy

library 'PipelineTools@11.0.3'
library 'SCVMMHelpers@7.1.2'
library 'GitHelpers@1.0.0'
library 'SlackHelpers@3.0.0'
library 'TestTrackerHelpers@2.0.0'

properties([
	pipelineTriggers(env.JOB_NAME.contains("IntegrationPointsNightly") ? [cron('H 20 * * *')] : []),
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
			defaultValue: '',
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
		),
		choice(
			name: 'UITestsBrowser',
			choices: ['chrome', 'chromium', 'firefox'],
			description: 'Name of browser the UI tests should be run on. Default: chrome'
		),
		string(
			name: 'chromiumVersion', 
			defaultValue: '72.0.3626.0', 
			description: 'Set chromium version to be installed for UI tests.'
		),
		booleanParam(
			name: 'enableCheckConfigureAwait',
			defaultValue: true,
			description: 'Enable checking if configureAwait is present everywhere it needs to be.'
		),
		booleanParam(
            name: 'importBuiltRAP', 
            defaultValue: true, 
            description: 'Check if you want to deploy built Integration Points RAP'
        )
	])
])

// *********
// IMPORTANT
// *********
// Set variable below to the branch name, when you create new release branch!!!
// This should be changed on the release branch
def relativityBranchFallback = "release-11.1-juniper"

def jenkinsHelpers = null

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
				jenkinsHelpers.initializeRIPPipeline(this, env, params, relativityBranchFallback)
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

			if (jenkinsHelpers.testingVMsAreRequired(params))
			{
				stage ('Stash Tests and Package Artifacts')
				{
					jenkinsHelpers.stashTestsAndPackageArtifacts()
				}
			}
			else
			{
				stage ('Publish to NuGet')
				{
					jenkinsHelpers.publishToNuget()
				}

				stage ('Publish to bld-pkgs')
				{
					jenkinsHelpers.publishToBldPkgs()
				}
			}
			stage ('Cleanup Source directory')
			{
				jenkinsHelpers.deleteDirectoryIfExists('Source')
			}
		}

		if (jenkinsHelpers.testingVMsAreRequired(params))
		{
			node('SCVMM-AGENTS-POOL')
			{
				// Provision SUT
				stage('Install RAID')
				{
					jenkinsHelpers.raid()
				}
			}

			def sessionId = jenkinsHelpers.getSessionId()
			node ("$sessionId && dependencies")
			{
				stage ('Unstash Tests Artifacts')
				{
					jenkinsHelpers.unstashTestsArtifacts()
				}

				try
				{
					stage ('Integration Tests')
					{
						withEnv([
							"JenkinsUseIPRapFile=$params.importBuiltRAP"
						])
						{
							jenkinsHelpers.runIntegrationTests()
						}
					}
					if (jenkinsHelpers.isNightly())
					{
						stage ('Integration Tests in Quarantine')
						{
							withEnv([
								"JenkinsUseIPRapFile=$params.importBuiltRAP"
							])
							{
								jenkinsHelpers.runIntegrationTestsInQuarantine()
							}
						}
					}
					stage ('UI Tests')
					{
						withEnv([
							"UITestsBrowser=$params.UITestsBrowser",
							"JenkinsUseIPRapFile=$params.importBuiltRAP"
						]) 
						{
							echo "Browser used for running UI Tests: $params.UITestsBrowser"
							echo "Value of JenkinsUseIPRapFile: $params.importBuiltRAP"
							
							jenkinsHelpers.downloadAndSetUpBrowser()
							jenkinsHelpers.runUiTests()
						}
					}
				}
				finally
				{
					stage ('Gathering test stats')
					{
						jenkinsHelpers.publishBuildArtifacts()
						jenkinsHelpers.gatherTestStats()
						jenkinsHelpers.importTestResultsToTestTracker()
					}
				}
			}

			node ('PolandBuild')
			{
				def publishArtifactsDirectory = 'publishArtifactsWorkspace'

				jenkinsHelpers.deleteDirectoryIfExists(publishArtifactsDirectory)
				dir(publishArtifactsDirectory)
				{
					stage ('Unstash Package artifacts')
					{
						jenkinsHelpers.unstashPackageArtifacts()
					}
					stage ('Publish to NuGet')
					{
						jenkinsHelpers.publishToNuget()
					}
					stage ('Publish to bld-pkgs')
					{
						jenkinsHelpers.publishToBldPkgs()
					}
				}
			}
		}

		currentBuild.result = 'SUCCESS'
	}
	catch (err)
	{
		echo err.toString()
		currentBuild.result = "FAILED"
	}
	finally
	{
		stage('Cleanup')
		{
			parallel([
				CleanupVms: { jenkinsHelpers.cleanupVMs() },
				CleanupChefArtifacts: { jenkinsHelpers.cleanupChefArtifacts() }
			])
		}

		stage('Reporting')
		{
			jenkinsHelpers.reporting()
		}
	}
}
