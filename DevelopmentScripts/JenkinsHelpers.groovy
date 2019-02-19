/**
 ** CONSTANTS
 **/
final String NIGHTLY_JOB_NAME = "IntegrationPointsNightly"
final String QUARANTINED_TESTS_CATEGORY = 'InQuarantine'


/**
 * Return the current build version in the TeamCity versioning database & increment
 * for the next build if necessary. Basically a pass-through to the New-TeamCityBuildVersion.ps1
 * script. Examine that script for info about how CI build versioning works.
 *
 * @param packageName Name of the package being versioned. Equivalent to the "Product" for purposes of build versioning or the folder in the bld-pkgs Packages folder.
 * @param buildType   Build type of the current build, e.g. 'DEV', 'GOLD', etc. Affects how the next version is incremented.
 * @return String indicating the current build version, e.g. "10.2.1.3".
 */
def incrementBuildVersion(String packageName, String buildType)
{
	def versionOutput = powershell(returnStdout: true, script: ".\\DevelopmentScripts\\New-TeamCityBuildVersion.ps1 -Product '$packageName' -Project 'Development' -ServerType 'Jenkins' -BuildType '$buildType'")
	versionNumber = versionOutput.tokenize()[0]
	return versionNumber
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
	return env.JOB_NAME.contains(NIGHTLY_JOB_NAME)
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
	return "cat != $QUARANTINED_TESTS_CATEGORY"
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


return this