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
 
## How to Build
 
    usage:
        build [debug|release] [dev|alpha|beta|rc|gold]
        [-b quiet|minimal|normal|detailed|diagnostic] [-skip] [-test]
        [-deploy 1234567 172.17.100.47] [-alert] [help|?]

    options:
        -b               sets the verbosity level for msbuild, default is
                         minimal
        -sk[ip]          skips build step
        -t[est]          runs nunit test step
        -de[ploy]        uploads Integration Point binaries to a given
    	                 Relativity Instance

        -al[ert]         shows alert popup when build completes

	Common commands:
        build /?
    	build
    	build -deploy localhost
    	build -deploy 172.17.100.72
	
## How to Test
Running tests:

    unit tests:
        build -test
    	build -skip -test
 
## Build artifacts
 
* RAP package (name: RelativityIntegrationPoints.Auto.rap)
* NuGet packages
    * kCura.IntegrationPoints.Contracts
    * kCura.IntegrationPoints.Services.Interfaces.Private
    * kCura.IntegrationPoints.Web
 
## Maintainers
 
Diffendoofer team (diffendoofer@kcura.com)
 
## Miscellaneous
 
[Documentation](https://help.kcura.com/integrationpoints/Content/Relativity_Integration_Points/Integration_Points/Relativity_Integration_Points.htm)

[Developer Documentation](https://platform.kcura.com/9.3/Content/Relativity_Integration_Points/Get_started_with_integration_points.htm)

Integration Points hosts the following private Kepler services:
* Job History Manager
* Document Manager
* Integration Point Manager

Integration Points is required for the ECA and Investigation
application.
