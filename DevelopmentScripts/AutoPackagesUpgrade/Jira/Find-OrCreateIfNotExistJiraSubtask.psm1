<#  
.SYNOPSIS  
    Finds JIRA subtask or creates one if not exists

.PARAMETER Credential
    Domain user's credentials

.PARAMETER Summary
    JIRA's summary

.PARAMETER Project
    JIRA's project code

.PARAMETER ParentIssueKey
    JIRA's parent issue key

.PARAMETER Description
    JIRA's description

.PARAMETER IssueType
    JIRA's issue type

.PARAMETER Label
    JIRA's label name

.PARAMETER Assignee
    JIRA's assignee
#>
  
function Find-OrCreateIfNotExistJiraSubtask
{
    [CmdletBinding()]
    Param(  
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $Summary,

        [Parameter(Mandatory=$True)]  
        [string]
        $Project,

        [Parameter(Mandatory=$True)]
        [string]
        $ParentIssueKey,

        [Parameter(Mandatory=$True)]  
        [string]
        $Description,

        [Parameter(Mandatory=$True)]  
        [string]
        $IssueType,

        [Parameter(Mandatory=$False)]
        [string]
        $Label,

        [Parameter(Mandatory=$False)]
        [string]
        $Assignee
    )  

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    if($Label)
    {
        $Label = '"'+$Label+'"'
    }
   
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"  

    $createJiraUri = "$JiraApiUri/api/2/issue/"
    $getJiraUri = "$JiraApiUri/api/2/search?jql=parent=$ParentIssueKey+AND+labels=$Label&fields=key"
  
    Write-Host $createJiraUri
    Write-Host $getJiraUri

    [String] $body = '{  
        "fields":{  
           "project":{  
              "key":"'+$Project+'"
           },
           "parent":{
            "key":"'+$ParentIssueKey+'"
            },
           "issuetype":{  
              "name":"'+$IssueType+'"
           },
           "summary":"'+$Summary+'",
           "description":"'+$Description+'",
           "priority":{  
              "name":"P3"
           },
           "labels": [
               '+$Label+'
           ],
           "assignee": { "name": "'+$Assignee+'" }
        }
     }'
    
    Write-Host $body  
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  	 
        $response = Invoke-RestMethod -Uri $getJiraUri -Method GET -Headers $headers -UseBasicParsing
        
        if($response.issues -and $response.issues.length -gt 0)
        {
            $response = $response.issues[0]
        }
        else
        {
            Write-Host "Subtask does not exist - attempting to create a new one"
            $response = Invoke-RestMethod -Uri $createJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing
        }    
        $jiraKey = $($response.key)
    }  
    catch 
    {  
        Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)"  
        Write-Error "$($MyInvocation.MyCommand.Name) failed" -ErrorAction Stop
    }  

    Write-Host "End of $($MyInvocation.MyCommand.Name)"  

    $jiraKey  
}

Export-ModuleMember -Function Find-OrCreateIfNotExistJiraSubtask