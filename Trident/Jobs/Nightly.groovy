@Library('ProjectMayhem@v1') _

jobWithSut {
    slackChannel = "ci-rip"
    sutTemplate = "server-2022-patch-2"
    jobScript = "Trident/Scripts/Nightly.ps1"
    cron = "0 1 * * *"
}