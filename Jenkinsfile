#!groovy

// Based on https://git.kcura.com/projects/RAID/repos/rmtjobs/browse/Template.jenkinsfile
// Set PipelineTools label to the same as here: https://git.kcura.com/projects/REL/repos/relativity/browse/Junkinsfile

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

enum TestType {
    integration,
    ui,
    integrationInQuarantine
}

// This repo's package name for purposes of versioning & publishing
@Field final String PACKAGE_NAME = 'IntegrationPoints'
@Field final String NIGHTLY_JOB_NAME = "IntegrationPointsNightly"
@Field final String ARTIFACTS_PATH = 'Artifacts'
@Field final String INTEGRATION_TESTS_RESULTS_REPORT_PATH = "$ARTIFACTS_PATH/IntegrationTestsResults.xml"
@Field final String QUARANTINED_TESTS_CATEGORY = 'InQuarantine'

@Field
def testStageName = [
	(TestType.integration) : "Integration Tests",
	(TestType.ui) : "UI Tests",
	(TestType.integrationInQuarantine) : "Integration Tests in Quarantine"
]

@Field
def testCmdOptions = [
	(TestType.integration) : "-in",
	(TestType.ui) : "-ui",
	(TestType.integrationInQuarantine) : "-qu -in"
]

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
			}
			stage ('Get Version')
			{
				jenkinsHelpers = load ("${pwd()}/DevelopmentScripts/JenkinsHelpers.groovy")

				version = jenkinsHelpers.incrementBuildVersion(PACKAGE_NAME, params.relativityBuildType)

				currentBuild.displayName="${params.relativityBuildType}-$version"
				commonBuildArgs = "release $params.relativityBuildType -ci -v $version -b $env.BRANCH_NAME"
			}
			stage ('Build')
			{
				def sonarParameter = shouldRunSonar(params.enableSonarAnalysis, env.BRANCH_NAME)
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

			if (testingVMsAreRequired())
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
									(relativityBuildVersion, relativityBranch, relativityBuildType) = getNewBranchAndVersion(
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
								runIntegrationTests()
							}
						}
						if(isNightly())
						{
							stage ('Integration Tests in Quarantine')
							{
								timeout(time: 180, unit: 'MINUTES')
								{
									runIntegrationTestsInQuarantine()
								}
							}
						}
						stage ('UI Tests')
						{
							updateChromeToLatestVersion()
							timeout(time: 8, unit: 'HOURS')
							{
								runUiTests()
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
                                archiveArtifacts artifacts: "$ARTIFACTS_PATH/**/*", fingerprint: true, allowEmptyArchive: true

								if (!params.skipIntegrationTests)
								{
                                    numberOfFailedTests = getTestsStatistic('failed')
                                    numberOfPassedTests = getTestsStatistic('passed')
                                    numberOfSkippedTests = getTestsStatistic('skipped')
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
						PACKAGE_NAME, 
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
							slackSend channel: getSlackChannelName().toString(), color: "E8E8E8", message: "${message}", teamDomain: 'kcura-pd', token: token
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

def shouldRunSonar(Boolean enableSonarAnalysis, String branchName)
{
	return (enableSonarAnalysis && branchName == "develop" && !isNightly())
			? "-sonarqube"
			: ""
}

def configureNunitTests()
{
	def credentials = usernamePassword(
		credentialsId: 'eddsdbo', 
		passwordVariable: 'eddsdboPassword', 
		usernameVariable: 'eddsdboUsername'
	)
	withCredentials([credentials])
	{
		def configuration_command = """python -m jeeves.create_config -t nunit -n "app.jeeves-ci" --dbuser "${eddsdboUsername}" --dbpass "${eddsdboPassword}" -s "${sut.name}.${sut.domain}" -db "${sut.name}\\EDDSINSTANCE001" -o .\\lib\\UnitTests\\"""
		bat script: configuration_command
	}
}

def isNightly()
{
	return env.JOB_NAME.contains(NIGHTLY_JOB_NAME)
}

def getSlackChannelName()
{
	if (isNightly() && env.BRANCH_NAME == "develop")
	{
		return "#cd_rip_nightly"
	}
	return "#cd_rip_${env.BRANCH_NAME}"
}

def exceptQuarantinedTestFilter()
{
	return "cat != $QUARANTINED_TESTS_CATEGORY"
}

def withQuarantinedTestFilter()
{
	return "cat == $QUARANTINED_TESTS_CATEGORY"
}

def unionTestFilters(String testFilter, String andTestFilter)
{
	if(testFilter == "")
	{
		return andTestFilter
	}
	return "${testFilter} && ${andTestFilter}"
}

def isQuarantine(TestType testType)
{
	return testType == TestType.integrationInQuarantine
}

def getTestsFilter(TestType testType)
{
	def paramsTestsFilter = isNightly() ? params.nightlyTestsFilter : params.testsFilter
	return isQuarantine(testType)
		? unionTestFilters(paramsTestsFilter, withQuarantinedTestFilter())
		: unionTestFilters(paramsTestsFilter, exceptQuarantinedTestFilter())
}

def runIntegrationTests()
{
    runTestsAndSetBuildResult(TestType.integration, params.skipIntegrationTests)
}

def runUiTests()
{
    runTestsAndSetBuildResult(TestType.ui, params.skipUITests)
}

def runIntegrationTestsInQuarantine()
{
	if(params.skipIntegrationTests)
	{
		return
	}
    runTests(TestType.integrationInQuarantine)
}

def runTestsAndSetBuildResult(TestType testType, Boolean skipTests) 
{ 
	def stageName = testStageName[testType]

    if (skipTests)
	{
		echo "$stageName are going to be skipped."
		return
	}

    def result = runTests(testType) 
    if (result != 0) 
    { 
        echo "$stageName FAILED with status: $result"
		currentBuild.result = "FAILED"
    } 
    echo "$stageName OK" 
}

def runTests(TestType testType)
{
	configureNunitTests()
	def cmdOptions = testCmdOptions[testType]
    def currentFilter = getTestsFilter(testType)
    def result = powershell returnStatus: true, script: "./build.ps1 -ci -sk $cmdOptions \"\"\"$currentFilter\"\"\""
	return result
}

def getTestsStatistic(String prop)
{
	try
	{
		def cmd = ('''
			[xml]$testResults = Get-Content '''
			+ INTEGRATION_TESTS_RESULTS_REPORT_PATH  
			+ '''; $testResults.'test-run'.'''
			+ "'$prop'")

		echo "getTestsStatistic cmd: $cmd"
		def stdout = powershell returnStdout: true, script: cmd
		echo "getTestsStatistic result: $stdout"

		return stdout ?: -1
	}
	catch(err)
	{
		echo "getTestsStatistic error: $err"
		return -1
	}
}

def testingVMsAreRequired()
{
	return !params.skipIntegrationTests || !params.skipUITests
}

def getNewBranchAndVersion(
	String relativityBranchFallback, 
	String relativityBranch, 
	String paramRelativityBuildVersion, 
	String paramRelativityBuildType, 
	String sessionId)
{
	def firstFallbackBranch = relativityBranchFallback // we should change first fallback branch on RIP release branches
	def GOLD_BUILD_TYPE = "GOLD"
	def DEV_BUILD_TYPE = "DEV"
	def relativityBranchesToTry = [
		[relativityBranch, paramRelativityBuildType], 
		[firstFallbackBranch, DEV_BUILD_TYPE], 
		[firstFallbackBranch, GOLD_BUILD_TYPE], 
		["master", GOLD_BUILD_TYPE]
	]

	for (branchAndType in relativityBranchesToTry)
	{
		def branch = branchAndType[0]
		def buildType = branchAndType[1]

		echo "Retrieving latest Relativity '$buildType' build from '$branch' branch"

		def buildVersion = tryGetBuildVersion(branch, paramRelativityBuildVersion, buildType, sessionId)
		if (buildVersion != null)
		{
			return [buildVersion, branch, buildType]
		}
	}

	error 'Failed to retrieve Relativity branch/version'
}

def tryGetBuildVersion(
	String relativityBranch, 
	String paramRelativityBuildVersion, 
	String paramRelativityBuildType, 
	String sessionId)
{
	try
	{
		if (!isRelativityBranchPresent(relativityBranch))
		{
			echo "Branch was not found: $relativityBranch"
			return null
		}
		def latestVersion = paramRelativityBuildVersion ?: getLatestVersion(relativityBranch, paramRelativityBuildType)
		echo "Checking Relativity artifacts for version: $latestVersion"
		return checkRelativityArtifacts(relativityBranch, latestVersion, paramRelativityBuildType)
				? latestVersion
				: null
	}
	catch (err)
	{
		echo "Error occured while getting build version for: '$relativityBranch' Relativity branch, version '$paramRelativityBuildVersion', type '$paramRelativityBuildType', error: $err"
		return null
	}
}

def isTrue(s)
{
	return s.trim() == "True"
}

def isRelativityBranchPresent(branch)
{
	def command = "([System.IO.DirectoryInfo]\"//bld-pkgs/Packages/Relativity/$branch\").Exists"
	return isTrue(powershell(returnStdout: true, script: command))
}

def getLatestVersion(branch, type)
{
	def command = '''
		$result = (Get-ChildItem -path "\\\\bld-pkgs\\Packages\\Relativity\\%1$s" |
			? { (Get-ChildItem -Path $_.FullName).Name -like "BuildType_%2$s" } |
			ForEach-Object { $_.Name } | ForEach-Object { [System.Version] $_ } | sort) | Select-Object -Last 1;
		if (!$result)
		{
			return ''
		}
		else
		{
			return $result.ToString()
		}
	'''

	return powershell(returnStdout: true, script: String.format(command, branch, type)).trim()
}

def checkRelativityArtifacts(branch, version, type)
{
	def command = "([System.IO.FileInfo]\"//bld-pkgs/Packages/Relativity/$branch/$version/MasterPackage/$type $version Relativity.exe\").Exists"
	def result = powershell(returnStdout: true, script: command)
	return isTrue(result)
}

def updateChromeToLatestVersion()
{
	try
    {
		powershell """
            Invoke-WebRequest "http://dl.google.com/chrome/install/latest/chrome_installer.exe" -OutFile chrome_installer.exe
            Start-Process -FilePath chrome_installer.exe -Args "/silent /install" -Verb RunAs -Wait
            (Get-Item (Get-ItemProperty "HKLM:/SOFTWARE/Microsoft/Windows/CurrentVersion/App Paths/chrome.exe")."(Default)").VersionInfo
        """
    }
    catch(err)
    {
        echo "An error occured while updating Chrome: $err"
    }
}
