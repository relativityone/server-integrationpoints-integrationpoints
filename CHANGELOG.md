# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
## [1.2.2] - YYYY-MM-DD
### Changed
- [REL-825278] Code isolation

## [1.2] - 2021-07-16
### Changed
- Add IIntegrationPointsAgentManager Kepler with GetWorkloadAsync method

## [1.1] - 2021-07-15
### Changed
- Add RunDeploymentHealthChecksAsync method

## [1.0.5] - 2021-01-29
### Changed
- Add ImportFileCopyMode property to CreateIntegrationPointRequest

## [1.0.4] - 2020-03-26
### Changed
- Mark IIntegrationPointManager.GetEligibleToPromoteIntegrationPointsAsync method as obsolete

## [1.0.3] - 2019-11-04
### Changed
- Fixed package description in the nuspec file.

## [1.0.2] - 2019-10-24
### Changed
- Upgraded CSharpGuidelineAnalyzer dependency version to 3.1.0.
- Switched from the automatically generated nuspec file to the static nuspec file for Relativity.IntegrationPoints.Services.Interfaces.Private project to avoid adding CSharpGuidelineAnalyzer as a nuget dependency.
- Fixed inconsistent indentation in some of the C# source files.

## [1.0.1] - 2019-10-24
### Changed
- Renamed project directories from 'kCura.' to 'Relativity.'.

## [1.0.0] - 2019-09-24
### Added
- Moved Relativity Kepler Services Interfaces from the main Integration Points repository to this repository and renamed project and namespaces from 'kCura.' to 'Relativity.'.


