IF NOT EXIST "%IPSdkOutput%" (
	md "%IPSdkOutput%"
	echo create
)
del /F /S /Q "%IPSdkOutput%\*.*"
xcopy /Y "%SDK_TARGET%\kCura.IntegrationPoints.Contracts.dll" "%IPSdkOutput%"
xcopy /Y "%SDK_TARGET%\kCura.IntegrationPoints.Domain.dll" "%IPSdkOutput%"
xcopy /Y "%SDK_TARGET%\kCura.SourceProviderInstaller.dll" "%IPSdkOutput%"
xcopy /Y "%LDAPSync%\..\example\ProviderIncludes\api\frame-messaging.js" "%IPSdkOutput%"
xcopy /Y "%LDAPSync%\..\example\ProviderIncludes\api\jquery-1.8.2.js" "%IPSdkOutput%"
xcopy /Y "%LDAPSync%\..\example\ProviderIncludes\api\jquery-postMessage.js" "%IPSdkOutput%"