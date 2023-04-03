# Json Loader

Json Loader is a custom provider compatible with Integration Points app. It allows for importing data into Relativity directly from JSON file.

## History  

The server-main branch was forked for Relativity Server by the Server Vertical.
Tag: 11.1
Branch: server-main


## Build Tasks

This repository builds with Powershell through the `.\build.ps1` script. 
It supports standard tasks like `.\build.ps1 compile`, `.\build.ps1 test`, `.\build.ps1 functionaltest`, and `.\build.ps1 package`.

For functional tests, point PowerShell to the root of this repository and provide the necessary arguments for the test settings using `.\DevelopmentScripts\New-TestSettings.ps1 <INSERT_ARGUMENTS_HERE>` before running the functionaltest task.

## Maintainers

This repository is owned by the Codigo o Plomo Team.