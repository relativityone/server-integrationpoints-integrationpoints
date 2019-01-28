#!groovy

// Based on https://git.kcura.com/projects/RAID/repos/rmtjobs/browse/Template.jenkinsfile
// Set PipelineTools label to the same as here: https://git.kcura.com/projects/REL/repos/relativity/browse/Junkinsfile

library 'PipelineTools@RMT-9.3.1'
library 'SCVMMHelpers@3.2.0'
library 'GitHelpers@1.0.0'
library 'SlackHelpers@3.0.0'

import groovy.transform.Field

properties([
    [$class: 'BuildDiscarderProperty', strategy: [$class: 'LogRotator', artifactDaysToKeepStr: '30', artifactNumToKeepStr: '', daysToKeepStr: '30', numToKeepStr: '']],
    parameters([
        string(defaultValue: '', description: 'Set Relativity branch', name: 'relativityBranch'),
        string(defaultValue: 'DEV', description: 'Set Relativity build type (DEV, GOLD, etc.)', name: 'relativityBuildType'),
        string(defaultValue: '', description: 'Set Relativity version on which RIP will be tested. Leave blank to set the latest version.', name: 'relativityBuildVersion'),
        booleanParam(defaultValue: false, description: 'Check if you want to skip Integrations Tests stage.', name: 'skipIntegrationTests'),
        booleanParam(defaultValue: true, description: 'Check if you want to skip UI Tests stage.', name: 'skipUITests'),
        string(defaultValue: 'cat==SmokeTest', description: 'Set filter for integration and UI tests', name: 'testsFilter'),
        string(defaultValue: '', description: 'Set filter for nightly integration and UI tests', name: 'nightlyTestsFilter'),
        booleanParam(defaultValue: true, description: 'Enable SonarQube analysis for develop branch.', name: 'enableSonarAnalysis')
    ])
])

@Field
def sut = null

def nightlyJobName = "IntegrationPointsNightly"
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

