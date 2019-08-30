function ConvertTo-Base64($string) 
{ 
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($string)
    $encoded = [System.Convert]::ToBase64String($bytes)
    $encoded; 
} 
 
function ConvertFrom-Base64($string) 
{ 
    $bytes = [System.Convert]::FromBase64String($string)
    $decoded = [System.Text.Encoding]::UTF8.GetString($bytes);
    $decoded; 
}

function ConvertTo-PlainPassword($securedPassword) 
{ 
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securedPassword)
    $unsecuredPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    $unsecuredPassword
}

function Get-BasicAuthJsonHttpHeaders($Credential)
{
    $password = ConvertTo-PlainPassword($Credential.Password); 
 
	$b64 = ConvertTo-Base64($Credential.Username + ":" + $password);  
    @{  
        "Authorization" = "Basic $b64";
        "Content-Type"="application/json"  
    }
}

function Map-JenkinsBuildVersionToRelativityPackageVersion($BuildVersion)
{
    $split = $BuildVersion.Split("-")
    $buildType = $split[0]
    $version = $split[1]

    if($buildType -eq "GOLD")
    {
        return $version
    }
    "$version-DEV"
}

function Map-JenkinsBuildVersionToRipPackageVersion($BuildVersion)
{
    $split = $BuildVersion.Split("-")
    $buildType = $split[0]
    $versionSegments = $split[1].Split(".")
    $baseVersion = $versionSegments[0] + "." + $versionSegments[1] + "." + $versionSegments[2]

    if($buildType -eq "GOLD")
    {
        return $baseVersion
    }
    "$baseVersion-DEV-" + $versionSegments[3]
}

function Map-RipPackageVersionToSystemVersion($PackageVersion)
{
    $isGoldVersion = !$PackageVersion.Contains("DEV")

    Write-Verbose "Going to map RIP package version $PackageVersion to system version - isGold: $isGoldVersion"
    try
    {
        if($isGoldVersion -eq $true)
        {
            $systemVersion = [System.Version]$PackageVersion
        }
        else
        {
            $versionSegments = $PackageVersion.Split("-")
            $version = $versionSegments[0] + "." + $versionSegments[2]
            $systemVersion = [System.Version]$version
        }
    }
    catch
    {
        Write-Error "Mapping RIP package version failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    $systemVersion
}

function Map-RelativityPackageVersionToSystemVersion($PackageVersion)
{
    $isDevVersion = $PackageVersion.Contains("DEV")
    $isBetaVersion = $PackageVersion.Contains("beta")

    Write-Verbose "Going to map Relativity package version $PackageVersion to system version"
    try
    {
        if($isDevVersion -eq $true)
        {
            $version = $PackageVersion.Replace("-DEV", "")
        }
        elseif($isBetaVersion -eq $true)
        {
            $version = $PackageVersion.Replace("-beta", ".")
        }
        else
        {
            $version = $PackageVersion
        }

        $systemVersion = [System.Version]$version
    }
    catch
    {
        Write-Error "Mapping Relativity package version failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    $systemVersion
}

function Get-CurrentPackageVersionInRip($PaketDependenciesAsText, $PackageName)
{
    try 
	{
		$version = @($PaketDependenciesAsText | Where-Object { $_.Contains(" $PackageName ") }).split()[-1]
	}
	catch 
	{
		Write-Error "Retrieving version of $PackageName failed with $($_.Exception.Message)" -ErrorAction Stop
    }
    
    $version
}

function Get-CurrentRipVersionInRelativity($PackagesConfigAsText, $RipPackageRowSegment)
{
	try 
	{
		$ripPackageRow = $PackagesConfigAsText | Where-Object { $_.Contains($RipPackageRowSegment) }
		$ripVersion = $ripPackageRow | Select-String '(?<=version=")[^"]*' -All | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value
	}
	catch 
	{
		Write-Error "Retrieving old version of RIP failed with $($_.Exception.Message)" -ErrorAction Stop
    }

    $ripVersion
 }

 function Fail-OnAnyErrors($CommandName)
 {
    if (-not $?) 
	{
        Write-Error "Error $CommandName!" -ErrorAction Stop
    }
 }