@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "LanceleafAA1"
	relativityBranch = "develop"
    jobScript = "Trident/Scripts/UI-OldSync.ps1"
    cron = "0 1 * * *"
}