// Set this the same as in psake-test.ps1 in DevelopmentScripts
def integration_tests_results_file_path = "DevelopmentScripts/IntegrationTestsResults.xml"
def integration_tests_html_report = "IntegrationTestsResults.html"
def integration_tests_report_task = "generate_integration_tests_report"
def ui_tests_results_file_path = "DevelopmentScripts/UITestsResults.xml"
def ui_tests_html_report = "UITestsResults.html"
def ui_tests_report_task = "generate_ui_tests_report"

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
	catchError
	{
		node ('PolandBuild')
		{
			try
			{
				stage ('Checkout')
				{
					timeout(time: 3, unit: 'MINUTES')
					{
						checkout scm
						step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
					}

				}
				stage ('Build')
				{
					def sonarParameter = shouldRunSonar(params.enableSonarAnalysis, env.BRANCH_NAME, nightlyJobName)
					powershell "./build.ps1 release $sonarParameter"
					archiveArtifacts artifacts: "DevelopmentScripts/*.html", fingerprint: true
				}
				stage ('Unit Tests')
				{
					timeout(time: 3, unit: 'MINUTES')
					{
						powershell "./build.ps1 release -sk -t"
						archiveArtifacts artifacts: "TestLogs/*", fingerprint: true
						currentBuild.result = 'SUCCESS'
					}
				}
				timeout(time: 3, unit: 'MINUTES')
				{
					stage ('Stash Tests Artifacts')
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
			}
			finally
			{
				deleteDir()
			}
		}
		if (testingVMsAreRequired())
		{
			stage('Install RAID')
			{
				node (agentsPool)
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
									(relativityBuildVersion, relativityBranch, relativityBuildType) = getNewBranchAndVersion(relativityBranchFallback, relativityBranch, params.relativityBuildVersion, params.relativityBuildType, session_id)
									echo "Installing Relativity, branch: $relativityBranch, version: $relativityBuildVersion, type: $relativityBuildType"
								}
								
								uploadEnvironmentFile(this, sut.name, relativityBuildVersion, relativityBranch, relativityBuildType,
									"", "", // invariant version and branch
									ripCookbooks, chef_attributes, knife,
									"", "", // analytics version and branch
									session_id, installing_relativity, installing_invariant, installing_analytics
								)
								
								addRunlist(this, session_id, sut.name, sut.domain, sut.ip, run_list, knife, profile, event_hash, "", "")
								
								checkWorkspaceUpgrade(this, sut.name, session_id)
							},
							ProvisionNodes:
							{
								def numberOfSlaves = 1
								def numberOfExecutors = '1'
								ScvmmInstance.createNodes(numberOfSlaves, 60, numberOfExecutors)
								bootstrapDependencies(this, python_packages, relativityBranch, relativityBuildVersion, relativityBuildType, session_id)
							}
						)
					}
				}
			}
			node ("$session_id && dependencies")
			{
				timeout(time: 3, unit: 'MINUTES')
				{
					stage ('Unstash Tests Artifacts')
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
				catchError
				{
					stage ('Integration Tests')
					{
						timeout(time: 180, unit: 'MINUTES')
						{
							runTests(params.skipIntegrationTests, "-in", "Integration", nightlyJobName)
						}
					}
					//if(isNightly(nightlyJobName)) //commented out temporarly for testing purposes
					//{
					stage ('Integration Tests in Quarantine')
					{
						timeout(time: 180, unit: 'MINUTES')
						{
							runTests(params.skipIntegrationTests, "-in", "Quarantined Integration", nightlyJobName)
						}
					}
					//}
					stage ('UI Tests')
					{
						timeout(time: 8, unit: 'HOURS')
						{
							runTests(params.skipUITests, "-ui", "UI", nightlyJobName)
						}
					}
					currentBuild.result = 'SUCCESS'
				}
				stage ('Gathering test stats')
				{
					timeout(time: 5, unit: 'MINUTES')
					{				
						archiveTestsArtifacts(params.skipIntegrationTests, integration_tests_results_file_path, integration_tests_html_report, integration_tests_report_task)
						numberOfFailedTests = getTestsStatistic(integration_tests_results_file_path, 'failed')
						numberOfPassedTests = getTestsStatistic(integration_tests_results_file_path, 'passed')
						numberOfSkippedTests = getTestsStatistic(integration_tests_results_file_path, 'skipped')
						archiveTestsArtifacts(params.skipUITests, ui_tests_results_file_path, ui_tests_html_report, ui_tests_report_task)
						if (!params.skipUITests)
						{
							archiveArtifacts artifacts: "lib/UnitTests/app.jeeves-ci.config", fingerprint: true
							archiveArtifacts artifacts: "lib/UnitTests/*.png", fingerprint: true
						}
					}
				}
			}
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
										user = input(message: 'Save the VMs?', ok: 'Save', submitter: 'JNK-Basic', submitterParameter: 'submitter')
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
						slackSend channel: getSlackChannelName(nightlyJobName).toString(), color: "E8E8E8", message: "${message}", teamDomain: 'kcura-pd', token: token
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

def shouldRunSonar(Boolean enableSonarAnalysis, String branchName, String nightlyJobName)
{
	return (enableSonarAnalysis && branchName == "develop" && !isNightly(nightlyJobName))
						? "-sonarqube"
						: ""
}


def configureNunitTests()
{
    withCredentials([usernamePassword(credentialsId: 'eddsdbo', passwordVariable: 'eddsdboPassword', usernameVariable: 'eddsdboUsername')])
    {
        def configuration_command = """python -m jeeves.create_config -t nunit -n "app.jeeves-ci" --dbuser "${eddsdboUsername}" --dbpass "${eddsdboPassword}" -s "${sut.name}.${sut.domain}" -db "${sut.name}\\EDDSINSTANCE001" -o .\\lib\\UnitTests\\"""
        bat script: configuration_command
    }
}

def isNightly(String nightlyJobName)
{
    return env.JOB_NAME.contains(nightlyJobName)
}

def isQuarantined(String testName)
{
	return testName == "Quarantined Integration";
}

def getSlackChannelName(String nightlyJobName)
{
    if (isNightly(nightlyJobName) && env.BRANCH_NAME == "develop")
    {
        return "#cd_rip_nightly"
    }
    else
    {
        return "#cd_rip_${env.BRANCH_NAME}"
    }
}

def getQuarantinedTestCategory()
{
	return "InQuarantine"
}

def getTestFilterWithoutQuarantined(String testFilter)
{
	def notQuarantinedTestFilter = "cat!=${getQuarantinedTestCategory()}"
	if(testFilter == "")
	{
		return notQuarantinedTestFilter
	}
	return "${testFilter} and ${notQuarantinedTestFilter}"
}

def getTestsFilter(String testName, String nightlyJobName)
{
    echo "env.JOB_NAME $env.JOB_NAME"

	if(isNightly(nightlyJobName))
	{
		if(isQuarantined(testName))
		{
			return "cat==${getQuarantinedTestCategory()}"
		}
		return getTestFilterWithoutQuarantined(params.nightlyTestsFilter)
	}

	return getTestFilterWithoutQuarantined(params.testsFilter)
}

def runTests(Boolean skipTests, String cmdOption, String testName, String nightlyJobName)
{
    if (!skipTests) 
    {
        configureNunitTests()
        def currentFilter = getTestsFilter(testName, nightlyJobName)
        def result = powershell returnStatus: true, script: "./build.ps1 -sk $cmdOption $currentFilter"
        if (result != 0)
        {
            error "$testName Tests FAILED with status: $result"
        }
        echo "$testName Tests OK"
    }
    else
    {
        echo "$testName Tests are going to be skipped."
    }
}

def getTestsStatistic(String filePath, String prop)
{
    try
    {
        def cmd = ('''
            [xml]$testResults = Get-Content ''' 
            + filePath 
            + '''; $testResults.'test-run'.'''
            + "'$prop'")
        echo "getTestsStatistic cmd: $cmd"
        def stdout = powershell returnStdout: true, script: cmd
        echo "getTestsStatistic result: $stdout"
        if (stdout)
        {
            return stdout
        }
        else
        {
            return -1
        }
    }
    catch(err)
    {
        echo "getTestsStatistic error: $err"
        return -1
    }
}

def archiveTestsArtifacts(Boolean skipTests, String resultsFilePath, String reportFile, String generateHtmlReportTaskName)
{
    try
    {
        if (!skipTests) 
        {
            archiveArtifacts artifacts: "${resultsFilePath}", fingerprint: true
            powershell "Import-Module ./Vendor/psake/tools/psake.psm1; Invoke-psake ./DevelopmentScripts/psake-test.ps1 $generateHtmlReportTaskName"
            archiveArtifacts artifacts: "DevelopmentScripts/${reportFile}", fingerprint: true
        }
    }
    catch(err)
    {
        echo "archiveTestsArtifacts failed with error: $err"
    }
}

def testingVMsAreRequired()
{
    return !params.skipIntegrationTests || !params.skipUITests
}

def getNewBranchAndVersion(String relativityBranchFallback, String relativityBranch, String paramRelativityBuildVersion, String paramRelativityBuildType, String sessionId)
{	
	def firstFallbackBranch = relativityBranchFallback // we should change first fallback branch on RIP release branches
	def GOLD_BUILD_TYPE = "GOLD"
	def DEV_BUILD_TYPE = "DEV"
	def relativityBranchesToTry = [[relativityBranch, paramRelativityBuildType], [firstFallbackBranch, DEV_BUILD_TYPE], [firstFallbackBranch, GOLD_BUILD_TYPE], ["master", GOLD_BUILD_TYPE]]

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

def tryGetBuildVersion(String relativityBranch, String paramRelativityBuildVersion, String paramRelativityBuildType, String sessionId)
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
		echo "Error occured while getting build version for: '$relativityBranch' Relativity branch, version '$paramRelativityBuildVersion', and type '$paramRelativityBuildType', error: $err"
		return null
	}
}

def isTrue(s)
{
    s.trim() == "True"
}

def isRelativityBranchPresent(branch)
{
    return isTrue(powershell(returnStdout: true, script: "([System.IO.DirectoryInfo]\"//bld-pkgs/Packages/Relativity/$branch\").Exists"))
}

def getLatestVersion(branch, type)
{
    return powershell(returnStdout: true, script: String.format('''
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
					''', branch, type)).trim()
}

def checkRelativityArtifacts(branch, version, type)
{
    return isTrue(powershell(returnStdout: true, script: "([System.IO.FileInfo]\"//bld-pkgs/Packages/Relativity/$branch/$version/MasterPackage/$type $version Relativity.exe\").Exists"))
}
