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
	Write-Verbose "Beginning of Get-ActiveSprintId.ps1"  
    
    $board = .\Jira\Get-Board.ps1 -Credential $Credential -BoardName $BoardName
    $boardId = $board.id

    Write-Verbose $boardId

	$getActiveSprintJiraUri = "$JiraApiUri/agile/1.0/board/$boardId/sprint?state=active"
  
	Write-Verbose $getActiveSprintJiraUri  
  
	$headers = Get-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  	 
        $response = Invoke-RestMethod -Uri $getActiveSprintJiraUri -Method GET -Headers $headers -UseBasicParsing
	}  
    catch 
    {  
		Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
        Write-Error "Get-ActiveSprintId failed" -ErrorAction Stop
    }  
    
    Write-Verbose "End of Get-ActiveSprintId.ps1"

    $sprintId = $response.values | Where-Object { $_.originBoardId -eq $boardId } | Select-Object -ExpandProperty id

    if($sprintId.Length -eq 0)
    {
        Write-Error "Get-ActiveSprintId failed with active sprint not found" -ErrorAction Stop
    }

    if($sprintId.Length -gt 1)
    {
        Write-Error "Get-ActiveSprintId failed with multiple active sprints found" -ErrorAction Stop
    }

    $sprintId
}