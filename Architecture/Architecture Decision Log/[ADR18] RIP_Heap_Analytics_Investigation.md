# Heap Analytics investiogation 

## Status

Proposed

## Context

Customer feedback is one of the most important input for every application improvement. At this stage RIP gets feedback mostly through surveys or incidents. In order to get data about most useful/useless features proactively, user actions on the UI analysis can be used. Very useful and handy tool to reach this requirement is Heap Analytics.

## Getting Started

Heap Analytics
https://heapanalytics.com

Heap Analytics is already implemented solution in R1 and is quite well documented in Einstein:
Heap Access and Configuration on TestVm:
https://einstein.kcura.com/display/DV/4.+Heap+stats
https://einstein.kcura.com/display/LPH/Heap+Dev+Setup+Guide+for+TestVM

## Design 

### AutoCapture

AutoCapture is a fundamental feature for Heap. It automatically captures all user actions on the UI (at least by definition). Those actions are classified as Events and Properties whose are described later in this document. This autocaptured data is organized into the hierarchy account > user > sessions > pageviews > events. Autocapture records all of the following events:

1. View page: when a user view a page on your site or in your production.
2. Click on: when a user clicks on an element on your site or in your product.
3. Field change: when a user changes an input, text area, or selected element on your site or in your product.
4. Submit on: when a user submits a form on your site or in your product.

Events - an event is an interaction that a user has with your website or app. Events are the basic building blocks of charts.
Properties - bits of metadata that are captured during user interaction with app. There are a lot of different properties types where some are linked to user, sessio, events. 

## What's next?
