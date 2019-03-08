/*
Required libraries:

library 'PipelineTools@RMT-9.3.2'
library 'SCVMMHelpers@3.2.0'

 */

class PipelineState
{
    final script
    final env
    final params
    final String sessionId = System.currentTimeMillis().toString()

    String eventHash
    def relativityBuildVersion = ""
    def relativityBuildType = ""
    def relativityBranch = ""

    def scvmmInstance
    def sut

    PipelineState(script, env, params)
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
pipelineState = null

def initializePipeline(script, env, params)
{
    pipelineState = new PipelineState(script, env, params)
    pipelineState.relativityBranch = params.relativityBranch ?: env.BRANCH_NAME
    return pipelineState.sessionId
}

/*
 * @param relativityBranchFallback - the default fallback branch. develop by default, but should be set to release branch name on release branches!!!
 */
def prepareSut(String relativityBranchFallback)
{
    timeout(time: 90, unit: 'MINUTES')
    {
        pipelineState.getServerFromPool()
        def sut = pipelineState.sut

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

        def relativityBuildVersion = pipelineState.relativityBuildVersion
        def relativityBranch = pipelineState.relativityBranch
        def relativityBuildType = pipelineState.relativityBuildType

        def sessionId = pipelineState.sessionId
        def script = pipelineState.script
        def eventHash = pipelineState.eventHash

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
                    pipelineState.relativityBuildVersion = relativityBuildVersion
                    pipelineState.relativityBranch = relativityBranch
                    pipelineState.relativityBuildType = relativityBuildType

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
            },
        )
    }
}

def cleanupVMs()
{
    try
    {
        timeout(time: 20, unit: 'MINUTES')
        {
            if(pipelineState.sut?.name)
            {
                // If we don't have a result, we didn't get to a test because somthing failed out earlier.
                // If the result is FAILURE, a test failed.
                if (!currentBuild.result || currentBuild.result == "FAILURE")
                {
                    try
                    {
                        timeout(time: 1, unit: 'MINUTES')
                        {
                            //it returns username who submitted the request to save vms
                            user = input(
                                message: 'Save the VMs?', 
                                ok: 'Save', 
                                submitter: 'JNK-Basic', 
                                submitterParameter: 'submitter'
                            )
                        }
                        pipelineState.scvmmInstance.saveVMs(user)
                    }
                    // Exception is thrown if you click abort or let it time out
                    catch(err)
                    {
                        echo "Deleting VMs..."
                        pipelineState.scvmmInstance.deleteVMs()
                    }
                }
            }
            deleteNodes(pipelineState.script, pipelineState.sessionId)
        }
    }
    catch (err)
    {
        echo "Cleanup VMs FAILED."
    }
}

/*****************
 **** PRIVATE ****
/*****************

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

/*
 * Checks whether folder for given Relativity branch exists in build packages
 */
private isRelativityBranchPresent(String branch)
{
	def command = "([System.IO.DirectoryInfo]\"//bld-pkgs/Packages/Relativity/$branch\").Exists"
	return isPowershellResultTrue(powershell(returnStdout: true, script: command))
}

private checkRelativityArtifacts(String branch, String version, String type)
{
	def command = "([System.IO.FileInfo]\"//bld-pkgs/Packages/Relativity/$branch/$version/MasterPackage/$type $version Relativity.exe\").Exists"
	def result = powershell(returnStdout: true, script: command)
	return isPowershellResultTrue(result)
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

return this
