@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "sync_trident"
    sutTemplate = "LanceleafAA1"
	relativityBranch = "develop"
    jobScript = "Trident/Scripts/nightly-job.ps1"
    cron = "0 3 * * *"
}