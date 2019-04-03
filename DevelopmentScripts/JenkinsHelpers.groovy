/*
Required libraries:

library 'PipelineTools@RMT-9.3.1'
library 'SCVMMHelpers@3.2.0'
library 'GitHelpers@1.0.0'
library 'SlackHelpers@3.0.0'

 */

/**
 ** CONSTANTS
 **/
interface Constants
{
    // This repo's package name for purposes of versioning & publishing
    final String PACKAGE_NAME = 'IntegrationPoints'
    final String NIGHTLY_JOB_NAME = "IntegrationPointsNightly"
    final String UI_NIGHTLY_JOB_NAME = "IntegrationPointsUITests"
    final String ARTIFACTS_PATH = 'Artifacts'
    final String QUARANTINED_TESTS_CATEGORY = 'InQuarantine'
    final String INTEGRATION_TESTS_RESULTS_REPORT_PATH = "$ARTIFACTS_PATH/IntegrationTestsResults.xml"
    final String INTEGRATION_TESTS_IN_QUARANTINE_RESULTS_REPORT_PATH = "$ARTIFACTS_PATH/QuarantineIntegrationTestsResults.xml"
}

class RIPPipelineState
{
    final script
    final env
    final params
    final String sessionId = System.currentTimeMillis().toString()

    String eventHash
    def relativityBuildVersion = ""
    def relativityBuildType = ""
    def relativityBranch = ""

    def version
    def commonBuildArgs
    def scvmmInstance
    def sut

    def numberOfFailedTests = -1
    def numberOfPassedTests = -1
    def numberOfSkippedTests = -1

    RIPPipelineState(script, env, params)
    {
        this.script = script
        this.env = env
        this.params = params
    }

    def getServerFromPool()
    {
        eventHash = java.security.MessageDigest.getInstance("MD5").digest(env.JOB_NAME.bytes).encodeHex().toString()
        script.echo "Getting server from pool, sessionId: $sessionId, Relativity build type: $params.relativityBuildType, event hash: $eventHash"

        scvmmInstance = script.scvmm(script, sessionId)
        scvmmInstance.setHoursToLive("12")

        sut = scvmmInstance.getServerFromPool()
        script.echo "Acquired server: ${sut.name} @ ${sut.domain} (${sut.ip})"
    }

    def provisionNodes()
    {
        script.echo "Start provisioning for id ($sessionId)"
        // Make changes here if necessary.
        final String pythonPackages = 'jeeves==4.1.0 phonograph==5.2.0 selenium==3.0.1'
        def numberOfSlaves = 1
        def numberOfExecutors = '1'
        scvmmInstance.createNodes(numberOfSlaves, 60, numberOfExecutors)
        script.bootstrapDependencies(
            script, 
            pythonPackages, 
            relativityBranch, 
            relativityBuildVersion, 
            relativityBuildType, 
            sessionId
        )
        script.echo "Provisioning DONE"
    }

}

// State for the whole pipeline
ripPipelineState = null

def initializeRIPPipeline(script, env, params)
{
    ripPipelineState = new RIPPipelineState(script, env, params)
    ripPipelineState.relativityBranch = params.relativityBranch ?: env.BRANCH_NAME
}

/*
 * Returns true if the job is a nightly job based on naming convention
 */
def isNightly()
{
	return env.JOB_NAME.contains(Constants.NIGHTLY_JOB_NAME)
}

def isUiNightly()
{
	return env.JOB_NAME.contains(Constants.UI_NIGHTLY_JOB_NAME)
}

def getVersion()
{
    def version = incrementBuildVersion(Constants.PACKAGE_NAME, params.relativityBuildType)
    currentBuild.displayName = "$params.relativityBuildType-$version"
    ripPipelineState.commonBuildArgs = "release $params.relativityBuildType -ci -v $version -b $env.BRANCH_NAME"
    ripPipelineState.version = version
    echo "RIPPipeline::getVersion set commonBuildArgs to: $ripPipelineState.commonBuildArgs"
}

