# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.9] - 2023-04-06
### Changes
- Code isolation changes for integrationpoints-sdk
- Upgraded Relativity.IntegrationPoints.Services.Interfaces.Private dependency version to 1.2.2

## [1.0.8] - 2021-03-18
### Changed
- Updated kCura.EventHandler to 15.1.0

## [1.0.7] - 2019-01-16
### Added
- make-sdk.bat for generating Relativity Integration Points SDK.
- Upgraded Relativity.IntegrationPoints.Services.Interfaces.Private dependency version to 11.1.0 

## [1.0.6] - 2019-11-04
### Changed
- Upgraded Relativity.IntegrationPoints.Services.Interfaces.Private dependency version to 1.0.3.

## [1.0.5] - 2019-11-04
### Changed
- Fixed a bug in Relativity.IntegrationPoints.Contracts project Resource file causing exception when using Resources.

## [1.0.4] - 2019-10-31
### Changed
- Downgraded minimum required Relativity.Kepler version to 2.0.7.

## [1.0.3] - 2019-10-30
### Changed
- Added InternalsVisibleTo for kCura.IntegrationPoints.Core, kCura.IntegrationPoints.Core.Tests, kCura.IntegrationPoints.Domain to SourceProviderInstaller project and for kCura.IntegrationPoints.Core.Tests, kCura.IntegrationPoints.Domain, kCura.IntegrationPoints.EventHandlers, kCura.IntegrationPoints.EventHandlers.Tests, kCura.IntegrationPoints.Web, kCura.IntegrationPoints.Web.Tests to Contracts project to allow RIP to build properly.

## [1.0.2] - 2019-10-29
### Changed
- Downgraded minimum required Newtonsoft.Json version to 6.0.1.
- Upgraded Relativity.Kepler dependency version to 2.2.2.

## [1.0.1] - 2019-10-24
### Added
- Added missing dependencies to the Relativity.IntegrationPoints.SDK nuspec file.

### Changed
- Fixed inconsistency in the README file.
- Upgraded Relativity.IntegrationPoints.Services.Interfaces.Private dependency version to 1.0.2.
- Upgraded coverlet.msbuild dependency version to 2.6.3 to fix Jenkins build failure.
- Switched from the automatically generated nuspec file to the static nuspec file for Relativity.IntegrationPoints.Contracts project to avoid adding CSharpGuidelineAnalyzer as a nuget dependency.

## [1.0.0] - 2019-10-23
### Added
- Moved Relativity Integration Points SDK from the main Integration Points repository to this repository and renamed all projects and namespaces from 'kCura.' to 'Relativity.'.

