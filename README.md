# Integration Points - A Relativity application that integrates 3rd party
systems with Relativity

## Overview
 
Developers can leverage the Integration Point framework to pull data from a
third party system. Integration Points handles the scheduling, set up, field
mapping, and actual import into Relativity. In addition to supporting
developer-built custom integrations, Integration Points also provides built-in
integrations to the following:

* Lightweight Directory Access Protocol (LDAP) enabled HR servers such as
Microsoft Active Directory. 
* Relativity - Allowing documents to be pushed between workspaces.
* File Transfer - Providing connection to an FTP server for upload.
 
## How to Build
usage: build [debug|release] [dev|alpha|beta|rc|gold] [-version VERSION]
             [-apps] [-noapps] [-test] [-nuget] [-package]
             [-deploy <workspaceId> <ip_address/localhost>] [help|?]

options:

    -e[ditor]                       opens Build Helper Project Editor to edit
                                    the build.xml file

    -v[ersion] VERSION              sets the version # for the build, default
                                    is 1.0.0.0 (example: 1.3.3.7)
    -ap[ps]                         skips the build step, continues to only
                                    build apps
    -no[apps]                       skips build apps step
    -sk[ip]                         skips build and build apps step
    -t[est]                         runs nunit test step
    -nu[get]                        runs the nuget pack step
    -p[ackage]                      runs the package step
    -de[ploy] WORKSPACEID IPADDRESS uploads Integration Point binaries to a
                                    given Relativity Instance

    -al[ert]                        show alert popup when build completes

Common commands:
    build /?
    build
    build -deploy 1234567 localhost
    build -deploy 1234567 172.17.100.72

## How to Test
Running unit tests:

    unit tests:
        build -test
        build -skip -test

## Build artifacts
* RAPs
    * RelativityIntegrationPoints.Auto.rap
    * MyFirstProvider.rap
    * JsonLoader.rap
* DLLs
* PDBs
* Custom Pages
    * IntegrationPoints
    * MyFirstProvider
    * JsonLoader
* NuGet packages on gold build
    * RelativityIntegrationPoints (Contains RelativityIntegrationPoints.Auto.rap)
    * kCura.IntegrationPoints.Contracts
    * kCura.IntegrationPoints.Services.Interfaces.Private
    * kCura.IntegrationPoints.Web
* SDK
    kCura.IntegrationPoints.Contracts.dll
    kCura.IntegrationPonits.Domain.dll
    kCura.IntegrationPonits.SourceProviderInstaller.dll
    frame-messaging.js
    jquery-3.3.1.js
    jquery-postMessage.js
* Example projects (Coming soon)
    * MyFirstProvider.sln
    * JsonLoader.sln
* Workflow diagrams

## Maintainers
* Codigo O Plomo team (codigooplomo@relativity.com)
* Buena Vista Coding Club team (buenavistacodingclub@relativity.com>)

## Miscellaneous
[Documentation](https://help.relativity.com/integrationpoints/Content/Relativity_Integration_Points/Integration_Points/Relativity_Integration_Points.htm)
[Developer Documentation](https://platform.relativity.com/9.5/Content/Relativity_Integration_Points/Get_started_with_integration_points.htm)

Integration Points hosts the following private Kepler services:
* Job History Manager
* Document Manager
* Integration Point Manager

Integration Points is required for the ECA and Investigation application.
