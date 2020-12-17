@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-mayapple-0"
    relativityBranch = "release-11.3-mayapple-0"
    jobScript = "Trident/Scripts/UI-OldSync.ps1"
    cron = "0 1 * * *"
}