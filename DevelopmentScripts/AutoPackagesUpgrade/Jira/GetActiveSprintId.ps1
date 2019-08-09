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
	[string]$BoardName 
)  
Begin
{
    . ".\Config.ps1"  
	. ".\Utils.ps1" 
}
Process  
{  
	Write-Verbose "Beginning of GetActiveSprintId.ps1"  
    
    $board = .\Jira\GetBoard.ps1 -Credential $Credential -BoardName $BoardName
    $boardId = $board.id

    Write-Verbose $boardId

	$getActiveSprintJiraUri = "$JiraApiUri/agile/1.0/board/$boardId/sprint?state=active"
  
	Write-Verbose $getActiveSprintJiraUri  
  
	$headers = GetBasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  	 
        $response = Invoke-RestMethod -Uri $getActiveSprintJiraUri -Method GET -Headers $headers -UseBasicParsing
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
        Write-Error "GetActiveSprintId failed" -ErrorAction Stop
	}  
    Write-Verbose "End of GetActiveSprintId.ps1"
    return $response.values.id
}