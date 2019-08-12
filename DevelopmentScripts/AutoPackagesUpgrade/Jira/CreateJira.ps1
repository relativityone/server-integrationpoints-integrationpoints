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
	[string]$Summary, 
	[Parameter(Mandatory=$True)]  
	[string]$Project,  
	[Parameter(Mandatory=$True)]  
	[string]$Description,  
	[Parameter(Mandatory=$True)]  
	[string]$Product,  
    [Parameter(Mandatory=$True)]  
	[string]$Feature,  
	[Parameter(Mandatory=$True)]  
    [string]$Team,  
	[Parameter(Mandatory=$True)]  
	[string]$IssueType,
	[Parameter(Mandatory=$False)]
	[string]$SprintId,
	[Parameter(Mandatory=$False)]
	[string]$Label,
	[Parameter(Mandatory=$False)]
	[string]$Assignee
)  
Begin
{
	. ".\Config.ps1"  
	. ".\Utils.ps1" 

	if(!$SprintId)
	{
		$SprintId = .\Jira\GetActiveSprintId.ps1 -Credential $Credential -BoardName $Team
	}

	if($Label)
	{
		$Label = '"'+$Label+'"'
	}
}
Process  
{    
	Write-Verbose "Beginning of CreateJira.ps1"  

	$createJiraUri = "$JiraApiUri/api/2/issue/"
  
	Write-Verbose $createJiraUri  

	[String] $body = '{  
		"fields":{  
		   "project":{  
			  "key":"'+$Project+'"
		   },
		   "issuetype":{  
			  "name":"'+$IssueType+'"
		   },
		   "summary":"'+$Summary+'",
		   "description":"'+$Description+'",
		   "priority":{  
			  "name":"P3"
		   },
		   "customfield_11312":[  
			  "'+$Team+'"
		   ],
		   "customfield_11311":[  
			  "'+$Product+'"
		   ],
		   "customfield_11313":[  
			  "'+$Feature+'"
		   ],
		   "customfield_10005":'+$SprintId+',
		   "labels": [
			   '+$Label+'
		   ],
		   "assignee": { "name": "'+$Assignee+'" }
		}
	 }'
	
	Write-Verbose $body  
  
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
	try 
	{  	 
		$response = Invoke-RestMethod -Uri $createJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing      
		$jiraKey = $($response.key)  
	}  
	catch 
	{  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
		Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
		Write-Error "Creating Jira failed" -ErrorAction Stop
	}  
	Write-Verbose "End of CreateJira.ps1"  
	return $jiraKey  
}