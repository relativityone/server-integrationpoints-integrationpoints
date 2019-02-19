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

return this