$uploaderPath = [System.IO.Path]::Combine($root, 'DevelopmentScripts', 'RapLoader', 'RapLoader.exe')
$applicationXmlPath = [System.IO.Path]::Combine($root, 'ApplicationsXml', 'application.xml')

#[string]$workspaceId, [string]$serverName = $DEPLOY.split(' ')
$serverName = $DEPLOY.Trim()

#$relativityUsername = 'relativity.admin@kcura.com'
#$relativityPassword = 'Test1234!'

$sqlUsername = 'eddsdbo'
$sqlPassword = 'MySqlPassword123'

[xml] $applicationXml = Get-Content $applicationXmlPath

$applicationGuid = $applicationXml.Application.Guid

$rapPath = [System.IO.Path]::Combine($root, 'Applications', 'RelativityIntegrationPoints.Auto.rap')

$args = @()
$args += '/s'
$args += $serverName
$args += '/p'
$args += $rapPath
$args += '/du'
$args += $sqlUsername
$args += '/dp'
$args += $sqlPassword 
$args += '/ag'
$args += $applicationGuid
if($VERSION -ne "1.0.0.0")
{	$args += '/v'
	$args += $VERSION
}
$args += '/uw'
				
& $uploaderPath $args				