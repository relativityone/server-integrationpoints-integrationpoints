<#  
.SYNOPSIS  
    Finds last successful build version for specified Jenkins pipeline

.PARAMETER Credential
    Domain user's credentials

.PARAMETER JiraKey
    Related jira key

.PARAMETER Category
    Jenkin's build category name

.PARAMETER Pipeline
    Jenkin's build pipeline name

.PARAMETER Branch
    Jenkin's build branch name
#>

function Find-LastSuccessfulBuildVersion
{
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True)]  
        [pscredential]
        $Credential,

        [Parameter(Mandatory=$True)]
        [string]
        $Category,

        [Parameter(Mandatory=$False)]
        [string]
        $SubCategory,

        [Parameter(Mandatory=$True)]
        [string]
        $Pipeline,

        [Parameter(Mandatory=$True)]
        [string]
        $Branch
    )

    Import-Module -Name $PSScriptRoot\..\Import-Config
    Import-Module -Name $PSScriptRoot\..\Import-Utils

    Write-Host "Beginning of $($MyInvocation.MyCommand.Name)"
    
    $getJobUri = "$JenkinsApiUri/job/$Category"
    
    if($SubCategory)
    {
        $getJobUri += "/job/$SubCategory"
    }

    $getJobUri += "/job/$Pipeline/job/$Branch/api/json"
  
    Write-Host $getJobUri
  
    $headers = Find-BasicAuthJsonHttpHeaders -Credential $Credential
    try 
    {
        $response = Invoke-RestMethod -Uri $getJobUri -Method GET -Headers $headers -UseBasicParsing
        $lastSuccessfulBuildUri = $response.lastSuccessfulBuild.url

        $build = Invoke-RestMethod -Uri "$lastSuccessfulBuildUri/api/json" -Method GET -Headers $headers -UseBasicParsing
        $version = $build.displayName
    }  
    catch 
    {  
        Exit-AndLogHttpError -CmdName $MyInvocation.MyCommand.Name 
    }  
    Write-Host "End of $($MyInvocation.MyCommand.Name)"

    $version
}

Export-ModuleMember -Function Find-LastSuccessfulBuildVersion