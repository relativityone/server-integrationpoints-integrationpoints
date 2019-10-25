# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

