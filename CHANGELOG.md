
## [24000.0.16] - 30-June-2024

### Changed

- [REL-870855](https://jira.kcura.com/browse/REL-870855) [RIP] Remove 'EnableSyncNonDocumentFlowToggle' Toggle - Backported from [REL-808758](https://jira.kcura.com/browse/REL-808758)

## [24000.0.15] - 28-June-2024

### Changed

- [REL-974405](https://jira.kcura.com/browse/REL-974405) - Removed OTEL assemblies from the RIP RAP file

## [24000.0.14] - 24-June-2024

### Changed

- [REL-973479](https://jira.kcura.com/browse/REL-973479) - Upgrade latest IAPI into SFU, Sync and RIP

## [24000.0.13] - 12-June-2024

### Changed

- [REL-944166](https://jira.kcura.com/browse/REL-944166) - Consumed oauth2 client into RIP
- Revved latest IAPI and that has unified RDC and SDK version
- Revved latest relativity outsidein upgraded to match IAPI version

## [24000.0.12] - 17-May-2024

### Changed

- [REL-946043](https://jira.kcura.com/browse/REL-946043) - Revved latest package for Relativity.DataExchange.Client.SDK and Relativity.Transfer.Client

## [24000.0.11] - 08-May-2024

### Changed

- [REL-942148](https://jira.kcura.com/browse/REL-942148) - [Server 2024] RAP Schema Update.

## [24000.0.10] - 24-APR-2024

### Changed
- [REL-931883](https://jira.kcura.com/browse/REL-931883) - Update OTEL Dependencies and Verify Kepler Services.

## [24000.0.9] - 19-APR-2024

### Changed
- [REL-929404](https://jira.kcura.com/browse/REL-929404) - Removed DNS Health checks which was adoption of R1 code changes.

## [24000.0.8] - 17-March-2024

- [REL-891061](https://jira.kcura.com/browse/REL-891061) - Combine RelativitySync into RIP monolith - Pipeline and Nightly changes

## [24000.0.7] - 29-Feb-2024

- [REL-891081](https://jira.kcura.com/browse/REL-891081) - Combine Bitbucket integrationpoints-sdk into GitHub RIP monolith.

## [24000.0.6] - 28-Feb-2024

### Changed

- [REL-891078](https://jira.kcura.com/browse/REL-891078) - Combine Bitbucket integrationpoints-myfirstprovider into GitHub RIP monolith.

## [24000.0.5] - 27-Feb-2024

### Changed

- [REL-891072](https://jira.kcura.com/browse/REL-891072)  -  Combine Bitbucket integrationpoints-jsonloader into GitHub RIP monolith.

## [24000.0.4] - 21-Feb-2024

### Changed

- [REL-895553](https://jira.kcura.com/browse/REL-895553) - [Server Backport] [SYNC] Enable Non-Admin User to Run job on new Destination Workspace
- [REL-911797](https://jira.kcura.com/browse/REL-911797) - Changes made to sync to create webapi url that is supplied by rip and Logic to get Integration point web api url.

## [24000.0.3] - 06-Feb-2024

### Changed

- [REL-891060](https://jira.kcura.com/browse/REL-891060)  -  Combine Bitbucket RelativitySync into GitHub RIP monolith.

## [24000.0.2] - 06-Feb-2024

### Changed

- [REL-891075](https://jira.kcura.com/browse/REL-891075) - Combine Bitbucket integrationpoints-keplerservicesinterfaces into GitHub RIP monolith.

## [24000.0.1] - 12-14-2023

### Changed

- [REL-891069](https://jira.kcura.com/browse/REL-891069) Cloned repo from BitBucket to GitHub and created pipeline in AzDO.

## [24000.0.0] - 10-27-2023

### Changed

- Prepared branch for the next official Relativity 2024 release.
- Use the latest SUT release image.

## [23013.2.1004] - 10-06-2023

### Changed

- [REL-866456](https://jira.kcura.com/browse/REL-866456) - Revved latest package for Relativity.DataTransfer.Legacy.SDK

## [23013.2.1003] - 10-06-2023

### Changed

-[REL-873166](https://jira.kcura.com/browse/REL-873166) -Intermittent error on regression environments when navigating to certain pages integration point - Backported [REL-785057](https://jira.kcura.com/browse/REL-785057) [RIP] Error page is displayed when user clicks on Job ID
## [23013.2.1002] - 10-06-2023

### Changed

- [REL-875582](https://jira.kcura.com/browse/REL-875582) - Server - Incident - Error while loading LDAP provider - Backported [REL-872003](https://jira.kcura.com/browse/REL-872003) ticket from server 2022 release

## [23013.2.1001] - 10-06-2023

### Changed

- [REL-873991](https://jira.kcura.com/browse/REL-873991) - RIP Job History entries disappear after editing and saving integration point - Backported [REL-833766](https://jira.kcura.com/browse/REL-833766) ticket from server 2022 release
- Bumped the patch version based on [server-versioning-statergy](https://github.com/relativityone/server-adr/blob/5dd2dd1b1ce0592cdead2dba5127e4b622c4a9ff/00013-server-versioning-strategy.md)

## [23013.2.4] - 09-11-2023

### Changed

- Bumped the application version to align with the Server 2023 application versioning strategy ADR.

## [23013.2.3] - 09-08-2023

### Changed

- [REL-875022](https://jira.kcura.com/browse/REL-875022) - Revving Relativity Sync.

## [23013.2.2] - 09-04-2023

### Changed

- Bumped the application version to align with the Server 2023 application versioning strategy ADR.

## [13.2.2] - 09-01-2023

### Changed  
- [REL-871666](https://jira.kcura.com/browse/REL-871666) - Improve Import API error message when Kepler and WebAPI endpoints are not found.

## [13.2.1] - 08-24-2023
 
### Changed
 
- [REL-870446](https://jira.kcura.com/browse/REL-870446) - Added DefaultValue attribute to toggles EnableSyncNonDocumentFlowToggle and EnableRelativitySyncApplicationToggle. So that Sync toggles are enabled by default

## [13.2.0] - 08-17-2023

### Changed

- [REL-868461](https://jira.kcura.com/browse/REL-868461) - Create release branch for RAPCD
- Official Relativity 2023 12.3 release.
- The SUT configuration upgrades the previous release image to the latest release image.
## [13.1.0] - 07-21-2023

### Changes

- Code isolation changes for integrationpoints

### 0.1.0

- Initial work