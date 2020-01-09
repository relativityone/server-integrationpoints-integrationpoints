@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-indigo-eau"
	//relativityBranch = "develop"
    jobScript = "Trident/Scripts/UI-Sync.ps1 -Toggle Off"
    //cron = "0 3 * * *"
}