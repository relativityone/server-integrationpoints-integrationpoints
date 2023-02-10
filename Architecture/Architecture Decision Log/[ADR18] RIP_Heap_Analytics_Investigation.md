# Heap Analytics investigation 

## Status

Accepted

## Context

Customer feedback is one of the most important input for every application improvement. At this stage RIP gets feedback mostly through surveys incidents or splunk analysis. In order to get data about most useful/useless features proactively, user actions on the UI analysis can be used. Very useful and handy tool to reach this requirement is Heap Analytics.

## Getting Started

Heap Analytics:
* <https://heapanalytics.com>

Heap Analytics is already implemented solution in R1 and is quite well documented in Einstein:
* <https://einstein.kcura.com/display/LPH/Heap+Dev+Setup+Guide+for+TestVM>

Slack channel:
help-click-tracking-heap-support

## Design

### AutoCapture

AutoCapture is a fundamental feature for Heap. It automatically captures all user actions on the UI (at least by definition). Those actions are classified as Events and Properties whose are described later in this document. This autocaptured data is organized in Heap Data Model. 

Events - an event is an interaction that a user has with a website or app. Events are the basic building blocks of charts.

Autocapture records all of the following events:
1. View page: when a user view a page on your site or in your production.
2. Click on: when a user clicks on an element on your site or in your product.
3. Field change: when a user changes an input, text area, or selected element on your site or in your product.
4. Submit on: when a user submits a form on your site or in your product.

Detailed information:
* <https://help.heap.io/definitions/events/events-overview/>

Properties - bits of metadata that are captured during user interaction with app. There are a lot of different properties types where some are casted to Heap Data Model which is described later in this document. Types of properties:
1. Autocaptured properties: automatically captured properties.
2. Custom properties: defined via Heap's APIs or Snapshots.
3. Defined properties: used to create new schema on top of existing properties.

Properties casted on the heap data model:
1. User properties.
2. Session properties.
3. Event properties.

Detailed information:
* <https://help.heap.io/definitions/properties/properties-overview/>

### Heap Data Model

Multiple users can belong to an account. When users visit an app, they conduct sessions during which they do pageviews and events. This results in a hierarchy of account > users > sessions > pageviews and events within Heap. This hierarchy is in fact Heap Data Model.

Detailed information:
* <https://help.heap.io/getting-started/how-heap-works/heaps-data-model/

### Data analysis

In heapanalytics.com, once events are autocaptured it is very handy to use them to create defined events whose are human readable and can be used to create charts for future analysis. There is a plenty of charts that can be created very easily starting from total value of events through Average per user to more sophisticated charts. There are also many template charts.

Once charts are defined they can be used to create dashboards whose are actually final step to start user interraction analysis.

Detailed information:
* <https://help.heap.io/category/charts/

### Heap Api

In special cases Heap Api can be useful and probably the solution for issues described later. Here is the link to implementation description in R1:
* <https://einstein.kcura.com/display/DV/How+to+track+events+using+heap+Api>
* <https://einstein.kcura.com/display/LPH/%5BREL-507123%5D+Heap+Spike+Findings>

## Heap usage in Integration Points

### Configuration

Because `RelativityInternal.aspx` is not used while editing/creating Integration Point `click-tracking.min.js` should be added to `Source\kCura.IntegrationPoints.Web\Views\Shared\_Layout.cshtml` like below:
`<script async type="text/javascript" src="/Relativity/Scripts/click-tracking.min.js"></script>`

This is the only thing to add to autocapture user interactions with Integration Points. In order to test it on TestVm please follow Getting Started section steps.

### Analysis results
 
Most of actions on Integration Points UI are autocaptured and can be used for further analysis. Unfortunatelly not all. Here is a list of actions that are not autocaptured:
1. Actions on step 2 while editing/creating Integration Point.
2. Select type elements.

I assume that we would be able to solve those issues with Heap Api.

The best approach is to create events definitions for further analysis. While creation of new event definitions we should follow naming recommendations described below:
* <https://einstein.kcura.com/pages/viewpage.action?spaceKey=LPH&title=Heap+Event+Definition+Guidelines>

### Actions list

In cooperation with UX team, the list with the most useful actions to analyse was created. Brief implementation analysis of some of them where done during Heap exploration.

List below is created with pattern:
xx. Action - Implementation details

In case of select type elements for which we don't have currently solution only short description in implementation details is added:
`select type issue`

In case of Step 2 issue similar comment is added:
`Step 2 issue`
```
General
1. How often users use tooltips - Autocapture -> Defined event
2. How often Back button in Integration Point Edit/Create page is used - Autocapture -> Defined event
3. Which tabs on the Integration Points navigation bar are mostly used - would need Heap Api implementation as we can't easily read Text value of the tabs.
4. How often users edits Integration Points and Profiles from lists and details view - Autocapture -> defined events -> Chart

Setup step:
Sync:
1. Transferred objects, which types are mostly used - select type issue
2. Profile, how many profiles usually users have - this data should be available in view page event, but for sure it would need additional actions, Defined or Custom properties should help.
3. Log Errors Radio buttons usage comparison - Autocapture -> Defined events for yes and no -> Chart
4. Enable Scheduler Radio buttons usage comparison - Autocapture -> Defined events for yes and no -> Chart

Scheduler:
1. Frequency, options selection comparison - select type issue
2. Times, options selection comparison - select type issue
3. Time ranges - Heap Api is potential solution probably chart would be neeeded

Connect to source - Step 2 issue
1. Source comparison - select type issue
2. Source/Production and other select type elements: which searching option is mostly used, dropdown/search field/more button - select type issue
3. Production Set -> Create button, how often is used - Autocapture -> Defined event
4. Saved Search Radio buttons usage comparison - Autocapture -> Defined events for yes and no -> Chart

Map Fields
1. Map All Fields and Map Saved Search comparison, how often are used per one session - Autocapture -> Defined events for both buttons -> Chart
2. Filter field usage - Autocapture -> Defined events
3. What is average value of clicks to map all fields - Autocapture -> Defined events -> Chart
4. Check if user start configuaration from field mapping or from settings - Autocapture -> Defined events -> Char, I believe it would be complex
5. Overwrite, options selection comparison - select type issue
6. Copy Images Radio buttons usage comparison - Autocapture -> Defined events for yes and no -> Chart
7. Copy Native Files Radio buttons usage comparison - Autocapture -> Defined events for yes and no -> Chart
8. Image/Native sending comparison - Autocapture -> Defined events -> Chart
	
Integration Point Summary Page:
1. How often Schedule tab is used - Autocapture -> Defined events
2. How often Transfer Options buttons are clicked - Autocapture -> Defined events -> Chart
3. How often Edit/Delete/Back/Edit Permissions/View Audit buttons are used - Autocapture -> Defined events -> Chart

Integration Point Profile
1. How often Integration Point Profile is created - Autocapture -> Defined events -> Chart
```

## Next Steps

1. ~~Create Epic for Heap Analytics implementation.~~
2. ~~Create tickets for all points below and link them to Epic from point 1.~~
3. ~~Solve issue with select type elements.~~
4. Solve issue with Step 2.
5. All Actions from `Actions list` need be implemented and grouped into dashboards. At this stage cooperation with UX team will be required.
6. Probably some new actions will be also useful to add - ask UX team. 