<#
.SYNOPSIS

.DESCRIPTION

.EXAMPLE
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True)]  
	[pscredential]$Credential,
	[Parameter(Mandatory=$True)]
    [string]$Category,
    [Parameter(Mandatory=$False)]
	[string]$SubCategory,
	[Parameter(Mandatory=$True)]
    [string]$Pipeline,
    [Parameter(Mandatory=$True)]
    [string]$Branch
)
Begin
{
    . ".\Config.ps1"  
	. ".\Utils.ps1"  
}
Process
{
    Write-Verbose "Beginning of GetLastSuccessfulBuildVersion.ps1"
	
    $getJobUri = "$JenkinsApiUri/job/$Category"
    
    if($SubCategory)
    {
        $getJobUri += "/job/$SubCategory"
    }

    $getJobUri += "/job/$Pipeline/job/$Branch/api/json"
  
	Write-Verbose $getJobUri
  
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {
        $response = Invoke-RestMethod -Uri $getJobUri -Method GET -Headers $headers -UseBasicParsing
        $lastSuccessfulBuildUri = $response.lastSuccessfulBuild.url
        $build = Invoke-RestMethod -Uri "$lastSuccessfulBuildUri/api/json" -Method GET -Headers $headers -UseBasicParsing
        $version = $build.displayName
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "GetLastSuccessfulBuildVersion failed" -ErrorAction Stop 
	}  
    Write-Verbose "End of GetLastSuccessfulBuildVersion.ps1"

    return $version
}

