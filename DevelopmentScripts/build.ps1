#requires -version 3
$root = (Get-Item $PSScriptRoot).parent.FullName
Import-Module $root\Vendor\psake\tools\psake.psm1

# Constants
New-Variable -Name CURRENT_VERSION -Value '10.2.0.0' -Option Constant
New-Variable -Name COMPANY -Value 'Relativity ODA LLC' -Option Constant
New-Variable -Name PRODUCT -Value 'IntegrationPoints' -Option Constant
New-Variable -Name PRODUCTDESCRIPTION -Value 'IntegrationPoints' -Option Constant
New-Variable -Name DEFAULTPACKAGEROOT -Value (Join-Path $root 'BuildPackages') -Option Constant

# Build variables
$SERVERTYPE = 'local'
$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$CUSTOM_BRANCH = ''
$VERSION = $CURRENT_VERSION
$PACKAGEROOT = $DEFAULTPACKAGEROOT
$EDITOR = $false
$BUILD = $true
$APPS = $true
$TEST = $false
$QUARANTINE = $false
$INTEGRATION_TESTS = $false
$UI_TESTS = $false
$NUGET = $false
$PACKAGE = $false
$DEPLOY = ""
$APIKEY = $null
# $GIT = Test-Path -Path ([System.IO.Path]::Combine($root, '.git'))
$RUN_SONARQUBE = $false
$SKIP_TESTS = $false
$ALERT = [environment]::GetEnvironmentVariable("alertOnBuildCompletion","User")

$SHOWHELP = $false

$STATUS = $true

for ($i = 0; $i -lt $args.count; $i++){

    switch -Regex ($args[$i]){
        "^[/-]v"      {$VERSION = $args[$i + 1]; $i++}
        "^[/-]b"      {$CUSTOM_BRANCH = $args[$i + 1]; $i++}
        "^[/-]r"      {$PACKAGEROOT = $args[$i + 1]; $i++}
        "^[/-]ci"     {$SERVERTYPE = 'Jenkins'}
        "^[/-]ap"     {$BUILD   = $false}
        "^[/-]no"     {$APPS    = $false}
        "^[/-]sk"     {$BUILD   = $false; $APPS = $false}
        "^[/-]t"      {$TEST    = $true}
        "^[/-]qu"     {$QUARANTINE = $true}
        "^[/-]in"     {
                        $INTEGRATION_TESTS        = $true;
                        $INTEGRATION_TESTS_FILTER = $args[$i + 1];
                      }
        "^[/-]ui"     {
                        $UI_TESTS        = $true;
                        $UI_TESTS_FILTER = $args[$i + 1];
                      }
        "^[/-]nu"     {$NUGET   = $true; $APIKEY = $args[$i + 1]; $i++}
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
        "^[/-]st"      {$SKIP_TESTS = $true}
        "^[/-]al"      {$ALERT   = $true}
                
        "^debug$"   {$BUILDCONFIG = "Debug"}
        "^release$" {$BUILDCONFIG = "Release"}

        "^dev$"   {$BUILDTYPE = "DEV"}
        "^alpha$" {$BUILDTYPE = "ALPHA"}
        "^beta$"  {$BUILDTYPE = "BETA"}
        "^rc$"    {$BUILDTYPE = "RC"}
        "^gold$"  {$BUILDTYPE = "GOLD"}

        "^[/-]\?"   {$SHOWHELP = $true}
        "^[/-]h"    {$SHOWHELP = $true}

        "^[/-]e" {$EDITOR = $true} 
        "^[/-][Ss]onar[Qq]ube$" {$RUN_SONARQUBE = $true}
    }
}

write-host "buildconfig       is" $BUILDCONFIG
write-host "buildtype         is" $BUILDTYPE
write-host "version           is" $VERSION
write-host "server type       is" $SERVERTYPE
write-host "show editor       is" $EDITOR
write-host "branch            is" $CUSTOM_BRANCH
write-host "package root      is" $PACKAGEROOT
write-host "build             step is set to" $BUILD
write-host "apps              step is set to" $APPS
write-host "test              step is set to" $TEST
write-host "quarantine        is" $QUARANTINE
write-host "integration tests step is set to" $INTEGRATION_TESTS
write-host "ui tests          step is set to" $UI_TESTS
write-host "nuget             step is set to" $NUGET
write-host "package           step is set to" $PACKAGE
write-host "deploy            step is set to" ($DEPLOY -eq "")
write-host "sonarQube         step is set to" $RUN_SONARQUBE
write-host "skip tests build  step is set to" $SKIP_TESTS


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
write-host "usage: build [debug|release] [dev|alpha|beta|rc|gold] [-version VERSION] [-branch BRANCH] [-apps] [-noapps] [-test] [-integration FILTER] [-ui FILTER] [-nuget] [-package] [-ci] [-deploy <server>] [-help|-?]"
write-host ""
write-host "options:"
write-host ""
write-host "    -e[ditor]                       opens Build Helper Project Editor to edit the build.xml file" 
write-host "" 
write-host "    -v[ersion] VERSION              sets the version # for the build, default is $CURRENT_VERSION (example: 1.3.3.7)"
write-host "    -b[ranch] BRANCH                uses BRANCH as the Git branch name when building"
write-host "    -r[oot] PACKAGEROOT             uses PACKAGEROOT as the root directory for packaging the product (e.g. '.\BuildPackages'), default is $DEFAULTPACKAGEROOT"
write-host "    -ap[ps]                         skips the build step, continues to only build apps"
write-host "    -no[apps]                       skips build apps step"
write-host "    -sk[ip]                         skips build and build apps step"
write-host "    -qu[arantine]                   modifies tests results and report file names adding Quarantine postfix"
write-host "    -t[est]                         runs nunit unit test step"
write-host "    -in[tegration] FILTER           runs nunit integration tests step with expression for where clause for nunit"
write-host "    -ui[tests] FILTER               runs nunit ui tests step with expression for where clause for nunit"
write-host "    -nu[get] PROGET_API_KEY         runs the nuget pack & publish steps using the given ProGet API key"
write-host "    -p[ackage]                      runs the package step"
write-host "    -ci                             sets the server type to be one appropriate for a CI/CD run; always use this when running on Jenkins"
write-host "    -de[ploy] WORKSPACEID IPADDRESS uploads Integration Point binaries to a given Relativity Instance"
write-host ""
write-host "    -al[ert]                        show alert popup when build completes"
write-host "    -sonarqube                      runs sonarqube analysis and send results to server"
write-host "    -st                             skips build of test projects to shorten build times"
write-host "    -h[elp]/-?                      show this usage info and exit"
write-host ""

