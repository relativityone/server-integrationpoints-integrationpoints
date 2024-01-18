# Architecture Evolution Process

The overall Relativity Sync architecture is described on [Einstein](https://einstein.kcura.com/display/DV/Relativity+Sync).

The goal of this process is to have an agile and collaborative way of evolving Relativity Sync architecture and produce artifacts representing made decisions and changes. Every now and then the overall architecture description will be updated with current state based on artifacts created in this process.

Contents:

* [C4 Diagrams](#c4-diagrams)
* [Architecture Decision Log](#architecture-decision-log)

## C4 Diagrams

[C4 model](https://c4model.com/) has been chosen as a way to visualize the architecture going forward.

C4 model doesn't require any specific tooling. Current Relativity standard is Lucidchart and we are going to stick to it. The *C4 Diagrams* folder should contain diagrams exported as SVG and an index file with links to Lucidchart originals. The diagrams should follow below naming convention:

> [TYPE] Diagram Desciption.svg

For example:
* [CONTEXT] ECA Flow.svg
* [CONTAINER] Agents Framework.svg
* [CONTAINER] C4 Framework.svg
* [COMPONENT] Saved Search Pipeline.svg
* [CODE] Metrics.svg

We don't have the core diagrams present, so we need to create context, container, and component diagrams as soon as possible (existing *Pipeline Diagram* is close to component diagram and currently we will be treating it as such). Code diagrams can be created at any team member discretion, whenever there is a feeling that one is needed.

C4 model also provides supplementary diagrams which can be used whenever we trully feel they will provide additional value.

## Architecture Decision Log

The *Architecture Decision Log* folder contains architecture decision records in form of single markdown files. The files names should follow below convention:

> [ADR] Decision Description.md

There is an *[ADR] Decision Template.md* file in the folder which can be copied an edited to start discussion on new decision. The steps to discuss a new decision are as follow:
1. Copy the *[ADR] Decision Template.md* file and rename appropriately.
2. Fill out *Title*, *Context*, and *Consequences* (if known).
3. Set *Status* to *Proposed*.
4. Create a pull request which will be a place of discussion around the decision.
5. During the discussion the file is constantly updated.
6. When the final decision is reached, the pull request is merged.