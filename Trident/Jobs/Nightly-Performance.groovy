@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "sync_trident"
    sutTemplate = "aio-ninebark-ea"
	relativityBranch = "develop"
    jobScript = "Trident/Scripts/nightly-performance.ps1"
    cron = "0 3 * * *"
}