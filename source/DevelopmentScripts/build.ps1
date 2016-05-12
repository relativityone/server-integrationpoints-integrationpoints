#requires -version 3
$root = git rev-parse --show-toplevel
$root = [System.IO.Path]::Combine($root, 'source')
$branch = git branch

$BUILDFILE = [System.IO.Path]::Combine($root, 'DevelopmentScripts', 'Build.build')

$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$VERSION = "2.0.0.0"
$VERBOSE = "minimal"
$BUILD = $true
$TEST = $false
$DEPLOY = ""
$ALERT = [environment]::GetEnvironmentVariable("alertOnBuildCompletion","User")

$SHOWHELP = $false

$STATUS = $true


for ($i = 0; $i -lt $args.count; $i++){
    switch -Regex ($args[$i]){
        "^[-/]b"      {$VERBOSE   = $args[$i + 1]; $i++}
        "^[/-]sk"     {$BUILD     = $false; $APPS = $false}
        "^[/-]t"      {$TEST      = $true}
        "^[/-]de"     {
		               $CASE      = $args[$i + 1];
					   $IP        = $args[$i + 2];					   
					   $DEPLOY    += $CASE;
					   $DEPLOY    += " ";
					   $DEPLOY    += $IP;
					   $i++;
		}  
        "^[/-]al"     {$ALERT     = $true}
                
        "^debug$"   {$BUILDCONFIG = "Debug"}
        "^release$" {$BUILDCONFIG = "Release"}
        "^noopt$"   {$BUILDCONFIG += ",Optimize=false"}

        "^dev$"   {$BUILDTYPE = "DEV"}
        "^alpha$" {$BUILDTYPE = "ALPHA"}
        "^beta$"  {$BUILDTYPE = "BETA"}
        "^rc$"    {$BUILDTYPE = "RC"}
        "^gold$"  {$BUILDTYPE = "GOLD"}

        "\?"   {$SHOWHELP = $true}
        "help" {$SHOWHELP = $true}
    }
}

write-host "buildconfig is" $BUILDCONFIG
write-host "buildtype   is" $BUILDTYPE
write-host "verbosity   is" $VERBOSE
write-host "build     step is set to" $BUILD
write-host "test      step is set to" $TEST
write-host "deploy    step is set to" ($DEPLOY -eq "")

if($ALERT) {
write-host "You will be notified after the build completes..."
}

$startTime = Get-Date
Write-Host "Starting build at" $startTime

if($SHOWHELP) {

Write-Host ""
write-host "Use this script to peform a full build of all projects."
write-host "This build is the same as the build that happens on the TeamCity server. "
write-host ""
write-host "usage: build [debug|release] [dev|alpha|beta|rc|gold] [-b quiet|minimal|normal|detailed|diagnostic] [-skip] [-test] [-deploy 1234567 172.17.100.47] [-alert] [help|?]"
write-host ""
write-host "options:"
write-host ""
write-host "    -b               sets the verbosity level for msbuild, default is minimal"  
write-host "    -sk[ip]          skips build step"
write-host "    -t[est]          runs nunit test step"
write-host "    -de[ploy]        uploads Integration Point sync binaries given Relativity Instance"
write-host ""
write-host "    -al[ert]         show alert popup when build completes"
Write-Host ""

exit

}

if($BUILD){
    & nant build_all -buildfile:$BUILDFILE "-D:root=$root" "-D:buildconfig=$BUILDCONFIG" "-D:action=build" "-D:buildType=$BUILDTYPE" "-D:serverType=local" "-D:signOutput=false" "-D:verbosity=$VERBOSE"
    if(-not $?) {$STATUS = $false}
}

if($TEST -and $STATUS){
    & nant start_tests -buildfile:$BUILDFILE "-D:root=$root"
    if(-not $?) {$STATUS = $false}
}

if($DEPLOY -ne "" -and $STATUS){
    Invoke-Expression ([System.IO.Path]::Combine($root, 'DevelopmentScripts', 'UploadLDAPSyncBinariesIntoLocalRelativity.bat') + " " + $DEPLOY)
    if(-not $?) {$STATUS = $false}
}

$endTime = Get-Date
Write-Host "Build finished at" $endTime
Write-Host "Total time:" ([Math]::Round((New-TimeSpan -Start $startTime -End $endTime).TotalSeconds, 1)) "seconds."
 
if($ALERT) {
	if($STATUS) {
		if(([System.IO.File]::Exists( [System.IO.Path]::Combine($env:windir, 'Media', 'tada.wav')))) {
			Start-Job -ScriptBlock { (New-Object Media.SoundPlayer ([System.IO.Path]::Combine($env:windir, 'Media', 'tada.wav'))).PlaySync() } | Out-Null
		}	
		Start-Job -ScriptBlock { (New-Object -ComObject Wscript.Shell).Popup('Build SUCCESS!', 0, 'Build Status Update', 64) | Out-Null } | Out-Null
	}
	else {
		if(([System.IO.File]::Exists( [System.IO.Path]::Combine($env:windir, 'Media', 'chord.wav')))) {
			Start-Job -ScriptBlock { (New-Object Media.SoundPlayer ([System.IO.Path]::Combine($env:windir, 'Media', 'chord.wav'))).PlaySync() } | Out-Null
		}	
		Start-Job -ScriptBlock { (New-Object -ComObject Wscript.Shell).Popup('Build FAILED!', 0, 'Build Status Update', 16) | Out-Null } | Out-Null
	}
}