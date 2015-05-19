Add-Type -TypeDefinition @"
public struct Change 
{
public string id;
public string user;
public string email;
public string date;
public string node;
public string desc;
public int mods;
public int adds;
public int dels;
}
"@

Function GetDate([String] $datestring) {

    $date = Get-Date -Year $datestring.Substring(0,4) -Month $datestring.Substring(4,2) -Day $datestring.Substring(6,2) -Hour $datestring.Substring(9,2) -Minute $datestring.Substring(11,2) -Second $datestring.Substring(13,2)
    
    return $date
}

Function GetTime([DateTime] $date) {

    $ts = New-TimeSpan -Start $date -End (Get-Date)

    $return = ''

    if($ts.TotalHours -gt 1) {
        $return += [String]($ts.Hours) + 'h:'
    }

    if($ts.TotalMinutes -gt 1) {
        $return += [String]($ts.Minutes) + 'm:'
    }

    $return += [String]($ts.Seconds) + 's'

    
    return $return
}

$username = 'kcura\bigboss'
$password = 'GobuildorgohomeG@dgets'
$buildserver = 'bld-mstr-01.kcura.corp'
$database = 'TCBuildVersion'

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

$buildid = 47574
$hgroot = 'C:\SourceCode\Mainline'

$statusraw = Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} "http://$buildserver/app/rest/builds/id:$buildid"

$x = [xml]$statusraw.InnerXml

$status = $x.build.status
$statusText = $x.build.statusText
$product = $x.build.buildType.projectName.split(':', [StringSplitOptions]::RemoveEmptyEntries)[0].trim()
$project = $x.build.buildType.projectName.split(':', [StringSplitOptions]::RemoveEmptyEntries)[1].trim()
$branch = $x.build.branchName
$buildtype =  ($x.build.properties.property | ? {$_.name -eq 'buildType'} | select Value).value
$agent = $x.build.agent.name
$buildversion = $x.build.number

$triggeredby = $x.build.triggered.type
$triggeredtime = GetDate($x.build.triggered.date)
$starttime = GetDate($x.build.startDate)


if($triggeredby -eq 'user') {
    $triggeredby = $x.build.triggered.user.username
}
else {
    $triggeredby = $x.build.triggered.details
}

if($status -eq 'SUCCESS') {
    $statusColor = 'green'
} 
else {
    $statusColor = 'red'

    $importantMessage = Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} "http://$buildserver/viewLog.html?buildId=$buildid&tab=buildLog&filter=important&hideBlocks=true&state=&expand=all#_"

    $importantMessage = $importantMessage.Substring($importantMessage.IndexOf('<div class="log rTree" id="buildLog">'), $importantMessage.IndexOf('<!-- END EXTENSION CONTENT jetbrains.buildServer.controllers.viewLog.BuildLogTab -->', $importantMessage.IndexOf('<div class="log rTree" id="buildLog">')) - $importantMessage.IndexOf('<div class="log rTree" id="buildLog">'));

    while ($importantMessage.IndexOf("<script ") -gt 0)
    {
	    $importantMessage = $importantMessage.Remove($importantMessage.IndexOf("<script "), $importantMessage.IndexOf("</script>") - $importantMessage.IndexOf("<script ") + 9);
    }
    if ($importantMessage.IndexOf('<div id="buildLogProgress"') -gt 0)
    {
	    $importantMessage = $importantMessage.Remove($importantMessage.IndexOf('<div id="buildLogProgress"'), $importantMessage.IndexOf("</div>", $importantMessage.IndexOf('<div id="buildLogProgress"')) - $importantMessage.IndexOf('<div id="buildLogProgress"') + 6);
    }

    $importantMessage = $importantMessage.Replace("<br class='hidden'>", "")

    $IMs = New-Object System.Collections.ArrayList

    foreach ($x in ([xml]$importantMessage).SelectNodes('/div/div/div/i')){
        if ($x.Attributes["class"].Value -eq "mark error_msg  status_warn")
	    {
		    [void]$IMs.Add([System.String]::Format("<span style='color:Orange'>{0}</span><br/>", $x.InnerText))
	    }
	    elseif ($x.Attributes["class"].Value -eq "mark error_msg  status_err")
	    {
		    [void]$IMs.Add([System.String]::Format("<span style='color:RED'>{0}</span><br/>", $x.InnerText))
	    }
    }

    if($IMs.Count -gt 100) {
        $IMs.RemoveRange(50, $IMs.Count - 100)
        $IMs.Insert(50, "<br/>...<br/>")
    }
}

