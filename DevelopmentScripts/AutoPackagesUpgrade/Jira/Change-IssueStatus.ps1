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
	[string]$JiraNumber, 
	[Parameter(Mandatory=$True)]  
	[string]$StatusName
)  
Begin
{
	. ".\Config.ps1"  
	. ".\Utils.ps1"  
} 
Process  
{  
	Write-Verbose "Beginning of Change-IssueStatus.ps1"

    $changeStatusJiraUri = "$JiraApiUri/api/2/issue/$JiraNumber/transitions"
	$statusJiraUri = "$JiraApiUri/api/2/issue/$JiraNumber/transitions?expand=transitions.fields"
  
    Write-Verbose $statusJiraUri
    
	$headers = Get-BasicAuthJsonHttpHeaders -Credential $Credential
	try 
	{  
        $response = Invoke-RestMethod -Uri $statusJiraUri -Method GET -Headers $headers -UseBasicParsing  
        $transitionId = $response.transitions | Where-Object name -eq $StatusName | Select-Object -ExpandProperty id
        Write-Verbose "ID: $($transitionId)"  

        [String] $body = '{  
            "transition": {
                "id": "'+$transitionId+'"
            }
        }'
        
        Write-Verbose $body 

		$response = Invoke-RestMethod -Uri $changeStatusJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing  
	}  
	catch 
	{  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
		Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
		Write-Error "Change-IssueStatus failed" -ErrorAction Stop
    }  
    
	Write-Verbose "End of Change-IssueStatus.ps1"
}