def build()
{
    def sonarParameter = shouldRunSonar(params.enableSonarAnalysis, env.BRANCH_NAME)
    powershell "./build.ps1 $sonarParameter $ripPipelineState.commonBuildArgs"
    archiveArtifacts artifacts: "DevelopmentScripts/*.html", fingerprint: true
}

def unitTest()
{
    timeout(time: 3, unit: 'MINUTES')
    {
        powershell "./build.ps1 -sk -t $ripPipelineState.commonBuildArgs"
        archiveArtifacts artifacts: "TestLogs/*", fingerprint: true
        currentBuild.result = 'SUCCESS'
    }
}

def packageRIP()
{
    powershell "./build.ps1 -sk -package -root ./BuildPackages $ripPipelineState.commonBuildArgs"
}

def stashTestsAndPackageArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
        stashCommonArtifacts()
		stashPackageOnlyArtifacts()
		stashTestsOnlyArtifacts()
    }
}

def unstashTestsAndPackageArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
        unstashCommonArtifacts()
		unstashPackageOnlyArtifacts()
		unstashTestsOnlyArtifacts()
    }
}

def stashTestsArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
        stashCommonArtifacts()
		stashTestsOnlyArtifacts()
    }
}

def unstashTestsArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
        unstashCommonArtifacts()
		unstashTestsOnlyArtifacts()
    }
}

def stashPackageArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
		stashCommonArtifacts()
		stashPackageOnlyArtifacts()
    }
}

def unstashPackageArtifacts()
{
    timeout(time: 3, unit: 'MINUTES')
    {
		unstashCommonArtifacts()
		unstashPackageOnlyArtifacts()
    }
}

def testingVMsAreRequired(params)
{
	return !params.skipIntegrationTests || !params.skipUITests
}

/*
 * @param relativityBranchFallback - the default fallback branch. develop by default, but should be set to release branch name on release branches!!!
 */
def raid(String relativityBranchFallback)
{
    timeout(time: 90, unit: 'MINUTES')
    {
        ripPipelineState.getServerFromPool()
        def sut = ripPipelineState.sut

        final installingRelativity = true
        final installingInvariant = false
        final installingAnalytics = false
        final installingDatagrid = false

        // Do not modify.
        final runList = createRunList(installingRelativity, installingInvariant, installingAnalytics, installingDatagrid)
        final profile = createProfile(installingRelativity, installingInvariant, installingAnalytics, installingDatagrid)
        final knife = 'C:\\Python27\\Lib\\site-packages\\jeeves\\knife.rb'
        def chefAttributes = 'fluidOn:1,cdonprem:1'
        def ripCookbooks = getCookbooks()

        def relativityBuildVersion = ripPipelineState.relativityBuildVersion
        def relativityBranch = ripPipelineState.relativityBranch
        def relativityBuildType = ripPipelineState.relativityBuildType

        def sessionId = ripPipelineState.sessionId
        def script = ripPipelineState.script
        def eventHash = ripPipelineState.eventHash

        parallel (
            Deploy:
            {
                if (installingRelativity)
                {
                    (relativityBuildVersion, relativityBranch, relativityBuildType) = getNewBranchAndVersion(
                        relativityBranchFallback, 
                        relativityBranch, 
                        params.relativityBuildVersion, 
                        params.relativityBuildType, 
                        sessionId
                    )
                    ripPipelineState.relativityBuildVersion = relativityBuildVersion
                    ripPipelineState.relativityBranch = relativityBranch
                    ripPipelineState.relativityBuildType = relativityBuildType

                    echo "Installing Relativity, branch: $relativityBranch, version: $relativityBuildVersion, type: $relativityBuildType"
                }

                echo "Uploading environment files"
                uploadEnvironmentFile(
                    script, 
                    sut.name, 
                    relativityBuildVersion, 
                    relativityBranch, 
                    relativityBuildType,
                    "", //invariant version
                    "", //invariant branch
                    ripCookbooks, 
                    chefAttributes, 
                    knife,
                    "", //analytics version
                    "", //analytics branch
                    sessionId, 
                    installingRelativity, 
                    installingInvariant, 
                    installingAnalytics
                )

                echo "Calling addRunList"
                addRunlist(
                    script, 
                    sessionId, 
                    sut.name, 
                    sut.domain, 
                    sut.ip, 
                    runList, 
                    knife, 
                    profile, 
                    eventHash, 
                    "", 
                    ""
                )

                echo "Checking workspace upgrade"
                checkWorkspaceUpgrade(script, sut.name, sessionId)
            },
            ProvisionNodes:
            {
                ripPipelineState.provisionNodes()
            }
        )
    }
}

