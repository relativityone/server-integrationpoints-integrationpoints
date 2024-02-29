# Investigation of REL-582094

## Status

Proposed

## Context

The current mechanism of Retry Errors in RIP/Sync runs new job in Append/Overlay mode, even if the Integration Point is created in Append Only mode. This might cause confusion for the clients, and in some cases data loss in destination workspace, because documents (or their fields) in destination may have been modified since last job run. The goal of this ADR is to investigate and figure out better ways of handling Retry Errors functionality in RIP/Sync.

**We already display warning** to the user that the job will run in Append/Overlay mode when clicking Retry Errors:

`The retry job will run in Append/Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?`

## Ideas

### Don't change overlay mode

The simplest solution is to leave overlay mode as it is. User will be responsible for resolving all item-level errors related to already existing documents in target workspace. This is safe for us, because users can't blame us anymore for data loss.

### Choose overlay mode

Another possibility is to give users option to select overlay mode after clicking Retry Errors. That way we force users to read the message and they have to select one option. It can be simple two radio buttons, for example:

- `Don't change overlay mode`
- `Retry with Append/Overlay mode`
