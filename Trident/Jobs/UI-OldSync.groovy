@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-ninebark-ea"
	relativityBranch = "release-12.0-ninebark-ea"
    jobScript = "Trident/Scripts/UI-OldSync.ps1"
    cron = "0 1 * * *"
}