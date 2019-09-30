<#  
.SYNOPSIS  
    Updates JIRA status

.PARAMETER Credential
    Domain user's credentials

.PARAMETER JiraNumber
    JIRA's issue key

.PARAMETER StatusName
    JIRA's status name
#> 
  
function Update-JiraStatus
{
    [CmdletBinding()]
    Param(  
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $JiraNumber,

        [Parameter(Mandatory=$True)]  
        [string]
        $StatusName
    )  

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils
  
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    $changeStatusJiraUri = "$JiraApiUri/api/2/issue/$JiraNumber/transitions"
    $statusJiraUri = "$JiraApiUri/api/2/issue/$JiraNumber/transitions?expand=transitions.fields"
  
    Write-Host $statusJiraUri
    
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  
        $response = Invoke-RestMethod -Uri $statusJiraUri -Method GET -Headers $headers -UseBasicParsing  
        $transitionId = $response.transitions | Where-Object name -eq $StatusName | Select-Object -ExpandProperty id
        Write-Host "ID: $($transitionId)"  

        [String] $body = '{  
            "transition": {
                "id": "'+$transitionId+'"
            }
        }'
        
        Write-Host $body 

        $response = Invoke-RestMethod -Uri $changeStatusJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing  
    }  
    catch 
    {  
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name
    }  
    
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function Update-JiraStatus