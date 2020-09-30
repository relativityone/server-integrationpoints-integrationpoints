@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "cd_trident_rip"
    sutTemplate = "aio-lanceleaf-latest"
    relativityBranch = "release-11.2-lanceleaf-1"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}