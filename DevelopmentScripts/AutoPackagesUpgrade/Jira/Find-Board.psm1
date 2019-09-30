<#  
.SYNOPSIS  
    Finds JIRA's board object by specified name

.PARAMETER Credential
    Domain user's credentials

.PARAMETER BoardName
    JIRA's board name
#>
  
function Find-Board
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

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"  
    
    $getBoardsJiraUri = "$JiraApiUri/agile/1.0/board?name=$BoardName"
  
    Write-Host $getBoardsJiraUri  
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  
        $response = Invoke-RestMethod -Uri $getBoardsJiraUri -Method GET -Headers $headers -UseBasicParsing
    }  
    catch 
    {  
        Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "$($MyInvocation.MyCommand.Name) failed" -ErrorAction Stop 
    }  

    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $response.values
}

Export-ModuleMember -Function Find-Board