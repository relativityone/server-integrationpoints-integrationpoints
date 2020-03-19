node('role-build-agent')
{
    try
    {
        powershell returnStatus: true, script: './../../Scripts/updateSplunkDashboard.ps1'
    }
    catch (err)
    {
        currentBuild.result = "FAILED"
    }
    finally
    {

    }    
}