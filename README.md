# RAP Repository Template

This repository should serve as the template when migrating your project to RAP CD or creating a new project using RAP CD.
Note: If you are going to start your project's repository using this template, you must replace the contents of this document with information specific to your project. For information on what your documentation should look like, refer to [this Einstein page](https://einstein.kcura.com/x/RglUB)

## Build Tasks

This repository builds with Powershell through the `.\build.ps1` script. 
It supports standard tasks like `.\build.ps1 compile`, `.\build.ps1 test`, `.\build.ps1 functionaltest`, and `.\build.ps1 package`.

For functional tests, point PowerShell to the root of this repository and provide the necessary arguments for the test settings using `.\DevelopmentScripts\New-TestSettings.ps1 <INSERT_ARGUMENTS_HERE>` before running the functionaltest task.


## Online Documentation

For more information on RAP CD, [view the documentation in Einstein](https://einstein.kcura.com/x/hRkFCQ)

## Maintainers

This repository is owned by the Tools Team. Please send any issues or feature requests to tools-support@relativity.com