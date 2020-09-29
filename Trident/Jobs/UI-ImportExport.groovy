@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-mayapple-latest"
    relativityBranch = "release-11.3-mayapple"
    jobScript = "Trident/Scripts/UI-ImportExport.ps1"
    cron = "0 1 * * *"
}