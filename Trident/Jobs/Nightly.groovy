@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-ninebark-eau"
	relativityBranch = "release-12.0-ninebark-eau"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}