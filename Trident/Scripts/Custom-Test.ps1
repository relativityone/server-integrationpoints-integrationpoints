<#
.SYNOPSIS
This script will be used by nightly pipeline to complie and run Integration tests
#>

$TaskRunner = Resolve-Path -Path build.ps1

&($TaskRunner) -Configuration Release

# Custom
&($TaskRunner) MyTest -Configuration Release -TestFilter "cat == Test"

# Nightly
#&($TaskRunner) MyTest -Configuration Release -TestFilter "(namespace =~ FunctionalTests || namespace =~ /Tests\.Integration[\$\.]/ || namespace =~ E2ETests) && cat != NotWorkingOnTrident"

# UI-ImportExport
#&($TaskRunner) MyTest -Configuration Release -TestFilter "cat == WebImportExport && cat != NotWorkingOnTrident"

# UI-NewSync
#&($TaskRunner) MyTest -Configuration Release -TestFilter "cat == RIP_SYNC && cat != NotWorkingOnTrident"

# UI-OldSync
#&($TaskRunner) MyTest -Configuration Release -TestFilter "(cat == RIP_OLD || cat == SCHEDULER ) && cat != NotWorkingOnTrident"