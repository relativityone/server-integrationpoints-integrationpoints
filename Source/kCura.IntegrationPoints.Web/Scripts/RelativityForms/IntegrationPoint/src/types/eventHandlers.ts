import { EventNames } from "./eventNames";

export type EventHandlers = {
    [K in EventNames]?: void
}