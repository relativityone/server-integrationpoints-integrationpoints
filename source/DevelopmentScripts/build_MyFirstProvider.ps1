#requires -version 3
$root = git rev-parse --show-toplevel
$root = [System.IO.Path]::Combine($root, 'source')
$branch = git branch

$BUILDFILE = [System.IO.Path]::Combine($root, 'DevelopmentScripts', 'Build.build')

$BUILDCONFIG = "Debug"
$BUILDTYPE = "DEV"
$VERSION = "0.0.0.1"
$VERBOSE = "minimal"

nant build_myfirstprovider -buildfile:$BUILDFILE "-D:root=$root" "-D:buildconfig=$BUILDCONFIG" "-D:action=build" "-D:buildType=$BUILDTYPE" "-D:serverType=local" "-D:signOutput=false" "-D:verbosity=$VERBOSE"
