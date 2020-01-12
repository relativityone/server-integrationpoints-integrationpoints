@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-indigo-eau"
	//relativityBranch = "develop"
    jobScript = "Trident/Scripts/Nightly.ps1"
    //cron = "0 3 * * *"
}