exit

}

if($EDITOR) {

    Invoke-psake $root\DevelopmentScripts\psake-editor.ps1 

    exit
}

# Helper fxn to conditionally add a psake property. Awkward workaround,
# but we don't have good access to the branch name on Jenkins. Requires
# more significant refactoring to get around.
function Set-BranchProperty([string] $Branch, [Hashtable] $Properties)
{
    if ($null -ne $Branch -and "" -ne $Branch)
    {
        $Properties['branch'] = $Branch
    }
    $Properties
}

if ($VERSION -ne $CURRENT_VERSION)
{
    $properties = Set-BranchProperty $CUSTOM_BRANCH @{'version'=$VERSION;
                                                      'server_type'=$SERVERTYPE;
                                                      'build_config'=$BUILDCONFIG;
                                                      'build_type'=$BUILDTYPE;
                                                      'company'=$COMPANY;
                                                      'product'=$PRODUCT;
                                                      'product_description'=$PRODUCTDESCRIPTION;}
    Invoke-psake $root\DevelopmentScripts\psake-version.ps1 -properties $properties
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($BUILD -and $STATUS){
    $properties = Set-BranchProperty $CUSTOM_BRANCH @{'version'=$VERSION;
                                                      'server_type'=$SERVERTYPE;
                                                      'build_config'=$BUILDCONFIG;
                                                      'build_type'=$BUILDTYPE;
                                                      'run_sonarqube'=$RUN_SONARQUBE;
                                                      'skip_tests'=$SKIP_TESTS;}
    Invoke-psake $root\DevelopmentScripts\psake-build.ps1 -properties $properties
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($APPS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-application.ps1 -properties @{'version'=$VERSION;
                                                                              'server_type'=$SERVERTYPE;
                                                                              'build_config'=$BUILDCONFIG;
                                                                              'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($TEST -and $STATUS -and ($RUN_SONARQUBE -eq $false)){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($INTEGRATION_TESTS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1 run_integration_tests -parameters @{'tests_filter'="$INTEGRATION_TESTS_FILTER";
                                                                                             'is_quarantine'=$QUARANTINE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($UI_TESTS -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-test.ps1 run_ui_tests -parameters @{'tests_filter'="$UI_TESTS_FILTER";
                                                                                    'is_quarantine'=$QUARANTINE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($PACKAGE -and $STATUS){
    Invoke-psake $root\DevelopmentScripts\psake-nugetpack.ps1 -properties @{'version'=$VERSION;
                                                                            'server_type'=$SERVERTYPE;
                                                                            'build_config'=$BUILDCONFIG;
                                                                            'build_type'=$BUILDTYPE;}
    if ($psake.build_success -eq $false) { $STATUS = $false }

    if ($STATUS)
    {
        # Doing this allows us to specify a non-existent relative path on the command line but pass a "resolved" path to the psake script.
        $resolvedPackageRoot = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PACKAGEROOT)

        $properties = Set-BranchProperty $CUSTOM_BRANCH @{'version'=$VERSION;
                                                          'product'=$PRODUCT;
                                                          'package_root_directory'=$resolvedPackageRoot;
                                                          'server_type'=$SERVERTYPE;
                                                          'build_config'=$BUILDCONFIG;
                                                          'build_type'=$BUILDTYPE;}
        Invoke-psake $root\DevelopmentScripts\psake-package.ps1 -properties $properties
        if ($psake.build_success -eq $false) { $STATUS = $false }
    }
}

if($NUGET -and $STATUS) {
    if ($null -eq $APIKEY -or "" -eq $APIKEY)
    {
        throw "Proget API key must be provided to publish. Run this script with 'help' or '?' to see usage information."
    }

    Invoke-psake $root\DevelopmentScripts\psake-nugetpublish.ps1 -properties @{'version'=$VERSION;
                                                                                'build_config'=$BUILDCONFIG;
                                                                                'build_type'=$BUILDTYPE;
                                                                                'proget_api_key'=$APIKEY;}
    if ($psake.build_success -eq $false) { $STATUS = $false }
}

if($DEPLOY -ne "" -and $STATUS){

    $deployScript = [System.IO.Path]::Combine($root, 'DevelopmentScripts', 'deploy.ps1').ToString()
    & $deployScript
    if(-not $?) {$STATUS = $false}
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