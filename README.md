# Integration Points - A Relativity application that integrates 3rd party systems with Relativity

## Overview

Developers can leverage the Integration Point framework to pull data from a
third party system. Integration Points handles the scheduling, set up, field
mapping, and actual import into Relativity. In addition to supporting
developer-built custom integrations, Integration Points also provides built-in
integrations to the following:

* Lightweight Directory Access Protocol (LDAP) enabled HR servers such as
Microsoft Active Directory.
* Relativity - Allowing documents to be pushed between workspaces.
* File Transfer - Providing connection to an FTP/SFTP server for upload.

## How to Build

Usage:
> ./build.ps1

For list of available build options, run:
> ./build.ps1 Help

Above command outputs the following:

    Name              Alias Depends On        Default Description
    ----              ----- ----------        ------- -----------
    Analyze                                      True Run build analysis
    CIFunctionalTest                                  Run tests that require a deployed environment.
    Clean                                             Delete build artifacts
    Compile                 NugetRestore         True Compile code for this repo
    FunctionalTest          OneTimeTestsSetup         Run tests that require a deployed environment.
    Help              ?                               Display task information
    MyTest                  OneTimeTestsSetup         Run custom tests based on specified filter
    NugetRestore                                      Restore the packages needed for this build
    OneTimeTestsSetup                                 Should be run always before running tests that require setup in deployed environment.
    Package                                      True Package up the build artifacts
    Rebuild                                           Do a rebuild
    RegTest                                           Run custom tests based on specified filter on regression environment
    Sign                                              Sign all files
    Test                                         True Run tests that don't require a deployed environment.

## How to Test

Running unit tests:
> ./build.ps1 Test

Running Custom Tests against Hopper Relativity environment:

1. Create feature branch with suffix _'-test'_
2. Modify _Trident/Scripts/Custom-Test.ps1_
   * Edit TestFilter if needed
   * Add _[Category("Test")]_ to selected tests
3. Push branch
4. Run build <https://trident.kcura.corp/dea/job/IntegrationPoints/job/IntegrationPoints-Jobs/job/IntegrationPoints-Custom-Test>

## Build artifacts

* RAPs
  * kCura.IntegrationPoints.rap
* DLLs
* PDBs
* Custom Pages
  * IntegrationPoints
* NuGet packages on gold build
  * RelativityIntegrationPoints (Contains kCura.IntegrationPoints.rap)
  * kCura.IntegrationPoints.Web
* SDK scripts
    frame-messaging.js
    jquery-3.4.1.js
    jquery-postMessage.js
* Workflow diagrams

## Maintainers

* Adler Sieben (adlersieben@relativity.com)
* team-server-delta -working on code isolation

## Miscellaneous

[Documentation](https://help.relativity.com/integrationpoints/Content/Relativity_Integration_Points/Integration_Points/Relativity_Integration_Points.htm)

[Developer Documentation](https://platform.relativity.com/9.5/Content/Relativity_Integration_Points/Get_started_with_integration_points.htm)

Integration Points hosts the following private Kepler services:
* Job History Manager
* Document Manager
* Integration Point Manager

Integration Points is required for the ECA and Investigation application.

## History  

The server-main branch was forked for Relativity Server by the Server Vertical.
Tag: 12.2
Branch: server-main
