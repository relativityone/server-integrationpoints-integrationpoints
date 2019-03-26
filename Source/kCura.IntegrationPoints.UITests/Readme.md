# UI test

## Developer documentation

More details about RIP UI tests: https://einstein.kcura.com/display/DV/RIP+UI+Tests

---

## Choosing web browser

Change `UI.Browser` in `app.config`. Currently Chrome and Firefox are supported and both support headless mode.

---

## Running tests locally

To run tests against chosen environment, either set `ripUiTestsConfig` system variable, or just set `_configName` in `kCura.IntegrationPoints.UITests.Configuration.TestConfiguration`. Its value should correspond to config file located in `kCura.IntegrationPoints.UITests\UiTestsConfig\`.
In case of running against your TestVM, modify `testvm.config` file. Set proper values to:
* RelativityInstanceAddress
* RSAPIServerAddress
* ServerBindingType
* AdminUsername
* AdminPassword
* fileshareLocation

Web browsers run in headless mode by default. To disable this mode and see browser window, set `UI.Options.Arguments.Headless` in `app.config` to `false`.

It's recommended to run these tests in debug mode in Visual Studio. Thanks to that, Visual Studio will pause execution on error. Then it's possible to interact with web page as one wishes. For example, data entered on page can be modified, Integration Point can be rerun, or Errors tab can be checked.

---

## Running tests on Jenkins

UI tests can be executed as part of these pielines:
* Integration Points (https://jenkins.kcura.corp/job/DataTransfer/job/IntegrationPoints/) - UI test are not executed by default. They can be enabled by running this job manually.
* Integration Points Nightly (https://jenkins.kcura.corp/job/DataTransfer/job/IntegrationPointsNightly/) - the same as default.
* Integration Points UI tests (https://jenkins.kcura.corp/job/DataTransfer/job/IntegrationPointsUITests/) - all UI tests are executed by default.

---

## General development rules
* Do not use inheritance just for dealing with code duplication. See https://www.petrikainulainen.net/programming/unit-testing/3-reasons-why-we-should-not-use-inheritance-in-our-tests/
* Log all relevant information on proper logging level. Use `kCura.IntegrationPoints.UITests.Logging.LoggerFactory` to get logger with common configuration.

---

## Troubleshooting

### Tests fail without any clear reason

#### Symptoms

Test passes setup phase, where Relativity objects are created with various APIs, but then fails before any action with browser. Usually it looks like timeout during accessing web page.

Part of the logs:
```
16:24:34 2018-12-17 09:14:44 [Information] [kCura.IntegrationPoints.UITests.Driver.DriverFactory] Driver info:
16:24:34 "Browser name: chrome
16:24:34 Browser version: 71.0.3578.98
16:24:34 Browser width: 1920, height: 1400
16:24:34 Driver implicit wait: 00:00:20
16:24:34 Driver URL: https://cd-sut-am64p06w.kcura.corp/Relativity/Identity/login?signin=c82d576eabbe73bf400bdceabdc76814
16:24:34 Platform: Any
16:24:34 "
16:24:34 2018-12-17 09:15:02 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] Workspace 'RIP Test Workspace 2018-12-17_09-14-42.6203' was successfully created using template 'Relativity Starter Template'. WorkspaceId=1017501
16:24:34 2018-12-17 09:15:02 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] Checking application '"Integration Points"' ("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C") in workspace '"RIP Test Workspace 2018-12-17_09-14-42.6203"' (1017501).
16:24:34 2018-12-17 09:15:02 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] Importing documents...
16:24:34 2018-12-17 09:15:02 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] TestDir for ImportDocuments '"C:\Jenkins\workspace\s-5HRMYHMWM5\lib\UnitTests"'
16:24:34 2018-12-17 09:15:02 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] Installing application '"Integration Points"' ("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C") in workspace '"RIP Test Workspace 2018-12-17_09-14-42.6203"' (1017501).
16:24:34 2018-12-17 09:16:27 [Information] [kCura.IntegrationPoints.UITests.Configuration.TestContext] Application '"Integration Points"' ("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C") has been installed in workspace '"RIP Test Workspace 2018-12-17_09-14-42.6203"' (1017501) after 24 seconds.
16:24:34 2018-12-17 09:24:32 [Information] [kCura.IntegrationPoints.UITests.Driver.ScreenshotSaver] Saving screenshot: "C:\Jenkins\workspace\s-5HRMYHMWM5\lib\UnitTests\2018-12-17_09-24-32-6800_kCura.IntegrationPoints.UITests.Tests.ExportToLoadFile.ProductionExportToLoadFileTests..png"
```

Exception like this one is logged after a while:
```
16:44:11 1) Error : kCura.IntegrationPoints.UITests.Tests.SelectWithSavedSearchTest.ChangesValueWhenSavedSearchIsChosenInDialog
16:44:11 kCura.IntegrationPoints.UITests.Common.UiTestException : Action timed out.
16:44:11   ----> Polly.Timeout.TimeoutRejectedException : The delegate executed through TimeoutPolicy did not complete within the timeout.
16:44:11   ----> System.OperationCanceledException : The operation was canceled.
16:44:11    at kCura.IntegrationPoints.UITests.Driver.DriverExtensions.ExecuteWithTimeout(Action action, TimeSpan timeout, TimeSpan retryInterval) in S:\Jenkins\workspace\s-5HRMYHMWM5\Source\kCura.IntegrationPoints.UITests\Driver\DriverExtensions.cs:line 115
16:44:11    at kCura.IntegrationPoints.UITests.Driver.DriverExtensions.ClickEx(IWebElement element, Nullable`1 timeout) in S:\Jenkins\workspace\s-5HRMYHMWM5\Source\kCura.IntegrationPoints.UITests\Driver\DriverExtensions.cs:line 30
16:44:11    at kCura.IntegrationPoints.UITests.Pages.IntegrationPointsPage.CreateNewExportIntegrationPoint() in S:\Jenkins\workspace\s-5HRMYHMWM5\Source\kCura.IntegrationPoints.UITests\Pages\IntegrationPointsPage.cs:line 23
16:44:11    at kCura.IntegrationPoints.UITests.Tests.SelectWithSavedSearchTest.ChangesValueWhenSavedSearchIsChosenInDialog() in S:\Jenkins\workspace\s-5HRMYHMWM5\Source\kCura.IntegrationPoints.UITests\Tests\SelectWithSavedSearchTest.cs:line 25
16:44:11 --TimeoutRejectedException
16:44:11    at Polly.Timeout.TimeoutEngine.Implementation[TResult](Func`3 action, Context context, CancellationToken cancellationToken, Func`2 timeoutProvider, TimeoutStrategy timeoutStrategy, Action`3 onTimeout) in C:\projects\polly\src\Polly.Shared\Timeout\TimeoutEngine.cs:line 63
16:44:11    at Polly.Policy.<>c__DisplayClass176_1.<Timeout>b__0(Action`2 action, Context context, CancellationToken cancellationToken) in C:\projects\polly\src\Polly.Shared\Timeout\TimeoutSyntax.cs:line 250
16:44:11    at Polly.Policy.Execute(Action`2 action, Context context, CancellationToken cancellationToken) in C:\projects\polly\src\Polly.Shared\Policy.cs:line 149
16:44:11    at Polly.Wrap.PolicyWrapEngine.Implementation(Action`2 action, Context context, CancellationToken cancellationToken, Policy outerPolicy, Policy innerPolicy) in C:\projects\polly\src\Polly.Shared\Wrap\PolicyWrapEngine.cs:line 46
16:44:11    at Polly.Policy.<>c__DisplayClass225_0.<Wrap>b__0(Action`2 action, Context context, CancellationToken cancellationtoken) in C:\projects\polly\src\Polly.Shared\Wrap\PolicyWrapSyntax.cs:line 20
16:44:11    at Polly.Policy.Execute(Action`2 action, Context context, CancellationToken cancellationToken) in C:\projects\polly\src\Polly.Shared\Policy.cs:line 149
16:44:11    at Polly.Policy.Execute(Action action) in C:\projects\polly\src\Polly.Shared\Policy.cs:line 39
16:44:11    at kCura.IntegrationPoints.UITests.Driver.DriverExtensions.ExecuteWithTimeout(Action action, TimeSpan timeout, TimeSpan retryInterval) in S:\Jenkins\workspace\s-5HRMYHMWM5\Source\kCura.IntegrationPoints.UITests\Driver\DriverExtensions.cs:line 111
```


#### Fix procedure

Check if WebDriver libraries, especially Selenium.WebDriver.ChromeDriver, are up to date.
Version of browser can be found in logs - example above: "Browser version: 71.0.3578.98".
Compatibility of ChromeDriver with different Chrome versions can be checked on https://sites.google.com/a/chromium.org/chromedriver/downloads



### Test fails with "Metadata contains a reference that cannot be resolved"

#### Symptoms

Test fails with message like
```
kCura.Relativity.Client.EndpointInvalidException : Metadata contains a reference that cannot be resolved: 'https://p-dv-vm-pan4pal.kcura.corp.kcura.corp/relativity.services/Authentication.svc?wsdl'.
```

#### Fix procedure

The reason of this error is that API call (like workspace creation request) is hitting wrong URL.
Check if correct URL is set in config file.
