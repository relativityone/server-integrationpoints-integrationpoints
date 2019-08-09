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
	[string]$FromBranch,  
    [Parameter(Mandatory=$True)]  
	[string]$ToBranch, 
	[Parameter(Mandatory=$True)]  
	[string]$Project,  
	[Parameter(Mandatory=$True)]  
	[string]$Repository, 
	[Parameter(Mandatory=$True)]  
	[string]$Reviewers, 
	[Parameter(Mandatory=$False)]  
	[string]$Title, 
	[Parameter(Mandatory=$False)]  
	[string]$Description 
)  
Begin
{
	. ".\Config.ps1" 
	. ".\Utils.ps1"

	if(!$Title)
	{  
		$Title = $FromBranch 
	} 
 
	if(!$Description)
	{  
		$Description = "This PR was created automatically by AutoPackagesUpgrade script."  
	}
}
Process  
{  
	Write-Verbose "Beginning of CreatePullRequest.ps1"  
	  
	$uri = "$BitbucketApiUri/projects/$Project/repos/$Repository/pull-requests" 
	  
	Write-Verbose $uri
	   
	$reviewers = ($Reviewers.Split(';') |  ForEach-Object {' 
	{ 
		"user": { 
			"name": "'+$_+'" 
		} 
	}'}) -join "," 
 
	Write-Verbose $reviewers 
 
	[String] $body = '{ 
		"title": "'+$Title+'", 
		"description": "'+$Description+'", 
		"state": "OPEN", 
		"open": true, 
		"closed": false, 
		"fromRef": { 
			"id": "refs/heads/'+$FromBranch+'", 
			"repository": { 
				"slug": "'+$Repository+'", 
				"name": null, 
				"project": { 
					"key": "'+$Project+'" 
				} 
			} 
		}, 
		"toRef": { 
			"id": "refs/heads/'+$ToBranch+'", 
			"repository": { 
				"slug": "'+$Repository+'", 
				"name": null, 
				"project": { 
					"key": "'+$Project+'" 
				} 
			} 
		}, 
		"locked": false, 
		"reviewers": [ 
			'+$reviewers+' 
		], 
		"links": { 
			"self": [ 
				null 
			] 
		} 
	}';  
	Write-Verbose $body  
 
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
	try 
	{   
		Invoke-RestMethod -Uri $uri -Method POST -Headers $headers -Body $body -UseBasicParsing 
	}  
	catch 
	{  
		$_.Exception.Response
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
		Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
		Write-Error "Creating pull request failed" -ErrorAction Stop
	}  
	 
	Write-Verbose "End of CreatePullRequest.ps1" 
}