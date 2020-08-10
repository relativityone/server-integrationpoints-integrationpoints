function Invoke-TestParser
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $True)]
        [String]$Branch
    )
    $testEngineering = ".\.TestEngineering\TestParser"
    if (-not (Test-Path $testEngineering)) {
        New-Item -ItemType Directory -Force -Path $testEngineering
    }

    $UrlToJenkinsJob = $env:BUILD_URL
    $JiraPassword = $env:testtrackerJiraPassword
    $RelativityPassword = $env:testtrackerRelativityPassword
    $NugetPath = ".\buildtools\nuget.exe"
    $TestResults = ".\Artifacts\Logs"
    $LogLocation = ".\Artifacts\Logs\TestParser.log"

    & $NugetPath install TestParser -OutputDirectory $testEngineering
    
    & .\.TestEngineering\TestParser\TestParser*\lib\net462\TestParser.exe `
        "urlToJenkinsJob:$UrlToJenkinsJob" `
        "workspaceId:2697037" `
        "branchName:$Branch" `
        "jiraUsername:svc_jira_jenkins" `
        "jiraPassword:$JiraPassword" `
        "relativityUsername:test.execution.parser@relativity.com" `
        "relativityPassword:$RelativityPassword" `
        "testResults:$TestResults" `
        "instanceURL:https://test-tracker.kcura.corp/Relativity" `
        "logLocation:$LogLocation"`
}