def getSessionId()
{
    return ripPipelineState.sessionId
}

def runIntegrationTests()
{
    timeout(time: 180, unit: 'MINUTES')
    {
        runTestsAndSetBuildResult(TestType.integration, params.skipIntegrationTests)
    }
}

def runUiTests()
{
    timeout(time: 10, unit: 'HOURS')
    {
        runTestsAndSetBuildResult(TestType.ui, params.skipUITests)
    }
}

def runIntegrationTestsInQuarantine()
{
	if (params.skipIntegrationTests)
	{
		return
	}
    timeout(time: 180, unit: 'MINUTES')
    {
        runTests(TestType.integrationInQuarantine, params)
    }
}

def gatherTestStats()
{
    timeout(time: 5, unit: 'MINUTES')
    {
        if (!params.skipUITests)
        {
            archiveArtifacts artifacts: "lib/UnitTests/app.jeeves-ci.config", fingerprint: true
            archiveArtifacts artifacts: "lib/UnitTests/*.png", fingerprint: true, allowEmptyArchive: true
        }

        powershell "Import-Module ./Vendor/psake/tools/psake.psm1; Invoke-psake ./DevelopmentScripts/psake-test.ps1 generate_nunit_reports" 
        def artifactsPath = Constants.ARTIFACTS_PATH
        archiveArtifacts artifacts: "$artifactsPath/**/*", fingerprint: true, allowEmptyArchive: true

        if(isNightly())
        {
            storeIntegrationTestsInQuarantineResults()
        }

        if (!params.skipIntegrationTests)
        {
            ripPipelineState.numberOfFailedTests = getTestsStatistic('failed')
            ripPipelineState.numberOfPassedTests = getTestsStatistic('passed')
            ripPipelineState.numberOfSkippedTests = getTestsStatistic('skipped')
        }
    }
}

def publishToNuget()
{
    withCredentials([string(credentialsId: 'ProgetNugetApiKey', variable: 'key')])
    {
        retry(3)
        {
            powershell "./build.ps1 -sk -nuget $key $ripPipelineState.commonBuildArgs"
        }
    }
}

/**
 * Publish a local package to the bld-pkgs share. Packages should be built under the
 * folder structure '<root>\<packageName>\<branch>\<version>'.
 */
def publishToBldPkgs()
{
    // * @param username      Username to use when authenticating with blg-pkgs.
    // * @param password      Password to use when authenticating with blg-pkgs.
    def credentials = [
        usernamePassword(
            credentialsId: 'jenkins_packages_svc', 
            passwordVariable: 'BLDPKGSPASSWORD', 
            usernameVariable: 'BLDPKGSUSERNAME'
        )
    ]
    withCredentials(credentials)
    {
        def username = BLDPKGSUSERNAME
        def password = BLDPKGSPASSWORD
        // @param localPackages Local directory containing packaged code, e.g. "./BuildPackages".
        def localPackages = './BuildPackages'
        // Name of the package. Equivalent to the "Product" for purposes of build versioning or the folder in the bld-pkgs Packages folder.
        def packageName = Constants.PACKAGE_NAME
        // Git branch on which the build is being run.
        def branch = env.BRANCH_NAME
        // Version of the package to publish.
        def version = ripPipelineState.version
        powershell """
            net use \\\\bld-pkgs\\Packages\\$packageName /user:kcura\\$username "$password"
            try
            {
                \$destination_path = "\\\\BLD-PKGS.kcura.corp\\Packages\\$packageName\\$branch\\$version"
                \$source_path = Join-Path '$localPackages' '$packageName\\$branch\\$version'
                & .\\DevelopmentScripts\\Invoke-Robocopy.ps1 -Source \$source_path -Destination \$destination_path -Verbose
            }
            finally
            {
                net use \\\\bld-pkgs\\Packages\\$packageName /DELETE /Y
            }
        """
    }
}

