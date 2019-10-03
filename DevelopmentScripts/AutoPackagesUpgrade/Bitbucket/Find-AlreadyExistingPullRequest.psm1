<#  
.SYNOPSIS  
    Finds existing pull request filtered by title

.PARAMETER Credential
    Domain user's credentials

.PARAMETER Title
    Title of pull request

.PARAMETER Author
    Author of pull request 

.PARAMETER Project
    Project code of pull request

.PARAMETER Repository
    Repository code of pull request
#>  
  
function Find-AlreadyExistingPullRequest 
{
    [CmdletBinding()]
    Param( 
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]  
        [string]
        $Title,

        [Parameter(Mandatory=$True)]  
        [string]
        $Author,

        [Parameter(Mandatory=$True)]  
        [string]
        $Project,

        [Parameter(Mandatory=$True)]  
        [string]
        $Repository
    )
    
    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    Set-Variable pullRequestsPageLimit -option Constant -value 50

    $uri = "$BitbucketApiUri/projects/$Project/repos/$Repository/pull-requests?username.1=$Author&role.1=AUTHOR&order=NEWEST&state=ALL" 

    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential

    try 
    {
        $pullRequests = @()
        $pageIndex = 0
        do
        {
            $pullRequestUri = "$uri&start=$pageIndex"
            $response = Invoke-RestMethod -Uri $pullRequestUri -Method GET -Headers $headers -UseBasicParsing 

            $pullRequests = $response.values + $pullRequests
            $pageIndex += $response.limit
        }while((-n $response.isLastPage) -and $pageIndex -lt $pullRequestsPageLimit)
    }
    catch 
    {
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name
    }

    $pullRequestsFilteredByTitle = $pullRequests | Select-Object -ExpandProperty title | Where-Object { $_.Contains($Title) }

    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $pullRequestsFilteredByTitle
}

Export-ModuleMember -Function Find-AlreadyExistingPullRequest