$changesraw = Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} "http://$buildserver/app/rest/changes?locator=build:$buildid"

$x = [xml]$changesraw.InnerXml

$changes = @()
$changesLimit = 5
$changesCount = 0

Set-Location $hgroot
foreach ($node in $x.SelectNodes('/changes/change')){

    $changesCount++

    if ($changesCount -le $changesLimit) {
        $hg = (hg log -r $node.version --template "{author|user}|{author|email}|{date(date, '%m/%d/%Y %H:%M:%S %z')}|{node}|{join(file_mods, ';')}|{join(file_adds, ';')}|{join(file_dels, ';')}|{firstline(desc)}")

        $chng = New-Object Change
        $chng.id = $node.id
        $chng.user = $hg.split('|')[0]
        $chng.email = $hg.split('|')[1]
        $chng.date = $hg.split('|')[2]
        $chng.node = $hg.split('|')[3]
        $chng.mods = $hg.split('|')[4].split(';', [StringSplitOptions]::RemoveEmptyEntries).count
        $chng.adds = $hg.split('|')[5].split(';', [StringSplitOptions]::RemoveEmptyEntries).count
        $chng.dels = $hg.split('|')[6].split(';', [StringSplitOptions]::RemoveEmptyEntries).count
        $chng.desc = $hg.split('|')[7]
    }
    else {
        $hg = (hg log -r $node.version --template "{author|email}")

        $chng = New-Object Change
        $chng.email = $hg.split('|')[0]       
    }    

    $changes += $chng
}

$changers = ''
foreach ($chng in $changes) {
    if ($changers -eq '') {
        $changers = "'" + $chng.email + "'"
    }
    else {
        $changers += ", '" + $chng.email + "'"
    }
}

$changesText= ''

$urlFiles = "http://$buildserver/viewModification.html?modId=####&tab=vcsModificationFiles"
$urlExtra = "http://$buildserver/viewLog.html?buildId=$buildid&tab=buildChangesDiv"

$changesCount = 0

foreach ($chng in $changes) {
    $changesCount++

    if ($changesCount -gt $changesLimit) {
        continue
    }

    $changesText += '<table style="width: 100%; font-family: Tahoma; font-size: 10pt; border-bottom-style: dashed; border-width: 1px; border-color: #C0C0C0">
                        <tr>
                            <td style="padding-bottom: 0px;">
                            <table style="padding: 0px;">
                        <tr>
                     <td style="padding-bottom: 0px;">
                        <b>[' + $chng.user + ']</b>
                     </td>
                     <td style="font-size: 10pt; padding: 0px;">
                        (' + $chng.date + ')
                     </td>
                     <td rowspan="2" style="padding: 0px;">
                        <a href="' + $urlFiles.Replace('####', $chng.id) + '">'

    if($chng.mods -gt 0) {
        $changesText += '<span style="color: Orange;"> [Modified (' + $chng.mods + ')] </span>'
    }

    if($chng.adds -gt 0) {
        $changesText += '<span style="color: Green;"> [Added (' + $chng.adds + ')] </span>'
    }

    if($chng.dels -gt 0) {
        $changesText += '<span style="color: Red;"> [Deleted (' + $chng.dels + ')] </span>'
    }

    $changesText += '</a>
                     </td>
                     </tr>
                     <tr>
                        <td colspan="2" style="color: #0099FF; font-size: 10pt; padding: 0px;">' + $chng.node + '</td>
                     </tr>
                     </table>
                     </td>
                     </tr>
                     <tr>
                        <td style="padding-left: 23px; padding-top: 0px;">
                            <u>' + $chng.desc + '</u>
                        </td>
                     </tr>
                     </table>'
}

if ($changesCount -gt $changesLimit) {
    $changesText += "<a href='$urlExtra'>And "

    if ($changesCount -eq 100) {
        $changesText += "100+"
    }
    else {
        $changesText += ($changesCount - $changesLimit)
    }

    $changesText += " others</a>"
}


