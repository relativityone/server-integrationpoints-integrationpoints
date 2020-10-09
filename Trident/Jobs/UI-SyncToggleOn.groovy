@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-lanceleaf-eau"
    relativityBranch = "release-11.2-lanceleaf"
    jobScript = "Trident/Scripts/UI-Sync.ps1 -Toggle On"
    cron = "0 1 * * *"
}