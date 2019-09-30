<#  
.SYNOPSIS  
    Creates new JIRA

.PARAMETER Credential
    Domain user's credentials

.PARAMETER Summary
    JIRA's summary

.PARAMETER Project
    JIRA's project code

.PARAMETER Description
    JIRA's description

.PARAMETER Product
    JIRA's product name

.PARAMETER Feature
    JIRA's feature name

.PARAMETER Team
    JIRA's team name

.PARAMETER IssueType
    JIRA's issue type

.PARAMETER SprintId
    JIRA's sprint ID

.PARAMETER Label
    JIRA's label name

.PARAMETER Assignee
    JIRA's assignee
#>
  
function New-Jira
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
        $Description,

        [Parameter(Mandatory=$True)]  
        [string]
        $Product,

        [Parameter(Mandatory=$True)]  
        [string]
        $Feature,

        [Parameter(Mandatory=$True)]  
        [string]
        $Team,

        [Parameter(Mandatory=$True)]  
        [string]
        $IssueType,

        [Parameter(Mandatory=$False)]
        [string]
        $SprintId,

        [Parameter(Mandatory=$False)]
        [string]
        $Label,

        [Parameter(Mandatory=$False)]
        [string]
        $Assignee
    )  

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils
    Import-Module -Name $PSScriptRoot\Find-ActiveSprintId

    if(!$SprintId)
    {
        $SprintId = Find-ActiveSprintId -Credential $Credential -BoardName $Team
    }

    if($Label)
    {
        $Label = '"'+$Label+'"'
    }
   
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"  

    $createJiraUri = "$JiraApiUri/api/2/issue/"
  
    Write-Host $createJiraUri  

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
    
    Write-Host $body  
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {  	 
        $response = Invoke-RestMethod -Uri $createJiraUri -Method POST -Headers $headers -Body $body -UseBasicParsing      
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

Export-ModuleMember -Function New-Jira