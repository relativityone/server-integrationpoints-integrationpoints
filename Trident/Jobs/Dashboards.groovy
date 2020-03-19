node('role-build-agent')
{
    try
    {
        powershell "Write-Host (Get-Item -Path '.\').FullName"
        powershell returnStatus: true, script: './../../Scripts/updateSplunkDashboard.ps1'
    }
    catch (err)
    {
        echo err.toString()
        currentBuild.result = "FAILED"
    }
    finally
    {

    }    
}