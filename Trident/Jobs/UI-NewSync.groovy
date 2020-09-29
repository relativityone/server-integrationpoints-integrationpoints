@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-mayapple-latest"
	relativityBranch = "release-12.0-ninebark"
    jobScript = "Trident/Scripts/UI-NewSync.ps1"
    cron = "0 1 * * *"
}