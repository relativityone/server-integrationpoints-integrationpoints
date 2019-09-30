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

    $uri = "$BitbucketApiUri/projects/$Project/repos/$Repository/pull-requests?username.1=$Author&role.1=AUTHOR&order=NEWEST&state=ALL" 

    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential

    try 
    {
        $pullRequests = @()
        $pageLimit = 0
        for($pageIndex = 0; $pageIndex -lt 50; $pageIndex += $pageLimit)
        { 
            $pullRequestUri = "$uri&start=$pageIndex"
            $response = Invoke-RestMethod -Uri $pullRequestUri -Method GET -Headers $headers -UseBasicParsing 

            $pullRequests = $response.values + $pullRequests
            $pageLimit = $response.limit
            if($response.isLastPage)
            {
                break
            }
        }
    }
    catch 
    {  
        Write-Warning "Remote Server Response: $($_.Exception.Message)"  
        Write-Output "Status Code: $($_.Exception.Response.StatusCode)" 
        Write-Error "$($MyInvocation.MyCommand.Name) failed" -ErrorAction Stop
    }

    $pullRequestsFilteredByTitle = $pullRequests | Select-Object -ExpandProperty title | Where-Object { $_.Contains($Title) }

    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $pullRequestsFilteredByTitle
}

Export-ModuleMember -Function Find-AlreadyExistingPullRequest