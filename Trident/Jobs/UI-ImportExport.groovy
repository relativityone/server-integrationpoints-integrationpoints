@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-lanceleaf-0"
    relativityBranch = "release-11.2-lanceleaf-0"
    jobScript = "Trident/Scripts/UI-ImportExport.ps1"
    cron = "0 1 * * *"
}