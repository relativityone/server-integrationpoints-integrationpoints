# Near-Zero Downtime Architectural Requirements - Backward Compatibility

## Status

Proposed

## Context

Relativity ADS applications must accept a set of architectural requirements and caveats in order to perform a Near-Zero Downtime deployment in Relativity environment. One of such requirements is to gracefully handle server-side calls that change between versions. Examples:
- A customer loads a form rendered on the old version of a Custom Page; the form is then be submitted to the new release of the Custom Page, potentially causing an error unless accounted for in the server code.
- A customer loads a page that contains links to other pages; the new release of the Custom Page removes or changes those links, potentially causing a navigation error unless accounted for in the server code.

## Decision

Two approaches can be considered:
1. Introduce a policy that all changes to public server-side endpoints are always backward compatible.
2. Introduce versioning

First approach has much lower cost of entry (doesn't require any upfront changes) and seems easier to implement, but makes proper design of future functionalities much harder if not impossible. No breaking changes means we can't even introduce a required field in a contract which will prevent us from basic consistency checks.

Second approach requires work upfront (introducing version mechanics into server-side endpoints) but after that gives us complete freedom. The caveat is that naively implemented versioning can easily snowball into multiple duplications which will have high impact on maintenance cost.

Current proposal is to introduce single digit versioning schema in "smart" way, by utilizing ASP.NET MVC routing capabilities. We can introduce a `SupportedVersionsAttribute` which can be applied to controllers and actions in order to provide supported versions range and read it from URL or HTTP headers.

```cs
[SupportedVersions(maxSupportedVersion: 3)]
public class ControllerWithActionsSupportedUpToVersion3 : Controller
{
    ...

    public ActionResult ActionSupportedInAllVersion()
    {
        ...
    }

    [SupportedVersions(maxSupportedVersion: 1)]
    public ActionResult ActionSupportedInV1Only()
    {
        ...
    }

    [SupportedVersions(minSupportedVersion: 2)]
    public ActionResult ActionSupportedFromV2()
    {
        ...
    }
}
```

This approach minimizes duplication as actions have only as many implementations as many breaking changes were in those actions. Of course to minimize the amount of required tests and maintained code, still obsolete versions should be regularly removed.

## Consequences

Application is able to gracefully handle server-side calls that change between versions. The design of new changes becomes a little bit more complicated as clear understanding is required whether change is a breaking or non-breaking one.
