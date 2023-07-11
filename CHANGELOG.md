# Changelog

All notable changes to this project will be documented in this file.

## [12.8.0] - 07-04-2023

### Changed

- Updated Relativity.OutsideIn package to 2023.4.0
- Updated kCura.OutsideIn.FI.Win32, kCura.OutsideIn.FI.Win64, kCura.OutsideIn.Full.Win64 packages to 2023.4.0
- Updated Relativity Kepler packages to 2.15.5s
- Updated Relativity.Testing.Framework packages to 10.3.0
- Updated Relatrivy Core packages to 48.2.17
- Updated Relativity SDK packages to 17.4.2
- Updated Toggles package to 1.10.8
- Updated NUnit package to 3.13.3
- Updated NUnit3TestAdapter package to 4.4.2
- Removed IgnoreAttribute from functional tests
- Reordered all assembly files in build.xml for improved maintenance

### Fixed

- Relativity.IntegrationPoints.Services now references the correct RIP dependency/platform packages
- All functional tests that used the RIP specific Kepler service endpoints that previously failed now pass
- PSAKE build scripts now update the PATH environment variable to include the chrome driver directory

## [12.7.0] - 06-20-2023

### Changed

- [REL-848320](https://jira.kcura.com/browse/REL-848320) - scheduled jobs miscalculation changes - Backported [REL-620760](https://jira.kcura.com/browse/REL-620760) ticket from Server 2022 release.

## [12.6.0] - 06-20-2023

### Changed

- [REL-848321](https://jira.kcura.com/browse/REL-848321) - backport changes implemented - Backported [REL-673577](https://jira.kcura.com/browse/REL-673577) ticket from Server 2022. 

## [12.5.0] - 06-07-2023

### Added

- Added updated version jquery files like jquery-3.6.3.min.js, jquery-3.6.3.min.map, jquery-3.6.3.slim.min.js, jquery-3.6.3.slim.min.map, jquery-ui-1.13.2.min.js.
- Added and Renamed jquery files for jquery-ui-1.13.2.js, jquery-3.6.3.js, jquery-3.6.3.slim.js.

### Changed

- Bumped version to 12.5.0 in version.txt file.
- Upgraded Relativity Sync version to 0.9.0.
- Modified jquery-ui.css and jquery-ui.min.css file.
- Modified _reference.js file with jquery version.
- Removed older version files like jquery-3.5.1.intellisense.js, jquery-3.5.1.min.js, jquery-3.5.1.min.map, jquery-3.5.1.slim.min.js, jquery-3.5.1.slim.min.map, jquery-ui-1.12.1.min.js.
- Modified kCura.IntegrationPoints.Web.csproj with latest versions of jquery files.

## [12.4.0] - 05-25-2023

### Added

- Added a new changelog.md

### Changed

- Archived the existing changelog.md file
- Removed ##Maintainers section from README.md
- SlackChannel has been updated to ci-server-delta in trident file
- The code owners details has been updated to show all server delta team members
- Bumped the minor version and zeroed out the patch number