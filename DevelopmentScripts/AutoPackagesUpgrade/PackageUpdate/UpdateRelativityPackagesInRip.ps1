<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
	[Parameter(Mandatory=$True)]
	[string]$ToVersion,
	[Parameter(Mandatory=$True)]
	[string]$RipSourceCodePath,
	[Parameter(Mandatory=$True)]
	[switch]$WithLatestkCura,
	[Parameter(Mandatory=$True)]
	[switch]$WithLatestRelativityApi,
	[Parameter(Mandatory=$True)]
	[switch]$WithLatestRelativityDataExchange
)
Begin
{
    . ".\Config.ps1" 
	. ".\Utils.ps1"	

	function UpdatePackageToLatestReleaseVersion($PaketDependenciesAsText, $PackageName)
	{
		$oldVersion = GetCurrentPackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName $PackageName
		$newVersion = .\Proget\GetLatestPackageVersion.ps1 -PackageName $PackageName
		return ReplacePackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName $PackageName -OldVersion $oldVersion -NewVersion $NewVersion 
	}

	function UpdateRelativityPackages($PaketDependenciesAsText, $OldVersion, $NewVersion)
	{
		Write-Host "Updating Relativity packages in paket.dependencies from $oldVersion to $NewVersion..." -ForegroundColor Cyan

		return ReplacePackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName "" -OldVersion $OldVersion -NewVersion $NewVersion
	}

	function ReplacePackageVersionInRip($PaketDependenciesAsText, $PackageName, $OldVersion, $NewVersion)
	{
		try
		{
			return $PaketDependenciesAsText.replace("$PackageName $oldVersion", "$PackageName $NewVersion")
		}
		catch
		{
			Write-Error "Replacing Relativity versions in paket.dependencies from $oldVersion to $NewVersion failed with $($_.Exception.Message)" -ErrorAction Stop
		}
	}
}
Process
{
	Write-Verbose "Beginning of UpdateRelativityPackagesInRip.ps1"

	$paketDependenciesPath = Join-Path "$RipSourceCodePath" "paket.dependencies"
	$paketLockPath = Join-Path "$RipSourceCodePath" "paket.lock"
	$csProjsPath = Join-Path "$RipSourceCodePath" "*.csproj"
	$paketExePath = Join-Path "$RipSourceCodePath" ".paket\paket.exe"
	
	$packages = Get-Content -Path $paketDependenciesPath
	$oldVersion = GetCurrentRelativityVersionInRip -PaketDependenciesAsText $packages

	$packages = UpdateRelativityPackages -PaketDependenciesAsText $packages -OldVersion $oldVersion -NewVersion $ToVersion
	if($WithLatestkCura)
	{
		$packages = UpdatePackageToLatestReleaseVersion -PaketDependenciesAsText $packages -PackageName "kCura"
		$packages = UpdatePackageToLatestReleaseVersion -PaketDependenciesAsText $packages -PackageName "kCura.Agent"
	}
	if($WithLatestRelativityApi)
	{
		$packages = UpdatePackageToLatestReleaseVersion -PaketDependenciesAsText $packages -PackageName "Relativity.API"
	}
	if($WithLatestRelativityDataExchange)
	{
		$packages = UpdatePackageToLatestReleaseVersion -PaketDependenciesAsText $packages -PackageName "Relativity.DataExchange.Client.SDK"
	}

	try 
	{
		Set-Content -Path $paketDependenciesPath -Value $packages
	}
	catch 
	{
		Write-Error "Updating content of paket.dependencies failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	Push-Location $RipSourceCodePath

	Write-Verbose "Starting paket update"

	try 
	{
		$paketLogs = & $paketExePath update
	}
	catch
	{
		Write-Error "Updating packages with Paket failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	Write-Verbose "End of paket update"

	Write-Host $paketLogs

	Pop-Location

	try 
	{
		git -C $RipSourceCodePath add $paketDependenciesPath
		git -C $RipSourceCodePath add $paketLockPath
		git -C $RipSourceCodePath add $csProjsPath
	}
	catch
	{
		Write-Error "Staging paket files failed with $($_.Exception.Message)" -ErrorAction Stop
	}

	Write-Host "Relativity version in RIP updated successfully in paket.dependencies from $oldVersion to $ToVersion" -ForegroundColor Cyan
	
	Write-Verbose "End of UpdateRelativityPackagesInRip.ps1"
	
	if($paketLogs.Length -le $LogCharsLimit)
	{
		return $paketLogs
	}

	return $paketLogs.Substring($paketLogs.Length - $LogCharsLimit)
}