$body = '<div>
			<table style="width: 100%; font-family: Tahoma;">
				<tr>
					<td>
						<b>BUILD</b>&nbsp;<b style="color:' + $statusColor + '">' + $status + '</b>
					</td>
					<td style="text-align: right">
						<a href="http://bld-mstr-01.kcura.corp" style="text-decoration:none;"><b style="color:2AA4FC">Team</b><font color="FBBD30">City</font></a>
					</td>
				</tr>		
			</table>		
		</div>
		<div>
			<table style="font-family: Tahoma;">	

				<tr>
					<td>Triggered&nbsp;By</td>
					<td>:</td>
					<td>' + $triggeredby + ' @ ' + $starttime.ToShortDateString() + ' ' + $starttime.ToShortTimeString() + '</td>
				</tr>
				<tr>
					<td>Product</td>
					<td>:</td>
					<td>' + $product + '</td>
				</tr>
				<tr>
					<td>Project</td>
					<td>:</td>
					<td>' + $project + '</td>
				</tr>
				<tr>
					<td>Branch</td>
					<td>:</td>
					<td>'+ $branch + '</td>
				</tr>
				<tr>
					<td>Build&nbsp;Type</td>
					<td>:</td>
					<td>'+ $buildtype +'</td>
				</tr>
				<tr>
					<td>Agent</td>
					<td>:</td>
					<td>' +$agent + '</td>
				</tr>	
				<tr>
					<td>Build&nbsp;Time</td>
					<td>:</td>
					<td>' + $starttime.ToShortDateString() + ' ' + $starttime.ToShortTimeString() + ' - ' + (Get-Date).ToShortTimeString()  + ' (<i>~' + (GetTime($starttime)) + '</i>)</td>
				</tr>'


if($status -eq 'SUCCESS') {
    $body += '<tr>
				<td>Package</td>
				<td>:</td>
				<td><a href="file:\\BLD-PKGS.kcura.corp\Packages\' + $product + '\' + $branch + '\' + $buildversion + '">\\BLD-PKGS.kcura.corp\Packages\' + $product + '\' + $branch + '\' + $buildversion + '</a></td>
			</tr>'
}
else {
    $body += '<tr>
				<td style="vertical-align: top"><font color="red">Errors</font></td>
				<td style="vertical-align: top"><font color="red">:</font></td>
				<td><font color="red">' + $statusText + '</font>
					<div style="font-family:Courier New, Courier, monospace; font-size: x-small; font-weight: normal; font-style: normal;">
					' + $IMs + '
					</div>
				</td>
			</tr>
			<tr>
				<td></td>
				<td></td>
				<td><font color="red">To claim ownership of this issue, follow instructions <a href="https://einstein.kcura.com/display/DV/Team+City+build+failure+responsibility">here</a></font></td>
			</tr>'
}


$body += '<tr>
					<td>Links</td>
					<td>:</td>
					<td><a href="http://' + $buildserver + '/viewLog.html?buildId=' + $buildid + '">[Overview]</a>  <a href="http://' + $buildserver + '/viewLog.html?buildId=' + $buildid + '&tab=buildLog&filter=debug">[Build Log]</a></td>
				</tr>
				<tr>
					<td style="vertical-align: top">Changes</td>
					<td style="vertical-align: top">:</td>
					<td style="font-size: 10pt">' + $changesText + '</td>
				</tr>
			</table>
		</div>
		<br /><br />
		<div><a href="https://storyboard.kcura.com/Relativity/External.aspx?AppID=1015532&amp;ArtifactID=1015532&amp;DirectTo=%25applicationPath%25%2fCustomPages%2f433751B7-9A72-4166-A0BC-C0AF0133A780%2fTeamCityEmailSubscription.aspx%3fStandardsCompliance%3dtrue&#43;&amp;SelectedTab=1070366">Manage TeamCity Email</a></div>
		<br /><br />
		<div style="position: relative; bottom: 0px; text-align: center; width: 100%; font-family: Tahoma; font-size: small; color: #C0C0C0;">Powered by Gadgets&trade; Team</div> '


$Conn = New-Object System.Data.SqlClient.SqlConnection
$Conn.ConnectionString = "server='$buildserver';Database='$database';trusted_connection=true;"

