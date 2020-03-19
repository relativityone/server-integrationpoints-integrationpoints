node('role-build-agent')
{
    powershell returnStatus: true, script: './../Scripts/updateSplunkDashboard.ps1'
}