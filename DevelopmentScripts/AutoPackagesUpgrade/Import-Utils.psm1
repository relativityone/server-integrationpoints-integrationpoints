function ConvertTo-Base64($PlainPassword) 
{ 
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($PlainPassword)
    $encoded = [System.Convert]::ToBase64String($bytes)
    $encoded; 
}

function ConvertTo-PlainPassword($SecuredPassword) 
{ 
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecuredPassword)
    $unsecuredPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    $unsecuredPassword
}

function Find-BasicAuthJsonHttpHeaders($Credential)
{
    $password = ConvertTo-PlainPassword -SecuredPassword $Credential.Password 
 
    $b64 = ConvertTo-Base64 -PlainPassword "$($Credential.Username):$password"

    @{  
        "Authorization" = "Basic $b64";
        "Content-Type"="application/json"  
    }
}

function Format-JenkinsBuildVersionToRelativityPackageVersion($BuildVersion)
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

function Format-JenkinsBuildVersionToRipPackageVersion($BuildVersion)
{
    $split = $BuildVersion.Split("-")
    $buildType = $split[0]
    $versionSegments = $split[1].Split(".")
    $baseVersion = $versionSegments[0] + "." + $versionSegments[1] + "." + $versionSegments[2]

    if($buildType -eq "GOLD")
    {
        return $baseVersion
    }

    $minorVersion = "{0:000}" -f [int]$versionSegments[3]

    "$baseVersion-DEV-$minorVersion"
}

function Format-RipPackageVersionToSystemVersion($PackageVersion)
{
    $isGoldVersion = !$PackageVersion.Contains("DEV")

    Write-Host "Going to map RIP package version $PackageVersion to system version - isGold: $isGoldVersion"
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

function Format-RelativityPackageVersionToSystemVersion($PackageVersion)
{
    $isDevVersion = $PackageVersion.Contains("DEV")
    $isBetaVersion = $PackageVersion.Contains("beta")

    Write-Host "Going to map Relativity package version $PackageVersion to system version"
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

function Find-CurrentPackageVersionInRip($PaketDependenciesAsText, $PackageName)
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

function Find-CurrentRipVersionInRelativity($PackagesConfigAsText, $RipPackageRowSegment)
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

 function Exit-AndLogHttpError($CmdName)
 {
    Write-Warning "Remote Server Response: $($_.Exception.Message)"  
    Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
    Write-Error "$($CmdName) failed" -ErrorAction Stop
 }

 function Exit-OnAnyErrors($CommandName)
 {
    if (-not $?) 
    {
        Write-Error "Error $CommandName!" -ErrorAction Stop
    }
 }

 Export-ModuleMember -Function *