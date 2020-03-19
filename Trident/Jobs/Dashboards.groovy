node('role-build-agent')
{
    try
    {
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