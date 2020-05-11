@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "sync_trident"
    sutTemplate = "aio-blazingstar-eau"
	relativityBranch = "REL-427630-Kepler250"
    jobScript = "Trident/Scripts/nightly-job.ps1"
    cron = "0 3 * * *"
}