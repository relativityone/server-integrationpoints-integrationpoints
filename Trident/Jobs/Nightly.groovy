@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "ci-rip"
    sutTemplate = "aio-whitesedge-latest"
	//relativityBranch = "develop"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}