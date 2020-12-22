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

After static code analysis Integration Point and Integration Point Profile page interaction events are similar. They are responsible to display formatted Integration Point details. Only difference exists in couple of fields which are missing in Integration Point Profile:

- Last Runtime UTC
- Job History
- Has Errors

Existing Implementation is based on ``CommonScriptsFactory.cs`` which vary with parameters:

- guidConstants (Integration Point/Integration Point Profile guids)
- fieldsConstants (Integration Point/Integration Point Profile fields)
- apiControllerName (IntegrationPointsAPI / IntegrationPointProfilesAPI)

Under the hood based on the information proper Integration Point model is prepared.

Loaded scripts vary based on Integration Point type (Import/Relativity).

After discussion, we have decided to do the things right and invest into summary page to follow Relativity Forms best practices.

We should get rid off current scripts loading implementation and make the event handler adhere to documentation:

```cs
public abstract class IntegrationPointPageInteractionEventHandle : EventHandler.PageInteractionEventHandler
{
    public override Response PopulateScriptBlocks()
    {
        return new Responce();
    }

    public override string[] ScriptFileNames => new string[] { "integration-point-event-handler.js" }

    public override string [] AdditionalHostedFileNames
    {
        get
        {
            return new string [] {  "...", ... };
        }
    }
}
```

The actual logic for event handling needs to be implemented inside `integration-point-event-handler.js` based on currently existing scripts in Integrations Points, but written from scratch with help of `convenienceAPI`.

```js
(function(eventNames, convenienceApi) {
    var eventHandlers = {};
	 
    eventHandlers[eventNames.PAGE_INTERACTION] = function () {
        // https://platform.relativity.com/RelativityOne/Content/Relativity_Forms/Load_pipeline.htm#replaceO
        // https://platform.relativity.com/RelativityOne/Content/Relativity_Forms/convenienceApi_object.htm#addition
        // ...
    };
	 
    eventHandlers[eventNames.CREATE_CONSOLE] = function () {
        // https://platform.relativity.com/RelativityOne/Content/Relativity_Forms/convenienceApi_object.htm#console
    };
	 
    return eventHandlers
}(eventNames, convenienceApi));
```

## Consequences

- Team will gain huge frontend skills
- Development process could be sprawling in time and product would be operational through this time
- New implementation could be hidden behind a toggle
- It's easier to write code from scratch following documentation than prepare hacks for existing implementation
- Depends on implementation there is chance to improve user experience and page performance
- Entry threshold is high
- We will need to adjust UI tests as rednered UI will be different
- Documentation changes are required

## Issues

- Job ID can be edited in Job History Edit layout.

## Reference Documentation

- <https://einstein.kcura.com/display/DV/Converting+Relativity+Dynamic+Objects+to+Relativity+Forms>
- <https://platform.relativity.com/RelativityOne/Content/Relativity_Forms/Relativity_Forms_API.htm>