$Conn.Open()

$Comm = New-Object System.Data.SqlClient.SqlCommand
$Comm.Connection = $Conn
$Comm.CommandText = "

SET NOCOUNT ON
	--Declare variables used by SQL passed in from NANT script
	DECLARE @projectName AS VARCHAR(255) = '$project'
	DECLARE @branchName AS VARCHAR(255) = '$branch'
	DECLARE @productName AS VARCHAR(255) = '$product'
	DECLARE @branchID as INT
	
	SET @branchID = (SELECT TOP 1 [ID] 
	                 FROM TCBranches
					 WHERE ProjectName = @projectName 
	                   AND BranchName = @branchName 
	                   AND ProductName = @productName)
	
	--Creates temp table to store all emails, used with DISTINCT so each email appears only once.
	CREATE TABLE #tmpEmails (Email VARCHAR (255))
	
	--Get email addresses of people subscribed to either Successful or Failure builds
	INSERT INTO #tmpEmails
	SELECT tcue.UserEmail
	FROM TCUserSubscription tcus
	INNER JOIN TCUserEmail tcue ON tcus.UserEmailID = tcue.[ID]
	INNER JOIN TCBranches tcb ON tcus.ProjectBranchID = tcb.[ID]
	WHERE tcb.[ID] = @branchID
	  AND tcus.Send${status} = 1
	  AND tcus.Send${status}Only1 = 0
	  
	--Get email addresses of people subscribed to either Successful or Failure builds after a failed or succeeded build respectively
	INSERT INTO #tmpEmails
	SELECT tcue.UserEmail
	FROM TCUserSubscription tcus
	INNER JOIN TCUserEmail tcue ON tcus.UserEmailID = tcue.[ID]
	INNER JOIN TCBranches tcb ON tcus.ProjectBranchID = tcb.[ID]
	WHERE tcb.[ID] = @branchID
	  AND tcus.Send${status}Only1 = 1
	  AND tcb.[Status] = CASE WHEN '$status' = 'Success' THEN 0 ELSE 1 END 
	
	--Get email addresses of people subscribed to builds they have triggered
	INSERT INTO #tmpEmails
	SELECT tcue.UserEmail
	FROM TCUserSubscription tcus
	INNER JOIN TCUserEmail tcue ON tcus.UserEmailID = tcue.[ID]
	INNER JOIN TCBranches tcb ON tcus.ProjectBranchID = tcb.[ID]
	WHERE tcb.[ID] = @branchID
      AND tcus.SendTrigger = 1 
	  AND tcue.UserEmail like '${triggeredby}%'
		
	--If there are any changes, get email addresses of people who subscribed to builds that contain their changes
	IF($changesCount > 0)
	BEGIN
		INSERT INTO #tmpEmails
		SELECT tcue.UserEmail
		FROM TCUserSubscription tcus
		INNER JOIN TCUserEmail tcue ON tcus.UserEmailID = tcue.[ID]
		INNER JOIN TCBranches tcb ON tcus.ProjectBranchID = tcb.[ID]
		WHERE tcb.[ID] = @branchID
		  AND tcus.SendChanges = 1 
		  AND tcue.UserEmail IN ($changers)
	END
	
	--Send failed builds to any and all changers in build
	IF('$status' <> 'SUCCESS')
	BEGIN
		INSERT INTO #tmpEmails
		SELECT tcue.UserEmail
		FROM TCUserEmail tcue
		WHERE tcue.UserEmail IN ($changers)
	END
	
	--Update branch table with build status
	UPDATE TCBranches 
	SET [Status] = CASE WHEN '$status' = 'SUCCESS' THEN 1 ELSE 0 END 
	WHERE ID = @branchID
	
	--Get distinct emails, so they only show up once
	SELECT DISTINCT Email FROM #tmpEmails
	DROP TABLE #tmpEmails
"

$rdr = $Comm.ExecuteReader()
$emails = @()

while($rdr.Read()) {
    $emails += $rdr.GetValue($1)
}


Send-MailMessage -From 'TeamCity@kcura.com' -To $emails -Subject "Build $status - $product [$branch] - $buildversion" -BodyAsHtml $body -SmtpServer "smtp.kcura.corp"






