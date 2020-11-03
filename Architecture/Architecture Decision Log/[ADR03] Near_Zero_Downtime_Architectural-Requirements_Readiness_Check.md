# Near-Zero Downtime Architectural Requirements - Readiness Check

## Status

Proposed

## Context

Relativity ADS applications must accept a set of architectural requirements and caveats in order to perform a Near-Zero Downtime deployment in Relativity environment. One of such requirements is to define a Readiness Check URL that prepares the Custom Page to receive traffic. The Readiness Check must adhere to following requirements:
- The Readiness Check should expect an authenticated call using the Relativity Service Account.
- The Readiness Check call will be an HTTP GET request.
- The Readiness Check must return a 200 status code if the warmup was successful. All other status codes will indicate the Readiness Check call failed.
- The Readiness Check must make no assumption about whether the call is made over HTTP or HTTPS.
- The Readiness Check must complete within 30 seconds.
- The Readiness Check may be called multiple times and should be resilient to multiple calls.
- Any content returned by Readiness Check will be ignored.

## Decision

Currently Integration Points has a Health Check (`/Relativity.Rest/api/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration+Point+Health+Check/RunHealthChecksAsync/kCura.IntegrationPoints.Services.IIntegrationPointsModule/Integration+Point+Health+Check/RunHealthChecksAsync`) but it's available as a Kepler. Because of that it is not suitable for Readiness Check as it wont warm up the Custom Page. At the same time checks which this Health Check performs would be ingored by Readiness Check caller.

When it comes to warming the Custom Page, we don't seem to perform any specific operations outside of `Global.asax`. In this context an empty action should be sufficient.

```cs
public class ProbesController : Controller
{
    [HttpGet]
    public HttpStatusCodeResult Readiness()
    {
        return new HttpStatusCodeResult(HttpStatusCode.OK);
    }
}
```

If we identify any specific warm up operation in future we can add them to this action.

We can't ensure 30 seconds time limit from within the action as the running time "outside of our code" is unknown.

## Consequences

Application is able to avoid user-perceptible delays when network traffic switches over to the new application process.