def cleanupVMs()
{
    try
    {
        timeout(time: 20, unit: 'MINUTES')
        {
            if(ripPipelineState.sut?.name)
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
                        ripPipelineState.scvmmInstance.saveVMs(user)
                    }
                    // Exception is thrown if you click abort or let it time out
                    catch(err)
                    {
                        echo "Deleting VMs..."
                        ripPipelineState.scvmmInstance.deleteVMs()
                    }
                }
            }
            deleteNodes(ripPipelineState.script, ripPipelineState.sessionId)
        }
    }
    catch (err)
    {
        echo "Cleanup VMs FAILED."
    }

}

def reporting()
{
    try
    {
        echo "Build result: $currentBuild.result"
        node("SCVMM-AGENTS-POOL")
        {
            timeout(time: 3, unit: 'MINUTES')
            {
                step([$class: 'StashNotifier', ignoreUnverifiedSSLPeer: true])
                withCredentials([string(credentialsId: 'SlackJenkinsIntegrationToken', variable: 'token')])
                {
                    message = "*${currentBuild.result.toString()}* ${((currentBuild.result.toString() == "FAILURE") ? ":alert:" : "" )} \n\n" +
                        "Build *#${env.BUILD_NUMBER}* from *${env.BRANCH_NAME}*.\n" +
                        ":greencheck: Passed tests: ${ripPipelineState.numberOfPassedTests}\n" +
                        ":negative_squared_cross_mark: Failed tests: ${ripPipelineState.numberOfFailedTests}\n" +
                        ":yellow_card: Skipped tests: ${ripPipelineState.numberOfSkippedTests} \n\n" +
                        "${env.BUILD_URL} \n" +
                        "Relativity branch: ${ripPipelineState.relativityBranch} \n" +
                        "Relativity build type: ${ripPipelineState.relativityBuildType} \n" +
                        "Relativity build version: ${(ripPipelineState.relativityBuildVersion ?: "0.0.0.0")}"
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

def deleteDirectoryIfExists(String directoryToDelete)
{
    dir(directoryToDelete)
	{
		deleteDir()
	}
}


/*****************
 **** PRIVATE ****
/*****************

/**
* Return the current build version in the TeamCity versioning database & increment
* for the next build if necessary. Basically a pass-through to the New-TeamCityBuildVersion.ps1
* script. Examine that script for info about how CI build versioning works.
*
* @param packageName Name of the package being versioned. Equivalent to the "Product" for purposes of build versioning or the folder in the bld-pkgs Packages folder.
* @param buildType   Build type of the current build, e.g. 'DEV', 'GOLD', etc. Affects how the next version is incremented.
* @return String indicating the current build version, e.g. "10.2.1.3".
*/
private incrementBuildVersion(String packageName, String buildType)
{
    def versionOutput = powershell(returnStdout: true, script: ".\\DevelopmentScripts\\New-TeamCityBuildVersion.ps1 -Product '$packageName' -Project 'Development' -ServerType 'Jenkins' -BuildType '$buildType'")
    return versionOutput.tokenize()[0]
}

/*
 * Check whether boolean value returned from Powershell represents true
 *
 * @param s - string result from powershell script
 * @return -  True if the script result is considered true
 */
private isPowershellResultTrue(s)
{
	return s.trim() == "True"
}

private shouldRunSonar(Boolean enableSonarAnalysis, String branchName)
{
	return (enableSonarAnalysis && branchName == "develop" && !isNightly())
			? "-sonarqube"
			: ""
}

private getSlackChannelName()
{
	if (isNightly() && env.BRANCH_NAME == "develop")
	{
		return "#cd_rip_nightly"
	}
    if (isUiNightly() && env.BRANCH_NAME == "develop")
	{
		return "#cd_rip_ui_nightly"
	}
	return "#cd_rip_${env.BRANCH_NAME}"
}

/*
 * Checks whether folder for given Relativity branch exists in build packages
 */
private isRelativityBranchPresent(String branch)
{
	def command = "([System.IO.DirectoryInfo]\"//bld-pkgs/Packages/Relativity/$branch\").Exists"
	return isPowershellResultTrue(powershell(returnStdout: true, script: command))
}

private getLatestVersion(String branch, String type)
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

private checkRelativityArtifacts(String branch, String version, String type)
{
	def command = "([System.IO.FileInfo]\"//bld-pkgs/Packages/Relativity/$branch/$version/MasterPackage/$type $version Relativity.exe\").Exists"
	def result = powershell(returnStdout: true, script: command)
	return isPowershellResultTrue(result)
}

private tryGetBuildVersion(
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

private getNewBranchAndVersion(
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


/**********************
 * Testing helpers
 **********************
 */
interface TestType {
    final integration = "integration"
    final ui = "ui"
    final integrationInQuarantine = "integrationInQuarantine"
}

/* Return test name based on the type of the test */
private testStageName(testType)
{
    if (testType == TestType.integration)
    {
        return "Integration Tests"
    }
    if (testType == TestType.ui)
    {
        return "UI Tests"
    }
    if (testType == TestType.integrationInQuarantine)
    {
        return "Integration Tests in Quarantine"
    }
}

/* Return command line option for powershell build script based on the type of the test */
private testCmdOptions(testType)
{
    if (TestType.integration == testType)
    {
        return "-in"
    }
    if (TestType.ui == testType)
    {
        return "-ui"
    }
    if (TestType.integrationInQuarantine == testType)
    {
        return "-qu -in"
    }
}

/*
 * Function creates configuration file using some python helper library for integration and ui tests
 * @param sut - sut returned from ScvmmInstance.getServerFromPool() 
 */
private configureNunitTests(sut)
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

/*
 * Returns filter for NUnit
 */
private exceptQuarantinedTestFilter()
{
	return "cat != $Constants.QUARANTINED_TESTS_CATEGORY"
}

/*
 * Returns filter for NUnit
 */
private withQuarantinedTestFilter()
{
	return "cat == $Constants.QUARANTINED_TESTS_CATEGORY"
}

/*
 * Get NUnit filter for particular test based also on the pipeline type - whether it is nightly or not
 * @param - testType - string value which should be equal to one of the values from TestType interface
 * @param - params - the params object in the pipeline
 */
private getTestsFilter(testType, params)
{
	def paramsTestsFilter = isNightly() ? params.nightlyTestsFilter : params.testsFilter
	return isQuarantine(testType)
		? unionTestFilters(paramsTestsFilter, withQuarantinedTestFilter())
		: unionTestFilters(paramsTestsFilter, exceptQuarantinedTestFilter())
}

/* 
 * Helper function for running specific type of test
 * @param - params - the params object in the pipeline
 */
private runTests(testType, params)
{
	configureNunitTests(ripPipelineState.sut)
	def cmdOptions = testCmdOptions(testType)
    def currentFilter = getTestsFilter(testType, params)
    def result = powershell returnStatus: true, script: "./build.ps1 -ci -sk $cmdOptions \"\"\"$currentFilter\"\"\""
	return result
}

private runTestsAndSetBuildResult(testType, Boolean skipTests) 
{ 
    echo "runTestsAndSetBuildResult test: $testType"
	def stageName = testStageName(testType)

    if (skipTests)
	{
		echo "$stageName are going to be skipped."
		return
	}

    def result = runTests(testType, params) 
    if (result != 0) 
    { 
		currentBuild.result = "FAILED"
        error "$stageName FAILED with status: $result"
    } 
    echo "$stageName OK" 
}

private unionTestFilters(String testFilter, String andTestFilter)
{
	if(testFilter == "")
	{
		return andTestFilter
	}
	return "${testFilter} && ${andTestFilter}"
}

private isQuarantine(testType)
{
	return testType == TestType.integrationInQuarantine
}

private getTestsStatistic(String prop)
{
	try
	{
		def cmd = ('''
			[xml]$testResults = Get-Content '''
			+ Constants.INTEGRATION_TESTS_RESULTS_REPORT_PATH  
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

private storeIntegrationTestsInQuarantineResults()
{
	try
	{
		withCredentials([string(credentialsId: 'TestResultAnalyzerStoreTestsResultsFunctionSecurityCode', variable: 'securityCode')])
		{
			def branchId = env.BRANCH_NAME
			def buildName = currentBuild.displayName
			def testType = TestType.integrationInQuarantine.capitalize()
			def testResultsPath = "$env.WORKSPACE/$Constants.INTEGRATION_TESTS_IN_QUARANTINE_RESULTS_REPORT_PATH"

			powershell script: """. ./DevelopmentScripts/test-results-analyzer.ps1; store_tests_results "$branchId" "$buildName" "$testType" "$testResultsPath" "$securityCode" """
		}
	}
	catch(err)
	{
		echo "storeIntegrationTestsInQuarantineResults error: $err"
	}
}

/**********************
 * Stash helpers
 **********************
 */

private stashCommonArtifacts()
{
	stash includes: 'DevelopmentScripts/*.ps1', name: 'buildScripts'
    stash includes: 'build.ps1', name: 'buildps1'
    stash includes: 'Vendor/psake/tools/*', name: 'psake'
    stash includes: 'Vendor/NuGet/NuGet.exe', name: 'nuget'
    stash includes: 'Version/version.txt', name: 'version'
}

private unstashCommonArtifacts()
{	
    unstash 'buildScripts'
	unstash 'buildps1'
    unstash 'psake'
    unstash 'nuget'
    unstash 'version'
}

private stashPackageOnlyArtifacts()
{
	stash includes: 'BuildPackages/**', name: 'buildPackages'
	stash includes: 'DevelopmentScripts/NuGet/*', name: 'nugets'   
}

private unstashPackageOnlyArtifacts()
{
	unstash 'buildPackages'
	unstash 'nugets'
}

private stashTestsOnlyArtifacts()
{
	stash includes: 'lib/UnitTests/**', name: 'testdlls'
    stash includes: 'DynamicallyLoadedDLLs/Search-Standard/*', name: 'dynamicallyLoadedDLLs'
    stash includes: 'Applications/RelativityIntegrationPoints.Auto.rap', name: 'integrationPointsRap'
    stash includes: 'DevelopmentScripts/IntegrationPointsTests.*', name: 'nunitProjectFiles'
    stash includes: 'DevelopmentScripts/NUnit.ConsoleRunner/tools/*', name: 'nunitConsoleRunner'
    stash includes: 'DevelopmentScripts/NUnit.Extension.NUnitProjectLoader/tools/*', name: 'nunitProjectLoader'
}

private unstashTestsOnlyArtifacts()
{
	unstash 'testdlls'
    unstash 'dynamicallyLoadedDLLs'
    unstash 'integrationPointsRap'
    unstash 'nunitProjectFiles'
    unstash 'nunitConsoleRunner'
    unstash 'nunitProjectLoader'
}

return this
