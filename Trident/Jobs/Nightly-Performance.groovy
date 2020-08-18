@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "sync_trident"
    sutTemplate = "LanceleafAA1"
	//relativityBranch = "develop"
    jobScript = "Trident/Scripts/nightly-performance.ps1"
    //cron = "0 3 * * *"
}

withCredentials([usernamePassword(credentialsId: 'HopperSutAdmin', usernameVariable: 'HopperSutAdminUserName', passwordVariable: 'HopperSutAdminPassword')]) {
    powershell "Get-ChildItem env:"
}