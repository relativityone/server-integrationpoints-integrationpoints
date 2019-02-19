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
    final String ARTIFACTS_PATH = 'Artifacts'
    final String QUARANTINED_TESTS_CATEGORY = 'InQuarantine'
    final String INTEGRATION_TESTS_RESULTS_REPORT_PATH = "$ARTIFACTS_PATH/IntegrationTestsResults.xml"
}

// TODO remove once the pipeline is moved here
def getConstants()
{
    return Constants
}

class RIPPipelineState
{
    // *********
    // IMPORTANT
    // *********
    // Set variable below to the branch name, when you create new release branch!!!
    // This should be changed on the release branch
    final String relativityBranchFallback = "develop"

    final script
    final env
    final params
    final String sessionId = System.currentTimeMillis().toString()

    String eventHash
    def relativityBuildVersion = ""
    def relativityBuildType = ""
    def relativityBranch = ""

    def commonBuildArgs
    def scvmmInstance
    def sut

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

}

// State for the whole pipeline
ripPipelineState = null

def initializeRIPPipeline(script, env, params)
{
    ripPipelineState = new RIPPipelineState(script, env, params)
    ripPipelineState.relativityBranch = params.relativityBranch ?: env.BRANCH_NAME
}

def getVersion()
{
    def version = incrementBuildVersion(Constants.PACKAGE_NAME, params.relativityBuildType)
    currentBuild.displayName = "$params.relativityBuildType-$version"
    ripPipelineState.commonBuildArgs = "release $params.relativityBuildType -ci -v $version -b $env.BRANCH_NAME"
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

def stashTestsArtifacts()
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

def testingVMsAreRequired(params)
{
	return !params.skipIntegrationTests || !params.skipUITests
}

def raid()
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
                    echo "Installing Relativity, branch: $relativityBranch, version: $relativityBuildVersion, type: $relativityBuildType"
                }

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

                checkWorkspaceUpgrade(script, sut.name, sessionId)
            },
            ProvisionNodes:
            {
                // Make changes here if necessary.
                final String pythonPackages = 'jeeves==4.1.0 phonograph==5.2.0 selenium==3.0.1'
                def numberOfSlaves = 1
                def numberOfExecutors = '1'
                scvmmInstance.createNodes(numberOfSlaves, 60, numberOfExecutors)
                bootstrapDependencies(
                    script, 
                    pythonPackages, 
                    relativityBranch, 
                    relativityBuildVersion, 
                    relativityBuildType, 
                    sessionId
                )
            }
        )
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

/**
 * Publish a local package to the bld-pkgs share. Packages should be built under the
 * folder structure '<root>\<packageName>\<branch>\<version>'.
 *
 * @param username      Username to use when authenticating with blg-pkgs.
 * @param password      Password to use when authenticating with blg-pkgs.
 * @param localPackages Local directory containing packaged code, e.g. "./BuildPackages".
 * @param packageName   Name of the package. Equivalent to the "Product" for purposes of build versioning or the folder in the bld-pkgs Packages folder.
 * @param branch        Git branch on which the build is being run.
 * @param version       Version of the package to publish.
 */
def publishToBldPkgs(String username, String password, String localPackages, String packageName, String branch, String version)
{
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


/*
 * Check whether boolean value returned from Powershell represents true
 *
 * @param s - string result from powershell script
 * @return -  True if the script result is considered true
 */
def isPowershellResultTrue(s)
{
	return s.trim() == "True"
}

/*
 * Returns true if the job is a nightly job based on naming convention
 */
def isNightly()
{
	return env.JOB_NAME.contains(Constants.NIGHTLY_JOB_NAME)
}

private shouldRunSonar(Boolean enableSonarAnalysis, String branchName)
{
	return (enableSonarAnalysis && branchName == "develop" && !isNightly())
			? "-sonarqube"
			: ""
}

def getSlackChannelName()
{
	if (isNightly() && env.BRANCH_NAME == "develop")
	{
		return "#cd_rip_nightly"
	}
	return "#cd_rip_${env.BRANCH_NAME}"
}

/*
 * Checks whether folder for given Relativity branch exists in build packages
 */
def isRelativityBranchPresent(String branch)
{
	def command = "([System.IO.DirectoryInfo]\"//bld-pkgs/Packages/Relativity/$branch\").Exists"
	return isPowershellResultTrue(powershell(returnStdout: true, script: command))
}

def getLatestVersion(String branch, String type)
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

def checkRelativityArtifacts(String branch, String version, String type)
{
	def command = "([System.IO.FileInfo]\"//bld-pkgs/Packages/Relativity/$branch/$version/MasterPackage/$type $version Relativity.exe\").Exists"
	def result = powershell(returnStdout: true, script: command)
	return isPowershellResultTrue(result)
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
enum TestType {
    integration,
    ui,
    integrationInQuarantine
}

/* Return test name based on the type of the test */
def testStageName(TestType testType)
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
def testCmdOptions(TestType testType)
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
def configureNunitTests(sut)
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
def exceptQuarantinedTestFilter()
{
	return "cat != $Constants.QUARANTINED_TESTS_CATEGORY"
}

/*
 * Returns filter for NUnit
 */
def withQuarantinedTestFilter()
{
	return "cat == $QUARANTINED_TESTS_CATEGORY"
}

/*
 * Get NUnit filter for particular test based also on the pipeline type - whether it is nightly or not
 * @param - params - the params object in the pipeline
 */
def getTestsFilter(TestType testType, params)
{
	def paramsTestsFilter = isNightly() ? params.nightlyTestsFilter : params.testsFilter
	return isQuarantine(testType)
		? unionTestFilters(paramsTestsFilter, withQuarantinedTestFilter())
		: unionTestFilters(paramsTestsFilter, exceptQuarantinedTestFilter())
}

/* 
 * Helper function for running specific type of test
 * @param sut - sut returned from ScvmmInstance.getServerFromPool() 
 * @param - params - the params object in the pipeline
 */
def runTests(TestType testType, sut, params)
{
	configureNunitTests(sut)
	def cmdOptions = testCmdOptions(testType)
    def currentFilter = getTestsFilter(testType, params)
    def result = powershell returnStatus: true, script: "./build.ps1 -ci -sk $cmdOptions \"\"\"$currentFilter\"\"\""
	return result
}

/*
 * @param sut - sut returned from ScvmmInstance.getServerFromPool() 
 */
def runTestsAndSetBuildResult(TestType testType, Boolean skipTests, sut) 
{ 
	def stageName = jenkinsTestingHelpers.testStageName(testType)

    if (skipTests)
	{
		echo "$stageName are going to be skipped."
		return
	}

    def result = runTests(testType, sut) 
    if (result != 0) 
    { 
        echo "$stageName FAILED with status: $result"
		currentBuild.result = "FAILED"
    } 
    echo "$stageName OK" 
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

/*
 * @param - params - the params object in the pipeline
 */
def runIntegrationTests(params)
{
    runTestsAndSetBuildResult(TestType.integration, params.skipIntegrationTests)
}

/*
 * @param - params - the params object in the pipeline
 */
def runUiTests(params)
{
    runTestsAndSetBuildResult(TestType.ui, params.skipUITests)
}

/*
 * @param - params - the params object in the pipeline
 */
def runIntegrationTestsInQuarantine(params)
{
	if(params.skipIntegrationTests)
	{
		return
	}
    runTests(TestType.integrationInQuarantine, params)
}

def getTestsStatistic(String prop)
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


return this