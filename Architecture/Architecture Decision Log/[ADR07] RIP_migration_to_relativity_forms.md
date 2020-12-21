# Integration Points migration to Relativity Forms

## Status

Proposed

## Context

In 2021 Relativity Classic Forms will be deprecated and all related functionalities must be migrated to Liquid Forms (Relativity Forms).

According to current information, this means reworking all customized views for RDOs, while Custom Pages will have separate impact coming from **Custom Pages to k8s project**

## Relativity Forms Overview

_Note:_ Relativity Forms == Liquid Forms

Relativity Forms is page when user clicks on View/Edit on an RDO object inside a list where changes can be made.

Main difference between Classic Forms and Relativity Forms is rendering technology. Classic Forms are based on ASP .Net Web Forms (Server returned entire HTML page on every request/change). Relativity Forms on other hand is get only some statics assets, images, resources and then using endpoints data is retrieved and populated on the page on demand.

Event Handlers which require changes:

- EventHandler.PageInteractionEventHandler
- EventHandler.ConsoleEventHandler

## Affected RIP RDOs

### Integration Point

- Execution Type: **Page Interaction**
- Class Name: **kCura.IntegrationPoints.EventHandlers.IntegrationPoints.IntegrationPointPageInteractionEventHandler**
- Behavior after turning Relativity Forms on:

        All details on General and Scheduling tabs have gone. Instead of it Source Configuration, Destination Configuration, Schedule Rule are displayed as JSON.

- Execution Type: **Console Interaction**
- Class Name: **kCura.IntegrationPoints.EventHandlers.IntegrationPoints.ConsoleEventHandler**
- Behavior after turning Relativity Forms on:

        Right side console have gone completely.

### Integration Point Profile

- Execution Type: **Page Interaction**
- Class Name: **kCura.IntegrationPoints.EventHandlers.IntegrationPoints.IntegrationPointProfilePageInteractionEventHandler**
- Behavior after turning Relativity Forms on:

        All details on General and Scheduling tabs have gone. Instead of it Source Configuration, Destination Configuration, Schedule Rule are displayed as JSON.

_Note:_ There is one more Page Interaction Event Handler in Job History Errors:
_kCura.IntegrationPoints.EventHandlers.IntegrationPoints.JobHistoryErrorPageInteraction_ (Removes ability to edit the Job History field on the Job History Error object), but after testing it turned out that it doesn't do nothing and Relativity Forms cover it out of the box.

All other RDOs objects are as used to be.

## Decision

### Page Interaction

After static code analysis Integration Point and Integration Point Profile page interaction events are similar. They are responsible to display formatted Integration Point details. Only difference exists in couple of fields which are missing in Integration Point Profile:

- Last Runtime UTC
- Job History
- Has Errors

Existing Implementation is based on ``CommonScriptsFactory.cs`` which vary with parameters:

- guidConstants (Integration Point/Integration Point Profile guids)
- fieldsConstants (Integration Point/Integration Point Profile fields)
- apiControllerName (IntegrationPointsAPI / IntegrationPointProfilesAPI)

Under the hood based on the information proper Integration Point model is prepared.

Loaded scripts vary based on Integration Point type (Import/Relativity)

We should get rid off current scripts loading implementation and make following replacement:
```cs
public abstract class PageInteractionEventHandler : EventHandler.PageInteractionEventHandler
```

Should be implemented with following code:

```cs
public abstract class PageInteractionEventHandler : EventHandler.PageInteractionEventHandler
{
    public override Response PopulateScriptBlocks()
    {
        return new Responce();
    }

    public override string[] ScriptFileNames => new string[] { <scripts> }
}
```

_Note:_ There is a small chance that if we would put there all loaded scripts from our current implementation it would work as expected.

### Console Interaction

Console interaction is much more complex. After turning on Relativity Forms it have gone completely. It's cause the ``EventHandler.ConsoleEventHandler`` isn't fired anymore. The console creation should be implemented from scratch using Relativity Forms API. It should be implemented following all Relativity Forms good practices (tests, etc.).

At the end the code should be injected to current Integration Point Summary Page implementation as replacement for existing console.

## Consequences

- until this time RIP Team did only small changes in UI so there is possible lack of knowledge in frontend technologies
- RIP UI implementation has no unit tests on frontend side. Only existing point of true are UI Tests written in C#
- Exsting implementation doesn't follow any Relativity code practices
- There is no CI/CD infrastructure for frontend side. Everything is highly couple with C# code and based on .Net MVC framework.

## Do things right

There is light at the end of the tunnel. All changes needs to be done are on Integration Point/Integration Point Profile summary page. It is well known that UI in RIP is awful and nearly impossible to maintain, develop. We know that RIP as a product would be sunset in few years, but maybe it would be worth to try replace summary page to follow Relativity Forms best practices. Trying to implement Relativity Forms in to existing implementation can cause us a lot of problems especially with regression - without tests there is no way to check if we didn't break anything. We could implement Integration Point summary page in right way following all good practices and at the end move pipeline to new code base.

Pluses:

- Team can gain huge frontend skills without risk to break anything
- Development process could be sprawling in time and product would be operational through this time
- New implementation could be hide behind a toggle
- It's easier to write code from scratch following documentation than prepare hacks for existing implementation
- We could add unit tests
- Depends on implementation there is chance to improve user experience and page performance
- We could get rid off couple UI Tests which testing Summary Page and replace them with unit.

Deltas:

- Whole frontend CI/CD infrastructure needs to be setup
- Entry threshold is high
- Documentation changes are required

## Issues

- Job ID can be edited in Job History Edit layout.

## Reference Documentation

- <https://einstein.kcura.com/display/DV/Converting+Relativity+Dynamic+Objects+to+Relativity+Forms>
- #convert_to_liquid
