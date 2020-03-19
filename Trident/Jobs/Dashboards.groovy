node('RelativityBuild')
{
    powershell returnStatus: true, script: './../Scripts/updateSplunkDashboard.ps1'
}