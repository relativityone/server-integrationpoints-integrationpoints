@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd-integrationpoints"
    sutTemplate = "aio-prairiesmoke-0"
	relativityBranch = "develop"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}