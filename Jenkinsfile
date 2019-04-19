#!groovy

library 'PipelineTools@RMT-9.3.1'
library 'SCVMMHelpers@3.2.0'
library 'GitHelpers@1.0.0'
library 'SlackHelpers@3.0.0'

properties([
	pipelineTriggers(env.JOB_NAME.contains("IntegrationPointsNightly") ? [cron('H 16 * * *')] : []),
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
		)
	])
])

// *********
// IMPORTANT
// *********
// Set variable below to the branch name, when you create new release branch!!!
// This should be changed on the release branch
def relativityBranchFallback = "release-qgate-foxglove-ea"

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
				jenkinsHelpers.initializeRIPPipeline(this, env, params)
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
		}
		
		if (jenkinsHelpers.testingVMsAreRequired(params))
		{
			node('SCVMM-AGENTS-POOL')
			{
				// Provision SUT
				stage('Install RAID')
				{
					jenkinsHelpers.raid(relativityBranchFallback)
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
						jenkinsHelpers.runIntegrationTests()
					}
					if (jenkinsHelpers.isNightly())
					{
						stage ('Integration Tests in Quarantine')
						{
							jenkinsHelpers.runIntegrationTestsInQuarantine()
						}
					}
					stage ('UI Tests')
					{
						withEnv([
							"UITestsBrowser=$params.UITestsBrowser"
							]) {
							jenkinsHelpers.downloadAndSetUpBrowser()
							jenkinsHelpers.runUiTests()
							}
					}
				}
				finally
				{
					stage ('Gathering test stats')
					{
						jenkinsHelpers.gatherTestStats()
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
		stage('Cleanup VMs')
		{
			jenkinsHelpers.cleanupVMs()
		}

		stage('Reporting')
		{
			jenkinsHelpers.reporting()
		}
	}
}
