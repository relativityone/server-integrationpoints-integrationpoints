$JiraApiUri = "https://jira.kcura.com/rest"
$BitbucketApiUri = "https://git.kcura.com/rest/api/1.0"
$JenkinsApiUri = "https://jenkins.kcura.corp"
$JiraAssignee = $env:UserName
$PRreviewers = "konrad.kopciuch;piotr.szmigielski;konrad.blasinski;kamil.bizon;damian.janas;filip.wrona;patryk.spytkowski;artur.jaskiewicz;tomasz.peczek;kasper.kadzielawa"
$TeamName = "Codigo o Plomo"
$MainJiraLabel = "rip-rel-packages-update"
$RipUpdateJiraLabel = "rip-packages-update"
$RelativityUpdateJiraLabel = "rel-packages-update"
$LogCharsLimit = 8000
$AutoPackageUpgradeAdnotation = "This Jira was created automatically by AutoPackagesUpgrade script."

Export-ModuleMember -Variable *