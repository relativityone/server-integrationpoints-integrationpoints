<#  
.SYNOPSIS  
    Creates new pull request

.PARAMETER Credential
    Domain user's credentials

.PARAMETER FromBranch
    Source branch name

.PARAMETER ToBranch
    Destination branch name 

.PARAMETER Project
    Project code of pull request

.PARAMETER Repository
    Repository code of pull request

.PARAMETER Reviewers
    Reviewers user names separated by semicolon

.PARAMETER Title
    Pull request's title

.PARAMETER Description
    Pull request's description
#>  
  
function New-PullRequest
{
    [CmdletBinding()]
    Param( 
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $FromBranch,

        [Parameter(Mandatory=$True)]  
        [string]
        $ToBranch,

        [Parameter(Mandatory=$True)]  
        [string]
        $Project,

        [Parameter(Mandatory=$True)]  
        [string]
        $Repository,

        [Parameter(Mandatory=$True)]  
        [string]
        $Reviewers,

        [Parameter(Mandatory=$False)]  
        [string]
        $Title,

        [Parameter(Mandatory=$False)]  
        [string]
        $Description 
    )
    Import-Module -Name $PSScriptRoot\..\Import-Config 
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    if(!$Title)
    {  
        $Title = $FromBranch 
    } 
 
    if(!$Description)
    {  
        $Description = "This PR was created automatically by AutoPackagesUpgrade script."  
    }
 
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
      
    $uri = "$BitbucketApiUri/projects/$Project/repos/$Repository/pull-requests" 
      
    Write-Host $uri
       
    $reviewers = ($Reviewers.Split(';') |  ForEach-Object {' 
    { 
        "user": { 
            "name": "'+$_+'" 
        } 
    }'}) -join "," 
 
    Write-Host $reviewers 
 
    [String] $body = '{ 
        "title": "'+$Title+'", 
        "description": "'+$Description.Replace("`r`n", "\n")+'", 
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
    Write-Host $body  
 
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {   
        Invoke-RestMethod -Uri $uri -Method POST -Headers $headers -Body $body -UseBasicParsing 
    }  
    catch 
    {
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name
    }  
     
    Write-Host "End of $($MyInvocation.MyCommand.Name)"
}

Export-ModuleMember -Function New-PullRequest