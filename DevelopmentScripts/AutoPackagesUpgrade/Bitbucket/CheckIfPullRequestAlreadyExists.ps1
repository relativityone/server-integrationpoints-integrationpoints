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
    [string]$Title,
    [Parameter(Mandatory=$True)]  
    [string]$Author,
    [Parameter(Mandatory=$True)]  
    [string]$Project,
    [Parameter(Mandatory=$True)]  
    [string]$Repository
)  
Begin
{
    . ".\Config.ps1" 
	. ".\Utils.ps1"  
}
Process  
{  
	Write-Verbose "Beginning of CheckIfPullRequestAlreadyExists.ps1"  
      
    $uri = "$BitbucketApiUri/projects/$Project/repos/$Repository/pull-requests?username.1=$Author&role.1=AUTHOR&order=NEWEST&state=ALL"

	Write-Verbose $uri  
 
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential

    try 
    {  
        $pullRequests = @()
        $pageLimit = 0
        for($pageIndex = 0; $pageIndex -lt 50; $pageIndex += $pageLimit)
        { 
            $pullRequestUri = "$uri&start=$pageIndex"
		    $response = Invoke-RestMethod -Uri $pullRequestUri -Method GET -Headers $headers -UseBasicParsing 
        
            $pullRequests = $response.values + $pullRequests
            $pageLimit = $response.limit
            if($response.isLastPage)
            {
                break
            }
        }
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
		Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
		Write-Error "CheckIfPullRequestAlreadyExists failed" -ErrorAction Stop
    } 
    
    $exists = ($pullRequests | Select-Object -ExpandProperty title | Where-Object { $_.Contains($Title) }).Count -gt 0
     
    Write-Verbose "End of CheckIfPullRequestAlreadyExists.ps1" 
    
    return $exists
}