<#  
.SYNOPSIS  
    Finds JIRA's active sprint ID for specified board name.

.PARAMETER Credential
    Domain user's credentials

.PARAMETER BoardName
    JIRA's board name
#>
  
function Find-ActiveSprintId
{
    [CmdletBinding()]
    Param(  
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $BoardName 
    )
    Import-Module -Name $PSScriptRoot\..\Import-Config 
    Import-Module -Name $PSScriptRoot\..\Import-Utils
    Import-Module -Name $PSScriptRoot\Find-Board
 
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"  
    
    $board = Find-Board -Credential $Credential -BoardName $BoardName
    $boardId = $board.id

    Write-Host $boardId

    $getActiveSprintJiraUri = "$JiraApiUri/agile/1.0/board/$boardId/sprint?state=active"
  
    Write-Host $getActiveSprintJiraUri  
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  	 
        $response = Invoke-RestMethod -Uri $getActiveSprintJiraUri -Method GET -Headers $headers -UseBasicParsing
    }  
    catch 
    {  
        Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
        Write-Error "$($MyInvocation.MyCommand.Name) failed" -ErrorAction Stop
    }  
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $sprintId = $response.values | Where-Object { $_.originBoardId -eq $boardId } | Select-Object -ExpandProperty id

    if($sprintId.Length -eq 0)
    {
        Write-Error "$($MyInvocation.MyCommand.Name) failed with active sprint not found" -ErrorAction Stop
    }

    if($sprintId.Length -gt 1)
    {
        Write-Error "$($MyInvocation.MyCommand.Name) failed with multiple active sprints found" -ErrorAction Stop
    }

    $sprintId
}

Export-ModuleMember -Function Find-ActiveSprintId