@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-lanceleaf-eau"
    relativityBranch = "release-11.2-lanceleaf"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}