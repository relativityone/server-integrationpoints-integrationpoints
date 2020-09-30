@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-lanceleaf-latest"
    relativityBranch = "release-11.2-lanceleaf-1"
    jobScript = "Trident/Scripts/UI-Sync.ps1 -Toggle On"
    cron = "0 1 * * *"
}