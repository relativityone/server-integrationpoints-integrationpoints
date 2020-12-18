@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-mayapple-1"
    relativityBranch = "release-11.3-mayapple-1"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}