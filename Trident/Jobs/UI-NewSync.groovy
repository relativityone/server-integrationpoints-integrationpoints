@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-mayapple-ea"
    relativityBranch = "release-11.3-mayapple-ea"
    jobScript = "Trident/Scripts/UI-NewSync.ps1"
    cron = "0 1 * * *"
}