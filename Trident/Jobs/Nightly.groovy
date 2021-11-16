@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd-integrationpoints"
    sutTemplate = "aio-osier-latest"
	relativityBranch = "release-12.1-osier-server"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}