stage ('UpdateSplunkDashboard')
{
    powershell returnStatus: true, script: './../Scripts/updateSplunkDashboard.ps1'
}