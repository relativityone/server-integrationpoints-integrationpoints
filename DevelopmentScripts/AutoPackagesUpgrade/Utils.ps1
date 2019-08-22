function ConvertTo-Base64($string) { 
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($string); 
    $encoded = [System.Convert]::ToBase64String($bytes); 
    return $encoded; 
} 
 
function ConvertFrom-Base64($string) { 
    $bytes = [System.Convert]::FromBase64String($string); 
    $decoded = [System.Text.Encoding]::UTF8.GetString($bytes); 
    return $decoded; 
}

function ConvertToPlainPassword($securedPassword) { 
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securedPassword)
    $unsecuredPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    return $unsecuredPassword
}

function GetBasicAuthJsonHttpHeaders($Credential)
{
    $password = ConvertToPlainPassword($Credential.Password); 
 
	$b64 = ConvertTo-Base64($Credential.Username + ":" + $password);  
    return @{  
        "Authorization" = "Basic $b64";
        "Content-Type"="application/json"  
    }
}

function MapJenkinsBuildVersionToRelativityPackageVersion($BuildVersion)
{
    $split = $BuildVersion.Split("-")
    $buildType = $split[0]
    $version = $split[1]

    if($buildType -eq "GOLD")
    {
        return $version
    }
    return "$version-DEV"
}

function MapJenkinsBuildVersionToRipPackageVersion($BuildVersion)
{
    $split = $BuildVersion.Split("-")
    $buildType = $split[0]
    $versionSegments = $split[1].Split(".")
    $baseVersion = $versionSegments[0] + "." + $versionSegments[1] + "." + $versionSegments[2]

    if($buildType -eq "GOLD")
    {
        return $baseVersion
    }
    return "$baseVersion-DEV-" + $versionSegments[3]
}

function MapRipPackageVersionToSystemVersion($PackageVersion)
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

    return $systemVersion
}

function MapRelativityPackageVersionToSystemVersion($PackageVersion)
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

    return $systemVersion
}

function GetCurrentPackageVersionInRip($PaketDependenciesAsText, $PackageName)
{
    try 
	{
		$version = @($PaketDependenciesAsText | Where-Object { $_.Contains(" $PackageName ") }).split()[-1]
	}
	catch 
	{
		Write-Error "Retrieving version of $PackageName failed with $($_.Exception.Message)" -ErrorAction Stop
    }
    
    return $version
}

function GetCurrentRelativityVersionInRip($PaketDependenciesAsText)
{
    Write-Host $PaketDependenciesAsText
    return GetCurrentPackageVersionInRip -PaketDependenciesAsText $PaketDependenciesAsText -PackageName Relativity.Data
}

function GetCurrentRipVersionInRelativity($PackagesConfigAsText, $RipPackageRowSegment)
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

    return $ripVersion
 }