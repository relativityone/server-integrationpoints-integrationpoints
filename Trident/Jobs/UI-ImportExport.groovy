@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-ninebark-latest"
	relativityBranch = "develop"
    jobScript = "Trident/Scripts/UI-ImportExport.ps1"
    cron = "0 1 * * *"
}