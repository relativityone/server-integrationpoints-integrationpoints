#Requires -Version 5.0

<#
.SYNOPSIS
    Gets version for current build

.PARAMETER projectName
    Project name

.PARAMETER buildType
    Current build type

.PARAMETER scriptsDir
    Path to scripts directory

.PARAMETER branchName
    Current branch name
#>

[CmdletBinding()]
param(
    [string]$projectName,
    [ValidateSet("DEV", "GOLD")]
    [string]$buildType,
    [string]$scriptsDir,
    [string]$branchName
)

Write-Output "Attempting to get package version..."

Write-Output "Branch name: $branchName"

Write-Verbose "Retrieving all tags matching version-* on branch $branchName..."
$tags = @(& git tag version-* --sort=-creatordate --merged "origin/$branchName")
if (!$tags) {
    throw "Unable to find any tag."
}

$tag = $tags[0]
Write-Verbose "Using tag $tag..."

Write-Verbose "Extracting major and minor from tag..."
if ($tag -match "^version-(\d+)\.(\d+)$") {
    $major = $matches[1]
    $minor = $matches[2]

    Write-Output "Major and minor: $major.$minor"
}
else {
    throw "Tag $tag is invalid."
}

if ($branchName -match "^develop$") {
    if ($buildType -eq "GOLD") {
        throw "GOLD builds are supported for release branches only"
    }
    $suffix = "beta"
}

if ($branchName -match "^REL-\d{6}") {
    if ($buildType -eq "GOLD") {
        throw "GOLD builds are supported for release branches only"
    }    
    $suffix = "dev"
}

if ($branchName -match "^release-(\d+)-") {
    if ($buildType -ne "GOLD") {
        $suffix = "rc"
    }
}

Write-Verbose "Retrieving version from database..."
& (Join-Path $scriptsDir "get-version-next-build.ps1") -projectName $projectName -buildType $buildType -majorNumber $major -minorNumber $minor

Write-Output "Next version: $global:nextVersion"

if ($global:nextVersion -match "^(\d+)\.(\d+)\.(\d+)\.(\d+)$") {
    $patch = $matches[3]
    $build = $matches[4]
}
else {
    throw "Invalid version $global:nextVersion format"
}

if ($buildType -eq "GOLD") {
    $global:version = "$major.$minor.$patch"
    $global:packageVersion = "$major.$minor.$patch"
}
else {
    $global:version = "$major.$minor.$patch.$build"
    $global:packageVersion = "$major.$minor.$patch-$suffix-$build"
}