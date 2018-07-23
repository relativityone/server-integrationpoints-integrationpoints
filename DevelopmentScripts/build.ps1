#requires -version 3
$root = (Get-Item $PSScriptRoot).parent.FullName
Import-Module $root\Vendor\psake\tools\psake.psm1

$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$VERSION = "9.7.0.0"
$EDITOR = $false
$BUILD = $true
$APPS = $true
$TEST = $false
$INTEGRATION_TESTS = $false
$UI_TESTS = $false
$NUGET = $false
$PACKAGE = $false
$DEPLOY = ""
$ENABLEINJECTIONS = $false
$GIT = Test-Path -Path ([System.IO.Path]::Combine($root, '.git'))

$ALERT = [environment]::GetEnvironmentVariable("alertOnBuildCompletion","User")

$SHOWHELP = $false

$STATUS = $true


for ($i = 0; $i -lt $args.count; $i++){

    switch -Regex ($args[$i]){
        "^[/-]v"      {$VERSION = $args[$i + 1]; $i++}
        "^[/-]ap"     {$BUILD   = $false}
        "^[/-]no"     {$APPS    = $false}
        "^[/-]sk"     {$BUILD   = $false; $APPS = $false}
        "^[/-]t"      {$TEST    = $true}
        "^[/-]in"     {
                        $INTEGRATION_TESTS        = $true;
                        $INTEGRATION_TESTS_FILTER = $args[$i + 1];
                    }
        "^[/-]ui"     {
                        $UI_TESTS        = $true;
                        $UI_TESTS_FILTER = $args[$i + 1];
                    }
        "^[/-]nu"     {$NUGET   = $true}
        "^[/-]p"      {$PACKAGE = $true}
        "^[/-]de"     {
                       $CASE      = $args[$i + 1];
                       $IP        = $args[$i + 2];
                       $CUSTOMPAGE= $args[$i + 3];
                       $DEPLOY    += $CASE;
                       $DEPLOY    += " ";
                       $DEPLOY    += $IP;
                       $DEPLOY    += " ";
                       $DEPLOY    += $CUSTOMPAGE;
                       $i++;
        } 
        "^[/-]al"     {$ALERT   = $true}
        "^[/-]ei"     {$ENABLEINJECTIONS = $true}
                
        "^debug$"   {$BUILDCONFIG = "Debug"}
        "^release$" {$BUILDCONFIG = "Release"}

        "^dev$"   {$BUILDTYPE = "DEV"}
        "^alpha$" {$BUILDTYPE = "ALPHA"}
        "^beta$"  {$BUILDTYPE = "BETA"}
        "^rc$"    {$BUILDTYPE = "RC"}
        "^gold$"  {$BUILDTYPE = "GOLD"}

        "\?"   {$SHOWHELP = $true}
        "help" {$SHOWHELP = $true}

        "^[/-]e" {$EDITOR = $true} 
    }
}

write-host "buildconfig is" $BUILDCONFIG
write-host "buildtype   is" $BUILDTYPE
write-host "version     is" $VERSION
write-host "show editor is" $EDITOR
write-host "build             step is set to" $BUILD
write-host "apps              step is set to" $APPS
write-host "test              step is set to" $TEST
write-host "integration tests step is set to" $INTEGRATION_TESTS
write-host "ui tests          step is set to" $UI_TESTS
write-host "nuget             step is set to" $NUGET
write-host "package           step is set to" $PACKAGE
write-host "deploy            step is set to" ($DEPLOY -eq "")


if($ALERT) {
    Write-Host ""
    write-host "You will be notified after the build completes..."
    Write-Host ""
}

$startTime = Get-Date
Write-Host "Starting build at" $startTime

if($SHOWHELP) {

Write-Host ""
write-host "Use this script to peform a full build of all projects."
write-host "This build is the same as the build that happens on the build server. "
write-host ""
write-host "usage: build [debug|release] [dev|alpha|beta|rc|gold] [-version VERSION] [-apps] [-noapps] [-test] [-integration FILTER] [-ui FILTER] [-nuget] [-package] [-deploy <server>] [help|?]"
write-host ""
write-host "options:"
write-host ""
write-host "    -e[ditor]                       opens Build Helper Project Editor to edit the build.xml file" 
write-host "" 
write-host "    -v[ersion] VERSION              sets the version # for the build, default is 1.0.0.0 (example: 1.3.3.7)"  
write-host "    -ap[ps]                         skips the build step, continues to only build apps"
write-host "    -no[apps]                       skips build apps step"
write-host "    -sk[ip]                         skips build and build apps step"
write-host "    -t[est]                         runs nunit unit test step"
write-host "    -in[tegration] FILTER           runs nunit integration tests step with expression for where clause for nunit"
write-host "    -ui[tests] FILTER               runs nunit ui tests step with expression for where clause for nunit"
write-host "    -nu[get]                        runs the nuget pack step"
write-host "    -p[ackage]                      runs the package step"
write-host "    -de[ploy] WORKSPACEID IPADDRESS uploads Integration Point binaries to a given Relativity Instance"
write-host ""
write-host "    -al[ert]                        Sshow alert popup when build completes"
write-host ""

exit

}

if($EDITOR) {

    Invoke-psake $root\DevelopmentScripts\psake-editor.ps1 

    exit
}

if($VERSION -ne "1.0.0.0") {

    Invoke-psake $root\DevelopmentScripts\psake-version.ps1 -properties @{'version'=$VERSION;
                                                                          'server_type'='local';
                                                                          'build_config'=$BUILDCONFIG;
                                                                          'build_type'=$BUILDTYPE;} 
    if ($psake.build_success -eq $false) { $STATUS = $false }   
}

if($BUILD -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-build.ps1 -properties @{'version'=$VERSION;
                                                                        'server_type'='local';
                                                                        'build_config'=$BUILDCONFIG;
                                                                        'build_type'=$BUILDTYPE;
                                                                        'enable_injections'=$ENABLEINJECTIONS;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($APPS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-application.ps1 -properties @{'version'=$VERSION;
                                                                              'server_type'='local';
                                                                              'build_config'=$BUILDCONFIG;
                                                                              'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($TEST -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($INTEGRATION_TESTS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1 run_integration_tests -parameters @{'integration_tests_filter'="$INTEGRATION_TESTS_FILTER"}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($UI_TESTS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1 run_ui_tests -parameters @{'ui_tests_filter'="$UI_TESTS_FILTER"}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($NUGET -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-nugetpack.ps1 -properties @{'version'=$VERSION;
                                                                            'server_type'='local';
                                                                            'build_config'=$BUILDCONFIG;
                                                                            'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($PACKAGE -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-package.ps1 -properties @{'version'=$VERSION;
                                                                          'server_type'='local';
                                                                          'build_config'=$BUILDCONFIG;
                                                                          'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

Invoke-psake $root\DevelopmentScripts\psake-builddoc.ps1

if($DEPLOY -ne "" -and $STATUS){
	
    $deployScript = [System.IO.Path]::Combine($root, 'DevelopmentScripts', 'deploy.ps1').ToString()
	& $deployScript
    if(-not $?) {$STATUS = $false}
}

if($VERSION -ne "1.0.0.0") {
    if ($GIT) {
        git checkout -- $root\Version
    }
}

$endTime = Get-Date
Write-Host ""
Write-Host "Build finished at" $endTime
Write-Host ""
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

exit !$STATUS