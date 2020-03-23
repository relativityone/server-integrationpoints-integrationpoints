<#
.SYNOPSIS
This script will be used by Trident pipeline to update Splunk dashboard
#>

try 
{
    $OriginalLocation = Get-Location
    Write-Host Current location: $OriginalLocation.Path

    $IssuesFilePath = "../../Source/Relativity.Sync.Dashboards/Relativity.Sync.Dashboards/issues.json"
    $Uri = "https://relativitysyncdashboards.azurewebsites.net/api/Function"

    Write-Host Reading Function Authorizarion Key from environment variables
    Set-Location Env:
    Set-Item -Path Env:FunctionAuthorizationKey -Value "zWL4b1k/EwpPnFIwshMddgYW0G/KApRg1XsAHgRpYHRgnGW4c4GTUg=="
    $FunctionAuthorizationKey = (Get-ChildItem FunctionAuthorizationKey).Value
    Write-Host Function authorization key length: $FunctionAuthorizationKey.Length

    Write-Host Changing current location back to: $OriginalLocation.Path
    Set-Location $OriginalLocation.Path

    Write-Host Issues file path: $IssuesFilePath
    Write-Host Azure Function URL: $Uri

    Write-Host Loading issues file
    $Body = Get-Content $IssuesFilePath
    Write-Host $Body.Length bytes has been read

    Write-Host Sending request
    Invoke-RestMethod $Uri -Method POST -Headers @{'x-functions-key' = $FunctionAuthorizationKey} -ContentType "application/json" -Body $Body    
}
catch
{
    Write-Host Exception: $_.Exception
    Throw
}

