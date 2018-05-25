# 001: Mocking SOAP Call in HealthCheck

## Context
As a part of the post-deployment healthcheck (kCura.IntegrationPoints.Services.IntegrationPointManager class) we decided to check if the  Relativity Web API address is set properly. The address is set by chef scripts in InstanceSetting under section 'kCura.IntegrationPoints' with key 'WebAPIPath'.

We are have to keep code as simple as possible. Simple tasks require simple solutions that should not involve a lot of code to maintain. On the other hand clean code reuires having explicit dependencies (like not having global variables etc.). This concept can be expanded also on dependencies between projects. Another important factor for writing healthckeck is it's durability for changes from external factors like in this case - changes in Relativity SOAP API.

### Concidered approaches

1. Rely on 403
request: GET https://p-dv-vm-fsgjcvh.kcura.corp/Relativitywebapi
response: 403 forbidden

Pros: Very simple
Cons: relying on response could be fragile, we have little information about the state of the service

desc: We know that something is there and responds but we don't know what and if this is what we desire. Relying on the 403 response is dirty and unrliable.

2. Rely on error from web services
request GET https://p-dv-vm-fsgjcvh.kcura.corp/Relativitywebapi/RelativityManager.asmx
response 200 OK but with error in HTML form

Pros: Very simple
Cons: relying on response could be fragile, we have little information about the state of the service, the server returns 200 no matter if the page exists or not, have to rely on hardocoded url path,

Desc: We know that something is there and responds but we don't know what and if it's there. Also the server returns 200 no matter if the page exists or not so we have even fewer information than in 1.
Relying on error is dirty and unrliable.

3. Rely on error from web services
request POST https://p-dv-vm-fsgjcvh.kcura.corp/Relativitywebapi/RelativityManager.asmx without body
response 400 Bad request, no body

Pros: server returns error like page if page does not exists, does not require login
Cons: relying on response could be fragile, we have little information about the state of the service, have to rely on hardocoded url path

Desc: We know that something is there and responds but we don't know what and if it's there. Although server informs that page does not exist we cannot differentiate if page does not exist or there is other error

5. forge check login web service call
request POST https://p-dv-vm-fsgjcvh.kcura.corp/Relativitywebapi/RelativityManager.asmx with login check in soap envelope
response 200 OK with body

Pros: response returns SOAP envelope with proper response on success and 200 OK HTTP, does not require login
Cons: on error (eg. page does not exist) returns 200 OK with html body, have to rely on hardocoded url path, have to send hardoded body

Desc: we would have to parse the response body to check the success, also we realy on the hardcoded body of the request. 
If for any reason namespaces in soap change it would be hard to detect an error. We would create a hard to detect dependency between projects.

6. Forge check getrelatitvityurl web service call
request POST https://p-dv-vm-fsgjcvh.kcura.corp/Relativitywebapi/RelativityManager.asmx with login check in soap envelope
response 200 OK with body

Pros: response returns SOAP envelope with proper response on success and 200 OK HTTP
Cons: on error (eg. page does not exist) returns 200 OK with html body, have to rely on hardocoded url path, have to send hardoded body

Desc: we would have to parse the response body to check the success, better than 3. as it returns relativity address which we could potentially check
If for any reason namespaces in soap change it would be hard to detect an error. We would create a hard to detect dependency between projects.

7. Generate full WS client

Pros: explicit dependency, very reliable
Cons: too much code for a very simple task, would duplicate work done by ImportAPI

Desc: It would reuire having to maintain generated code. Also WS client should be kept in ImportAPI library as well as in IntegrationPoints.

## Decision
We will go with 7. as it balances between all other considered approaches.

## Status
Accepted

## Consequences
Changes in Relativity SOAP API may result in breaking HealthCheck. This shouldn't be an often case especially that we rely on fairly stable API and simple method call. Refreshing reference should be strightforward for experienced and new people as it's supported by visual studio tools.
Downside is that we double the functionality that should probably be exposed by ExportAPI.